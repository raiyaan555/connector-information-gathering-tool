import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';

import { ProjectService } from '../../services/project.service';
import { NotificationService } from '../../services/notification.service';
import { WorkspaceDraftService } from '../../services/workspace-draft.service';
import {
  ProjectDocumentRepository,
  AttachmentMeta,
  ProjectVersionEntry,
} from '../../services/project-document.repository';
import { cigtDownloadName, PdfService } from '../../services/pdf.service';
import { Project } from '../../models/project.model';
import { JiraTicketDialogComponent, jiraTicketHref, jiraTicketLabel } from './jira-ticket-dialog.component';
import { ShareConnectorDialogComponent } from './share-connector-dialog.component';

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
  imports: [
    RouterLink,
    MatCardModule,
    MatExpansionModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressBarModule,
    DatePipe,
  ],
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
  private readonly pdfService = inject(PdfService);
  private readonly dialog = inject(MatDialog);

  readonly projectId = this.route.snapshot.paramMap.get('id')!;

  readonly project = signal<Project | null>(null);
  readonly versions = signal<ProjectVersionEntry[]>([]);
  readonly draftSnapshot = signal<{
    formData: Record<string, unknown>;
    attachments: AttachmentMeta[];
    completionPercent: number;
  } | null>(null);
  readonly selectedVersionNumber = signal<number | null>(null);
  readonly downloadingPdf = signal(false);
  readonly sharingEmail = signal(false);
  readonly savingJira = signal(false);
  readonly jiraTicket = computed(() => this.project()?.jiraTicket?.trim() || '');
  readonly jiraHref = computed(() => jiraTicketHref(this.jiraTicket()));
  readonly jiraLabel = computed(() => jiraTicketLabel(this.jiraTicket()));

  readonly sectionDefs = SECTION_DEFS;

  readonly snapshot = computed(() => {
    const v = this.selectedVersionNumber();
    if (v == null) return this.draftSnapshot();
    const version = this.versions().find((x) => x.versionNumber === v);
    if (!version) return this.draftSnapshot();
    return {
      formData: version.formData,
      attachments: version.attachments,
      completionPercent: version.completionPercent,
    };
  });

  readonly includesJson = computed(() =>
    (this.snapshot()?.attachments ?? []).some((file) => file.name.trim().toLowerCase().endsWith('.json')),
  );

  readonly isReadOnlyVersion = computed(() => {
    const versions = this.versions();
    const selected = this.selectedVersionNumber();
    if (!versions.length || selected == null) return false;
    return selected !== versions[0].versionNumber;
  });

  ngOnInit(): void {
    this.projectService.getById(this.projectId).subscribe({
      next: (res) => {
        if (res.success) this.project.set(res.data);
      },
    });

    this.draftService.load(this.projectId);
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
    this.selectedVersionNumber.set(versions.length ? versions[0].versionNumber : null);

    // Drafts with no official version go straight to the form.
    if (!versions.length) {
      this.router.navigate(['/project', this.projectId, 'edit'], { replaceUrl: true });
    }
  }

  hasCompletedVersion(): boolean {
    return this.versions().length > 0;
  }

  completionPercent(): number {
    return this.snapshot()?.completionPercent ?? 0;
  }

  lastOfficialSaveLabel(): string {
    const latest = this.docRepo.getLatestVersion(this.projectId);
    if (!latest) return 'Not saved yet';
    return new Date(latest.createdAt).toLocaleString();
  }

  editProject(): void {
    this.editSection('review');
  }

  editSection(sectionKey: SectionKey): void {
    if (this.isReadOnlyVersion()) {
      this.notification.info('Switch to the active version before editing.');
      return;
    }
    this.router.navigate(['/project', this.projectId, 'edit'], { queryParams: { section: sectionKey } });
  }

  setSelectedVersion(v: number): void {
    this.selectedVersionNumber.set(v);
  }

  selectActiveVersion(): void {
    const latest = this.versions()[0];
    if (latest) this.selectedVersionNumber.set(latest.versionNumber);
  }

  async downloadPdf(): Promise<void> {
    const snap = this.snapshot();
    if (!snap) {
      this.notification.error('No saved information available to generate a PDF.');
      return;
    }

    this.downloadingPdf.set(true);
    try {
      const formData = this.toStringMap(snap.formData);
      const response = await firstValueFrom(this.pdfService.generatePdf(this.projectId, formData));
      const blob = response.body;
      if (!blob) throw new Error('Empty download');
      const project = this.project();
      const fileName = cigtDownloadName(
        response,
        project?.clientName || 'Client',
        project?.applicationName || 'Application',
      );
      this.downloadBlob(blob, fileName);

      const versionNumber = this.selectedVersionNumber() ?? this.versions()[0]?.versionNumber;
      if (versionNumber != null) {
        this.docRepo.addGeneratedDocument({
          projectId: this.projectId,
          versionNumber,
          docType: 'pdf',
          fileName,
          includedAttachmentNames: snap.attachments.map((a) => a.name),
        });
      }

      this.notification.success(
        fileName.toLowerCase().endsWith('.zip')
          ? 'CIGT zip downloaded. It contains the PDF and the JSON files.'
          : 'PDF downloaded',
      );
    } catch {
      this.notification.error('PDF download failed. Please try again.');
    } finally {
      this.downloadingPdf.set(false);
    }
  }

  async attachJiraTicket(): Promise<void> {
    if (this.isReadOnlyVersion()) {
      this.notification.info('Switch to the active version to attach a Jira ticket.');
      return;
    }

    const result = await firstValueFrom(
      this.dialog.open(JiraTicketDialogComponent, {
        data: { initialValue: this.jiraTicket() },
        width: '480px',
      }).afterClosed(),
    );
    if (result === undefined) return;

    this.savingJira.set(true);
    try {
      const response = await firstValueFrom(this.projectService.setJiraTicket(this.projectId, result));
      if (!response.success || !response.data) {
        this.notification.error(response.message || 'Could not save the Jira ticket.');
        return;
      }
      this.project.set(response.data);
      this.notification.success(response.message || 'Jira ticket saved on this application.');
    } catch (error) {
      this.notification.error(apiErrorMessage(error, 'Could not save the Jira ticket.'));
    } finally {
      this.savingJira.set(false);
    }
  }

  async shareToConnectorTeam(): Promise<void> {
    if (this.isReadOnlyVersion()) {
      this.notification.info('Share is only available for the active version.');
      return;
    }

    this.sharingEmail.set(true);
    try {
      const response = await firstValueFrom(this.pdfService.shareCompose(this.projectId));
      if (!response.success || !response.data) {
        this.notification.error(response.message || 'Could not open Outlook. Please try again.');
        return;
      }

      const compose = response.data;
      const result = await firstValueFrom(
        this.dialog.open(ShareConnectorDialogComponent, {
          data: {
            toName: compose.toName,
            toAddress: compose.to,
            subject: compose.subject,
            message: compose.body,
            attachmentFileName: compose.attachmentFileName,
            jiraTicket: this.jiraLabel() || this.jiraTicket(),
            prepareFile: () => this.prepareShareFile(),
          },
          width: '600px',
          autoFocus: 'first-tabbable',
        }).afterClosed(),
      );

      if (!result?.fileName) return;
      const dropped = result.fileName.toLowerCase().endsWith('.zip')
        ? `${result.fileName} downloaded. It contains the PDF and the JSON files.`
        : `${result.fileName} downloaded.`;
      this.notification.success(
        result.openedInBrowser
          ? `Outlook opened for ${compose.toName}. ${dropped} Drop that file onto the message, then send.`
          : dropped,
        8000,
      );
    } catch (error) {
      this.notification.error(apiErrorMessage(error, 'Could not open Outlook. Please try again.'));
    } finally {
      this.sharingEmail.set(false);
    }
  }

  private async prepareShareFile(): Promise<string> {
    const snap = this.snapshot();
    if (!snap) throw new Error('No saved information available to generate a PDF.');

    let response;
    try {
      response = await firstValueFrom(this.pdfService.generatePdf(this.projectId, this.toStringMap(snap.formData)));
    } catch {
      throw new Error('Could not prepare the CIGT file. Please try again.');
    }

    const blob = response.body;
    if (!blob) throw new Error('Could not prepare the CIGT file. Please try again.');
    const project = this.project();
    const fileName = cigtDownloadName(
      response,
      project?.clientName || 'Client',
      project?.applicationName || 'Application',
    );
    this.downloadBlob(blob, fileName);

    const versionNumber = this.selectedVersionNumber() ?? this.versions()[0]?.versionNumber;
    if (versionNumber != null) {
      this.docRepo.addGeneratedDocument({
        projectId: this.projectId,
        versionNumber,
        docType: 'pdf',
        fileName,
        includedAttachmentNames: snap.attachments.map((a) => a.name),
      });
    }

    return fileName;
  }

  sectionValue(
    sectionControls: string[],
    formData: Record<string, unknown>,
  ): Array<{ label: string; value: unknown }> {
    return sectionControls
      .map((key) => ({
        label: CONTROL_LABELS[key] ?? key,
        value: formData[key],
      }))
      .filter((x) => x.value !== undefined && x.value !== null && String(x.value).trim() !== '');
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

function apiErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error && 'error' in error) {
    const body = (error as { error?: { message?: string } }).error;
    if (body && typeof body.message === 'string' && body.message.trim()) return body.message;
  }
  return fallback;
}
