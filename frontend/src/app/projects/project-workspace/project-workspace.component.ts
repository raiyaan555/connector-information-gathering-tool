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
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { debounceTime } from 'rxjs/operators';
import { firstValueFrom } from 'rxjs';
import { ProjectService } from '../../services/project.service';
import { NotificationService } from '../../services/notification.service';
import { WorkspaceDraftService } from '../../services/workspace-draft.service';
import { AttachmentService } from '../../services/attachment.service';
import { PdfService } from '../../services/pdf.service';
import { ProjectDocumentRepository } from '../../services/project-document.repository';
import { AuthService } from '../../services/auth.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { SampleHintComponent } from '../../shared/components/sample-hint/sample-hint.component';
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
  objectUrl?: string;
  /** In-session File reference for uploading bytes to the API. */
  file?: File;
  /** Server attachment id after successful upload. */
  serverId?: string;
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
  private readonly attachmentService = inject(AttachmentService);
  private readonly pdfService = inject(PdfService);
  private readonly docRepo = inject(ProjectDocumentRepository);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly stepper = viewChild<MatStepper>('stepper');
  readonly form = createCustomerForm(this.fb);
  readonly loading = signal(true);
  readonly project = signal<Project | null>(null);
  readonly files = signal<WorkspaceFile[]>([]);
  readonly draftSaved = signal(false);
  readonly generatingPdf = signal(false);
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
    if (draft) {
      this.form.patchValue(draft.formData);
      this.files.set(
        draft.attachments.map((f) => ({
          ...f,
          uploadedAt: new Date(f.uploadedAt),
        })),
      );
      this.draftSaved.set(true);
      return;
    }

    // Editing a completed project: pre-populate from the latest official version.
    const latest = this.docRepo.getLatestVersion(this.projectId);
    if (!latest) return;
    this.form.patchValue(latest.formData);
    this.files.set(
      latest.attachments.map((f) => ({
        ...f,
        uploadedAt: new Date(f.uploadedAt),
      })),
    );
  }

  hasOfficialVersion(): boolean {
    return this.docRepo.hasOfficialVersion(this.projectId);
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
    void this.addAndUploadFiles(selected);
    input.value = '';
  }

  onModuleDiagramSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    void this.addAndUploadFiles([file], 'Module diagram uploaded');
    input.value = '';
  }

  private async addAndUploadFiles(selected: File[], successMessage?: string): Promise<void> {
    const mapped = selected.map((file) => ({
      name: file.name,
      type: file.type || 'application/octet-stream',
      size: file.size,
      uploadedAt: new Date(),
      objectUrl: URL.createObjectURL(file),
      file,
    }));
    this.files.update((existing) => [...existing, ...mapped]);
    this.saveDraft(false);

    for (const item of mapped) {
      if (!item.file) continue;
      try {
        const res = await firstValueFrom(this.attachmentService.uploadFile(this.projectId, item.file));
        if (res.success && res.data) {
          this.files.update((list) =>
            list.map((f) => (f === item || (f.name === item.name && f.size === item.size && !f.serverId)
              ? { ...f, serverId: res.data!.id }
              : f)),
          );
        }
      } catch {
        this.notification.error(`Failed to upload ${item.name}. It may be missing from the PDF.`);
      }
    }

    this.notification.success(successMessage ?? `${selected.length} file(s) added`);
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

  generatePdf(): void {
    void this.runGeneratePdf();
  }

  private async runGeneratePdf(): Promise<void> {
    this.saveDraft(false);
    this.generatingPdf.set(true);

    try {
      for (const item of this.files()) {
        if (item.serverId || !item.file) continue;
        try {
          const res = await firstValueFrom(this.attachmentService.uploadFile(this.projectId, item.file));
          if (res.success && res.data) {
            this.files.update((list) =>
              list.map((f) => (f === item ? { ...f, serverId: res.data!.id } : f)),
            );
          }
        } catch {
          this.notification.error(`Failed to upload ${item.name}`);
        }
      }

      const rawForm = this.form.getRawValue() as Record<string, unknown>;
      const attachmentsMeta = this.files().map((f) => ({
        name: f.name,
        type: f.type,
        size: f.size,
        uploadedAt: f.uploadedAt.toISOString(),
      }));
      const completionPercent = this.completionPercent();
      const user = this.authService.user();
      const changedBy = user?.fullName || user?.email || 'Unknown';

      const version = this.docRepo.saveVersion({
        projectId: this.projectId,
        formData: rawForm,
        attachments: attachmentsMeta,
        completionPercent,
        changedBy,
      });

      this.docRepo.addGeneratedDocument({
        projectId: this.projectId,
        versionNumber: version.versionNumber,
        docType: 'pdf',
        fileName: `CIGT_v${version.versionNumber}.pdf`,
        includedAttachmentNames: attachmentsMeta.map((a) => a.name),
      });

      const project = this.project();
      if (project) {
        try {
          await firstValueFrom(
            this.projectService.update(this.projectId, {
              name: project.name,
              clientName: project.clientName,
              applicationName: project.applicationName,
              status: 'Completed',
            }),
          );
        } catch {
          // Version/PDF still succeed even if status update fails.
        }
      }

      const formData = this.toStringMap(rawForm);
      const blob = await firstValueFrom(this.pdfService.generatePdf(this.projectId, formData));
      const fileName = `CIGT_${project?.clientName || 'Client'}_${project?.applicationName || 'Application'}_v${version.versionNumber}.pdf`.replace(
        /[^\w.\-]+/g,
        '_',
      );
      this.downloadBlob(blob, fileName);

      if (completionPercent >= 100) {
        this.draftService.clear(this.projectId);
      }

      const label =
        version.saveType === 'Official'
          ? 'Version 1 saved. Redirecting to review…'
          : `${version.changeRequestId} (Version ${version.versionNumber}) saved. Redirecting to review…`;
      this.notification.success(label);
      await this.router.navigate(['/project', this.projectId]);
    } catch {
      this.notification.error('PDF generation failed. Please try again.');
    } finally {
      this.generatingPdf.set(false);
    }
  }

  private toStringMap(raw: Record<string, unknown>): Record<string, string> {
    const result: Record<string, string> = {};
    for (const [key, value] of Object.entries(raw)) {
      if (value === null || value === undefined) continue;
      if (Array.isArray(value)) {
        const joined = value.filter(Boolean).map(String).join(', ');
        if (joined) result[key] = joined;
        continue;
      }
      const text = String(value).trim();
      if (text) result[key] = text;
    }
    return result;
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.rel = 'noopener';
    a.click();
    URL.revokeObjectURL(url);
  }
}
