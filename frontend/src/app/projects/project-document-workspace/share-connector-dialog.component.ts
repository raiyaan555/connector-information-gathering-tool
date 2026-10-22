import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { outlookWebComposeUrl } from '../../services/pdf.service';

export interface ShareConnectorDialogData {
  toName: string;
  toAddress: string;
  subject: string;
  message: string;
  attachmentFileName: string;
  jiraTicket: string;
  prepareFile: () => Promise<string>;
}

export interface ShareConnectorResult {
  fileName: string;
  openedInBrowser: boolean;
}

@Component({
  selector: 'app-share-connector-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Share to Connector Team</h2>
    <mat-dialog-content>
      <p class="hint">
        Outlook opens in your browser, addressed to {{ data.toName }}
        ({{ data.toAddress }}). {{ data.attachmentFileName }} downloads with this message.
        Drop that file onto the Outlook message, then send.
      </p>
      <dl class="meta">
        <div>
          <dt>To</dt>
          <dd>{{ data.toName }} · {{ data.toAddress }}</dd>
        </div>
        <div>
          <dt>File</dt>
          <dd>{{ data.attachmentFileName }}</dd>
        </div>
        <div>
          <dt>Jira</dt>
          <dd>{{ data.jiraTicket || 'No ticket saved on this application yet' }}</dd>
        </div>
      </dl>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Message</mat-label>
          <textarea matInput rows="12" formControlName="message"></textarea>
          @if (form.controls.message.hasError('required') && form.controls.message.touched) {
            <mat-error>Write a message for the connector team.</mat-error>
          }
          @if (form.controls.message.hasError('maxlength')) {
            <mat-error>Keep the message under 4,000 characters.</mat-error>
          }
        </mat-form-field>
      </form>
      @if (error()) {
        <p class="share-error">{{ error() }}</p>
      }
      @if (blockedUrl()) {
        <p class="hint">Your browser blocked the Outlook tab. The file has downloaded.</p>
        <a class="outlook-link" [href]="blockedUrl()!" target="_blank" rel="noopener" (click)="finishFromLink()">
          Open Outlook
        </a>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close()" [disabled]="opening()">Cancel</button>
      <button mat-flat-button color="primary" type="button" (click)="openOutlook()" [disabled]="opening() || !!blockedUrl()">
        {{ opening() ? 'Opening Outlook…' : 'Open in Outlook' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .full-width { width: 100%; }
    mat-dialog-content { padding-top: 8px; max-width: 560px; }
    .hint { margin: 0 0 12px; color: var(--text-secondary); font-size: 13px; line-height: 1.45; }
    .meta { display: flex; flex-direction: column; gap: 8px; margin: 0 0 12px; }
    .meta div { display: grid; grid-template-columns: 52px 1fr; gap: 8px; }
    dt { color: var(--text-secondary); font-size: 12px; }
    dd { margin: 0; font-size: 13px; font-weight: 700; word-break: break-word; }
    .share-error { margin: 0 0 8px; color: #b91c1c; font-size: 13px; }
    .outlook-link { font-weight: 700; }
    textarea { resize: vertical; }
  `,
})
export class ShareConnectorDialogComponent {
  readonly data = inject<ShareConnectorDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ShareConnectorDialogComponent, ShareConnectorResult>);
  private readonly fb = inject(FormBuilder);
  readonly opening = signal(false);
  readonly error = signal<string | null>(null);
  readonly blockedUrl = signal<string | null>(null);
  private downloadedName = '';

  readonly form = this.fb.nonNullable.group({
    message: [this.data.message, [Validators.required, Validators.maxLength(4000)]],
  });

  openOutlook(): void {
    if (this.form.invalid || this.opening() || this.blockedUrl()) {
      this.form.markAllAsTouched();
      return;
    }

    const popup = window.open('about:blank', '_blank');
    this.opening.set(true);
    this.error.set(null);
    void this.finish(popup);
  }

  finishFromLink(): void {
    this.dialogRef.close({
      fileName: this.downloadedName || this.data.attachmentFileName,
      openedInBrowser: true,
    });
  }

  private async finish(popup: Window | null): Promise<void> {
    try {
      const fileName = await this.data.prepareFile();
      this.downloadedName = fileName;
      const url = outlookWebComposeUrl(
        this.data.toAddress,
        this.data.subject,
        this.form.controls.message.value.trim(),
      );
      if (popup && !popup.closed) {
        try {
          popup.opener = null;
          popup.location.href = url;
          this.dialogRef.close({ fileName, openedInBrowser: true });
          return;
        } catch {
          this.blockedUrl.set(url);
          return;
        }
      }
      popup?.close();
      this.blockedUrl.set(url);
    } catch (error) {
      popup?.close();
      this.error.set(error instanceof Error && error.message
        ? error.message
        : 'Could not prepare the CIGT file. Please try again.');
    } finally {
      this.opening.set(false);
    }
  }
}
