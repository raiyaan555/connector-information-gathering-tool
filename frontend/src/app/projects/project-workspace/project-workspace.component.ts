import { Component, computed, DestroyRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { debounceTime } from 'rxjs/operators';
import { ProjectService } from '../../services/project.service';
import { NotificationService } from '../../services/notification.service';
import { WorkspaceDraftService } from '../../services/workspace-draft.service';
import { ProjectDocumentRepository } from '../../services/project-document.repository';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { SampleHintComponent } from '../../shared/components/sample-hint/sample-hint.component';
import { ReviewSaveDialogComponent, ReviewSaveDialogSection } from '../../shared/components/review-save-dialog/review-save-dialog.component';
import {
  createCustomerForm,
  LIFECYCLE_OPTIONS,
  APPLICATION_TYPES,
  SOT_ATTRIBUTES,
  ALLOWED_FILE_TYPES,
  WORKSPACE_REQUIRED_KEYS,
  WORKSPACE_SECTIONS,
  FORM_FIELD_HINTS,
} from '../../customer-form/customer-form.model';
import { Project } from '../../models/project.model';

interface WorkspaceFile {
  name: string;
  type: string;
  size: number;
  uploadedAt: Date;
  /**
   * In-memory only (not persisted).
   * Used for preview/download actions before refresh.
   */
  objectUrl?: string;
}

@Component({
  selector: 'app-project-workspace',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    KeyValuePipe,
    DatePipe,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTableModule,
    MatDialogModule,
    MatExpansionModule,
    MatProgressBarModule,
    LoadingSpinnerComponent,
    SampleHintComponent,
  ],
  templateUrl: './project-workspace.component.html',
  styleUrl: './project-workspace.component.scss',
})
export class ProjectWorkspaceComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly notification = inject(NotificationService);
  private readonly draftService = inject(WorkspaceDraftService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly stepper = viewChild<MatStepper>('stepper');
  readonly form = createCustomerForm(this.fb);
  readonly loading = signal(true);
  readonly project = signal<Project | null>(null);
  readonly files = signal<WorkspaceFile[]>([]);
  readonly draftSaved = signal(false);
  readonly fileColumns = ['name', 'type', 'size', 'uploadedAt', 'actions'];

  readonly lifecycleOptions = LIFECYCLE_OPTIONS;
  readonly applicationTypes = APPLICATION_TYPES;
  readonly sotAttributes = SOT_ATTRIBUTES;
  readonly allowedTypes = ALLOWED_FILE_TYPES.join(',');
  readonly sections = WORKSPACE_SECTIONS;
  readonly hints = FORM_FIELD_HINTS;
  readonly moduleDiagramImage = computed(() => {
    const files = this.files();
    const images = files.filter((f) => this.isPngJpegFile(f));
    return images.length ? images[images.length - 1] : null;
  });

  projectId = '';
  private readonly stepControlCache = new Map<string, FormGroup>();

  stepControl(stepKey: 'about' | 'integration' | 'ci' | 'sot' | 'encryption' | 'optional'): FormGroup {
    if (this.stepControlCache.has(stepKey)) return this.stepControlCache.get(stepKey)!;

    // Only validate the controls that belong to the current section.
    const requiredByStep: Record<typeof stepKey, string[]> = {
      about: [
        'applicationPurpose',
        'isSourceOfTruth',
        'hasUatEnvironment',
        'applicationType',
        'connectionMethod',
        'isLegacyApplication',
      ],
      integration: [
        'lifecycleFeatures',
        'userOnboardingRequired',
        'userModificationRequired',
        'userDeletionRequired',
        'userReactivationRequired',
        'ssoRequired',
        'reconStrategy',
      ],
      ci: ['ciPackage', 'ciIntegrationRole'],
      sot: ['sotOnboardingStrategy', 'onboardingScan', 'sotAttributes'],
      encryption: ['apiPayloadEncrypted'],
      optional: [],
    };

    const controls: Record<string, any> = {};
    for (const name of requiredByStep[stepKey]) {
      const ctrl = this.form.get(name);
      if (ctrl) controls[name] = ctrl;
    }

    const group = new FormGroup(controls);
    this.stepControlCache.set(stepKey, group);
    return group;
  }

  readonly completionPercent = computed(() => {
    const data = this.form.getRawValue() as Record<string, unknown>;
    return this.draftService.computeCompletion(data, WORKSPACE_REQUIRED_KEYS);
  });

  readonly lastSavedLabel = computed(() => {
    this.draftService.lastSavedAt();
    return this.draftService.lastSavedLabel();
  });

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.projectService.getById(this.projectId).subscribe({
      next: (res) => {
        if (res.success) {
          this.project.set(res.data);
          this.restoreDraft();
        } else {
          this.notification.error('Project not found');
          this.router.navigate(['/dashboard']);
        }
        this.loading.set(false);
      },
      error: () => {
        this.notification.error('Failed to load project');
        this.router.navigate(['/dashboard']);
      },
    });

    this.form.valueChanges.pipe(debounceTime(800), takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.saveDraft(false);
    });

    // If navigated from the document workspace (e.g. "Edit Section"), jump directly to that step.
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const sectionKey = params['section'] as string | undefined;
      if (!sectionKey) return;
      const section = WORKSPACE_SECTIONS.find((s) => s.key === sectionKey);
      if (!section) return;
      // stepper view child may not be available in the same tick.
      setTimeout(() => this.goToStep(section.step), 0);
    });
  }

  restoreDraft(): void {
    const draft = this.draftService.load(this.projectId);
    if (!draft) return;
    this.form.patchValue(draft.formData);
    this.files.set(
      draft.attachments.map((f) => ({
        ...f,
        uploadedAt: new Date(f.uploadedAt),
      })),
    );
    // A loaded draft exists; show the saved badge.
    this.draftSaved.set(true);
  }

  saveDraft(showToast = true): void {
    const attachments = this.files().map((f) => ({
      name: f.name,
      type: f.type,
      size: f.size,
      uploadedAt: f.uploadedAt.toISOString(),
    }));
    this.draftService.save(
      this.projectId,
      this.form.getRawValue() as Record<string, unknown>,
      attachments,
      this.completionPercent(),
    );
    // Only show "Draft Saved" checkmark after an explicit manual save.
    this.draftSaved.set(showToast);
    if (showToast) this.notification.success('Draft saved');
  }

  onLifecycleChange(feature: string, checked: boolean): void {
    const current = [...(this.form.get('lifecycleFeatures')?.value || [])];
    if (checked && !current.includes(feature)) current.push(feature);
    else if (!checked) current.splice(current.indexOf(feature), 1);
    this.form.patchValue({ lifecycleFeatures: current });
  }

  isLifecycleSelected(feature: string): boolean {
    return (this.form.get('lifecycleFeatures')?.value || []).includes(feature);
  }

  onSotAttributeChange(attr: string, checked: boolean): void {
    const current = [...(this.form.get('sotAttributes')?.value || [])];
    if (checked && !current.includes(attr)) current.push(attr);
    else if (!checked) current.splice(current.indexOf(attr), 1);
    this.form.patchValue({ sotAttributes: current });
  }

  isSotAttributeSelected(attr: string): boolean {
    return (this.form.get('sotAttributes')?.value || []).includes(attr);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const selected = Array.from(input.files);
    this.files.update((existing) => [
      ...existing,
      ...selected.map((file) => ({
        name: file.name,
        type: file.type || 'application/octet-stream',
        size: file.size,
        uploadedAt: new Date(),
        objectUrl: URL.createObjectURL(file),
      })),
    ]);
    this.saveDraft(false);
    this.notification.success(`${selected.length} file(s) added`);
    input.value = '';
  }

  onModuleDiagramSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    this.files.update((existing) => [
      ...existing,
      {
        name: file.name,
        type: file.type || 'image/png',
        size: file.size,
        uploadedAt: new Date(),
        objectUrl: URL.createObjectURL(file),
      },
    ]);
    this.saveDraft(false);
    this.notification.success('Module diagram uploaded');
    input.value = '';
  }

  private isPngJpegFile(f: WorkspaceFile): boolean {
    const t = (f.type || '').toLowerCase();
    const n = (f.name || '').toLowerCase();
    return t.includes('image') && (t.includes('png') || t.includes('jpeg') || t.includes('jpg')) ? true : n.endsWith('.png') || n.endsWith('.jpg') || n.endsWith('.jpeg');
  }

  removeFile(index: number): void {
    this.files.update((files) => {
      const removed = files[index];
      if (removed?.objectUrl) URL.revokeObjectURL(removed.objectUrl);
      return files.filter((_, i) => i !== index);
    });
    this.saveDraft(false);
  }

  removeFileByReference(file: WorkspaceFile): void {
    const idx = this.files().findIndex((f) => f === file);
    if (idx < 0) return;
    this.removeFile(idx);
  }

  previewFile(file: WorkspaceFile): void {
    if (!file.objectUrl) {
      this.notification.info('Preview is available only for newly uploaded files in this session.');
      return;
    }
    window.open(file.objectUrl, '_blank', 'noopener,noreferrer');
  }

  downloadFile(file: WorkspaceFile): void {
    if (!file.objectUrl) {
      this.notification.info('Download is available only for newly uploaded files in this session.');
      return;
    }
    const a = document.createElement('a');
    a.href = file.objectUrl;
    a.download = file.name;
    a.rel = 'noopener';
    a.click();
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  goToStep(step: number): void {
    const s = this.stepper();
    if (s) s.selectedIndex = step;
  }

  generatePpt(): void {
    // Ensure the latest draft state is persisted before generation placeholders run.
    this.saveDraft(false);
    this.notification.info('PowerPoint generation will be available in a future release.');
  }

  generateWord(): void {
    this.saveDraft(false);
    this.notification.info('Word document generation will be available in a future release.');
  }

  generatePdf(): void {
    this.saveDraft(false);
    this.notification.info('PDF generation will be available in a future release.');
  }

  submitWorkspace(): void {
    // Backward compat: keep old method name (in case it is referenced elsewhere).
    this.reviewAndSave();
  }

  private readonly docRepo = inject(ProjectDocumentRepository);

  reviewAndSave(): void {
    const formData = this.form.getRawValue() as Record<string, unknown>;

    const controlLabels: Record<string, string> = {
      applicationPurpose: 'What does this application do?',
      isSourceOfTruth: 'Will this application be the Source of Truth (SOT)?',
      hasUatEnvironment: 'Do we have the UAT environment to build/test/freeze the connectors?',
      uatServer: 'server',
      uatUsername: 'username',
      uatPassword: 'password',
      applicationType: 'What type of application is it?',
      connectionMethod: 'How do we connect to this application?',
      isLegacyApplication: 'Is this a legacy application or web application?',
      legacyDetails: 'Legacy / Client Details',

      lifecycleFeatures: 'Which lifecycle management features are required?',
      userOnboardingRequired: 'Is user required to be on-boarded on the application?',
      userOnboardingDetails: 'On-boarding Details',
      userModificationRequired: 'Is user required to be modified on the application?',
      userModificationDetails: 'Modification Details',
      userDeletionRequired: 'Is user deletion required on the application?',
      userDeletionDetails: 'Deletion Details',
      deleteType: 'Is the removal of user a soft delete or hard delete?',
      userReactivationRequired: 'Is the user required to be reactivated?',
      reactivationMethod: 'Reactivation Method',
      ssoRequired: 'Will there be SSO?',
      ssoType: 'What type of SSO will be used for this application?',
      reconStrategy: 'What is the recon strategy that will be used for this application?',
      defaultEntitlement: 'While creating a user, does that user need to be assigned to some default entitlement?',
      reconUserTypes: 'While reconciliation are the active users & the disable users coming in the same request?',
      entitlementTypes: 'Will the user be assigned to multiple types of entitlements or only one type?',

      ciPackage: 'Which CI Package will be getting implemented?',
      ciIntegrationRole: 'How will it relate to the CI once integrated?',
      moduleDiagramNotes: 'Module Diagram of integration of this application with CI',

      sotOnboardingStrategy: 'What is the SOT on-boarding strategy that will be used?',
      onboardingScan: 'What is the on-boarding scan that will be configured?',
      sotAttributes: 'What are the attributes of the SOT that will be used for this application?',
      additionalSotAttributes: 'Additional SOT Attributes',

      encryptedFields: 'Which fields of the user details are encrypted?',
      apiPayloadEncrypted: 'Are the api payloads encrypted?',
      encodedFields: 'Which fields are encoded?',
      encryptionAlgorithm: 'Is there any specific standard encryption algorithm used?',

      apiDocumentationLink: 'Attach the api documentation for the collection',
      specialComments: 'Special Comments (If Any)',
    };

    const formatValue = (v: unknown): string => {
      if (Array.isArray(v)) return v.filter(Boolean).join(', ');
      if (v === null || v === undefined) return '';
      if (typeof v === 'string') return v.trim();
      return String(v);
    };

    const toItems = (keys: string[]): ReviewSaveDialogSection['items'] => {
      return keys
        .map((k) => ({
          label: controlLabels[k] ?? k,
          value: formatValue(formData[k]),
        }))
        .filter((x) => x.value);
    };

    const sections: ReviewSaveDialogSection[] = [
      {
        title: 'About Application',
        items: toItems([
          'applicationPurpose',
          'isSourceOfTruth',
          'hasUatEnvironment',
          'uatServer',
          'uatUsername',
          'uatPassword',
          'applicationType',
          'connectionMethod',
          'isLegacyApplication',
          'legacyDetails',
        ]),
      },
      {
        title: 'Application Integration',
        items: toItems([
          'lifecycleFeatures',
          'userOnboardingRequired',
          'userOnboardingDetails',
          'userModificationRequired',
          'userModificationDetails',
          'userDeletionRequired',
          'userDeletionDetails',
          'deleteType',
          'userReactivationRequired',
          'reactivationMethod',
          'ssoRequired',
          'ssoType',
          'reconStrategy',
          'defaultEntitlement',
          'reconUserTypes',
          'entitlementTypes',
        ]),
      },
      {
        title: 'Converged Identity',
        items: toItems(['ciPackage', 'ciIntegrationRole', 'moduleDiagramNotes']),
      },
      {
        title: 'Source Of Truth',
        items: toItems(['sotOnboardingStrategy', 'onboardingScan', 'sotAttributes', 'additionalSotAttributes']),
      },
      {
        title: 'Encryption',
        items: toItems(['encryptedFields', 'apiPayloadEncrypted', 'encodedFields', 'encryptionAlgorithm']),
      },
      {
        title: 'General Information',
        items: toItems(['apiDocumentationLink']),
      },
      {
        title: 'Special Comments',
        items: toItems(['specialComments']),
      },
    ];

    const attachments = this.files().map((f) => f.name);
    const existingVersions = this.docRepo.getVersions(this.projectId);
    const isChangeRequest = existingVersions.length > 0;

    const ref = this.dialog.open(ReviewSaveDialogComponent, {
      width: '980px',
      data: {
        title: 'Review Connector Information Gathering',
        sections,
        attachments,
        note: isChangeRequest
          ? 'You are editing an approved Connector Information Gathering Document. Saving these changes will create a new version as a Change Request.'
          : 'Once you click Review & Save, this becomes the official Version 1 of the Connector Information Gathering Document.',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      // Persist the latest draft right before committing the official version.
      const attachmentsMeta = this.files().map((f) => ({
        name: f.name,
        type: f.type,
        size: f.size,
        uploadedAt: f.uploadedAt.toISOString(),
      }));

      const completionPercent = this.completionPercent();
      this.draftService.save(this.projectId, formData, attachmentsMeta, completionPercent);

      const version = this.docRepo.saveVersion({
        projectId: this.projectId,
        formData,
        attachments: attachmentsMeta,
        completionPercent,
      });

      // Once the official save is done and completion is 100%, remove the draft from the app.
      if (completionPercent >= 100) {
        this.draftService.clear(this.projectId);
      }

      this.notification.success(`Saved Version ${version.versionNumber} (${version.saveType})`);
      this.router.navigate(['/project', this.projectId]);
    });
  }
}
