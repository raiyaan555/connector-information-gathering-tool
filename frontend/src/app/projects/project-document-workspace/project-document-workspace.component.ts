import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DatePipe } from '@angular/common';

import { ProjectService } from '../../services/project.service';
import { NotificationService } from '../../services/notification.service';
import { WorkspaceDraftService } from '../../services/workspace-draft.service';
import { ProjectDocumentRepository, DocumentGenerationOptions, DocType, AttachmentMeta, GeneratedDocumentEntry, ProjectVersionEntry } from '../../services/project-document.repository';
import { Project } from '../../models/project.model';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

type SectionKey = 'about' | 'integration' | 'ci' | 'sot' | 'encryption' | 'general' | 'comments' | 'attachments' | 'review';

const CONTROL_LABELS: Record<string, string> = {
  applicationPurpose: 'What does this application do?',
  isSourceOfTruth: 'Will this application be the Source of Truth (SOT)?',
  hasUatEnvironment: 'Do we have the UAT environment to build/test/freeze the connectors?',
  uatServer: 'Server',
  uatUsername: 'Username',
  uatPassword: 'Password',
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

const SECTION_DEFS: Array<{ key: SectionKey; title: string; controls: string[]; step: number }> = [
  {
    key: 'about',
    title: 'About Application',
    controls: [
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
    ],
    step: 0,
  },
  {
    key: 'integration',
    title: 'Application Integration',
    controls: [
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
    ],
    step: 1,
  },
  {
    key: 'ci',
    title: 'Converged Identity',
    controls: ['ciPackage', 'ciIntegrationRole', 'moduleDiagramNotes'],
    step: 2,
  },
  {
    key: 'sot',
    title: 'Source Of Truth',
    controls: ['sotOnboardingStrategy', 'onboardingScan', 'sotAttributes', 'additionalSotAttributes'],
    step: 3,
  },
  {
    key: 'encryption',
    title: 'Encryption',
    controls: ['encryptedFields', 'apiPayloadEncrypted', 'encodedFields', 'encryptionAlgorithm'],
    step: 4,
  },
  {
    key: 'general',
    title: 'General Information',
    controls: ['apiDocumentationLink'],
    step: 5,
  },
  {
    key: 'comments',
    title: 'Special Comments',
    controls: ['specialComments'],
    step: 6,
  },
  {
    key: 'attachments',
    title: 'Attachments',
    controls: [],
    step: 7,
  },
  {
    key: 'review',
    title: 'Review',
    controls: [],
    step: 8,
  },
];

@Component({
  selector: 'app-project-document-workspace',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatExpansionModule, MatButtonModule, MatIconModule, MatCheckboxModule, MatTableModule, MatDialogModule, MatProgressBarModule, DatePipe],
  templateUrl: './project-document-workspace.component.html',
  styleUrl: './project-document-workspace.component.scss',
})
export class ProjectDocumentWorkspaceComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  readonly notification = inject(NotificationService);
  readonly draftService = inject(WorkspaceDraftService);
  private readonly docRepo = inject(ProjectDocumentRepository);
  private readonly dialog = inject(MatDialog);

  readonly projectId = this.route.snapshot.paramMap.get('id')!;

  readonly project = signal<Project | null>(null);
  readonly versions = signal<ProjectVersionEntry[]>([]);

  readonly draftSnapshot = signal<{ formData: Record<string, unknown>; attachments: AttachmentMeta[]; completionPercent: number } | null>(null);

  readonly selectedVersionNumber = signal<number | null>(null);

  private readonly generatedDocsRefreshKey = signal(0);

  readonly options = signal<DocumentGenerationOptions>({
    includeConnectorForm: true,
    includeUploadedApiDocumentation: true,
    includeSwaggerFiles: true,
    includePostmanCollection: true,
    includeArchitectureDiagrams: true,
    includeScreenshots: true,
    includeCredentialsDocument: true,
    includeAdditionalUploadedFiles: true,
  });

  readonly docs = computed(() => {
    // LocalStorage updates are not reactive by themselves, so we depend on this key.
    this.generatedDocsRefreshKey();
    const v = this.selectedVersionNumber();
    return this.docRepo.listGeneratedDocuments(this.projectId, v ?? undefined);
  });

  readonly snapshot = computed(() => {
    const v = this.selectedVersionNumber();
    if (v == null) return this.draftSnapshot();
    const version = this.versions().find((x) => x.versionNumber === v);
    if (!version) return null;
    return { formData: version.formData, attachments: version.attachments, completionPercent: version.completionPercent };
  });

  readonly lastDraftSavedLabel = computed(() => this.draftService.lastSavedLabel());
  readonly sectionDefs = SECTION_DEFS;

  ngOnInit(): void {
    this.projectService.getById(this.projectId).subscribe({
      next: (res) => {
        if (res.success) this.project.set(res.data);
      },
    });

    this.draftService.load(this.projectId); // updates lastSavedLabel()
    const draft = this.draftService.load(this.projectId);
    if (draft) {
      this.draftSnapshot.set({
        formData: draft.formData,
        attachments: draft.attachments,
        completionPercent: draft.completionPercent,
      });
    }

    const versions = this.docRepo.getVersions(this.projectId);
    this.versions.set(versions);
    // Versions are ordered latest-first in the repository.
    this.selectedVersionNumber.set(versions.length ? versions[0].versionNumber : null);
  }

  completionPercent(): number {
    const snap = this.snapshot();
    return snap?.completionPercent ?? 0;
  }

  lastOfficialSaveLabel(): string {
    const latest = this.docRepo.getLatestVersion(this.projectId);
    if (!latest) return 'Not saved yet';
    return new Date(latest.createdAt).toLocaleString();
  }

  editSection(sectionKey: SectionKey): void {
    this.router.navigate(['/project', this.projectId, 'edit'], { queryParams: { section: sectionKey } });
  }

  previewDocument(doc: GeneratedDocumentEntry): void {
    const lines: string[] = [];
    lines.push(`Preview (placeholder)`);
    lines.push(`File: ${doc.fileName}`);
    lines.push(`Generated: ${new Date(doc.createdAt).toLocaleString()}`);
    lines.push('');
    lines.push(`Options:`);
    lines.push(JSON.stringify(doc.options, null, 2));
    lines.push('');
    lines.push(`Included attachments (${doc.includedAttachmentNames.length}):`);
    lines.push(doc.includedAttachmentNames.join(', '));
    const blob = new Blob([lines.join('\n')], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank', 'noopener,noreferrer');
    setTimeout(() => URL.revokeObjectURL(url), 60_000);
  }

  downloadDocument(doc: GeneratedDocumentEntry): void {
    const content = [
      `ConnectorInformation placeholder`,
      `File: ${doc.fileName}`,
      `Version: ${doc.versionNumber}`,
      `Generated: ${new Date(doc.createdAt).toISOString()}`,
      '',
      'Options:',
      JSON.stringify(doc.options, null, 2),
      '',
      'Included attachments:',
      doc.includedAttachmentNames.join(', '),
    ].join('\n');
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = doc.fileName.replace(/\.(pdf|docx|pptx)$/i, '.txt');
    a.rel = 'noopener';
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 20_000);
  }

  deleteDocument(doc: GeneratedDocumentEntry): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete Generated Document',
        message: `Are you sure you want to delete ${doc.fileName}?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      const ok = this.docRepo.deleteGeneratedDocument(this.projectId, doc.id);
      if (ok) {
        this.generatedDocsRefreshKey.update((n) => n + 1);
        this.notification.success('Document deleted');
      }
    });
  }

  regenerateDocument(doc: GeneratedDocumentEntry): void {
    const snap = this.snapshot();
    if (!snap) {
      this.notification.error('No snapshot available to regenerate.');
      return;
    }

    const fileExt = doc.docType === 'pdf' ? 'pdf' : doc.docType === 'word' ? 'docx' : 'pptx';
    const regenSuffix = `regen_${Date.now()}`;
    const fileName = doc.fileName.replace(new RegExp(`\\.${fileExt}$`, 'i'), `_${regenSuffix}.${fileExt}`);

    this.docRepo.addGeneratedDocument({
      projectId: this.projectId,
      versionNumber: doc.versionNumber,
      docType: doc.docType,
      fileName,
      options: doc.options,
      includedAttachmentNames: doc.includedAttachmentNames,
    });

    this.generatedDocsRefreshKey.update((n) => n + 1);
    this.notification.success('Document regenerated (placeholder)');
  }

  private activeVersionNumberOrNull(): number | null {
    const v = this.selectedVersionNumber();
    if (v != null) return v;
    return null;
  }

  generateDocument(docType: DocType): void {
    const versions = this.versions();
    const selected = this.activeVersionNumberOrNull() ?? (versions.length ? versions[versions.length - 1].versionNumber : null);
    if (selected == null) {
      this.notification.error('Please do Review & Save first to create an official version.');
      return;
    }

    const options = this.options();
    const snap = this.snapshot();
    if (!snap) {
      this.notification.error('No draft/version snapshot available.');
      return;
    }

    const fileExt = docType === 'pdf' ? 'pdf' : docType === 'word' ? 'docx' : 'pptx';
    const fileName = `ConnectorInformation_v${selected}.${fileExt}`;
    this.docRepo.addGeneratedDocument({
      projectId: this.projectId,
      versionNumber: selected,
      docType,
      fileName,
      options,
      includedAttachmentNames: snap.attachments.map((a) => a.name),
    });

    this.generatedDocsRefreshKey.update((n) => n + 1);
    this.notification.success('Document generated (placeholder)');
  }

  setDocOption<K extends keyof DocumentGenerationOptions>(key: K, value: boolean): void {
    this.options.update((o) => ({ ...o, [key]: value }));
  }

  setSelectedVersion(v: number): void {
    this.selectedVersionNumber.set(v);
  }

  sectionValue(sectionControls: string[], formData: Record<string, unknown>): Array<{ label: string; value: unknown }> {
    return sectionControls
      .map((key) => ({
        label: CONTROL_LABELS[key] ?? key,
        value: formData[key],
      }))
      .filter((x) => x.value !== undefined && x.value !== null && String(x.value).trim() !== '');
  }
}

