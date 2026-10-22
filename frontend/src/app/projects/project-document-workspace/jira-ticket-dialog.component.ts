import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export interface JiraTicketDialogData {
  initialValue: string;
}

const ISSUE_KEY = /^[A-Za-z][A-Za-z0-9_]*-\d+$/;

export function jiraTicketError(raw: string): string | null {
  const value = raw.trim();
  if (!value) return 'Enter a Jira issue key such as ABC-123, or a Jira link.';
  if (value.length > 500) return 'Jira ticket must be 500 characters or fewer.';
  if (ISSUE_KEY.test(value)) return null;
  try {
    const url = new URL(value);
    if ((url.protocol === 'http:' || url.protocol === 'https:') && url.hostname) return null;
  } catch {
    // Not a URL. The message below covers that.
  }
  return 'Enter a Jira issue key such as ABC-123, or a Jira link.';
}

export function jiraTicketHref(value: string): string | null {
  try {
    const url = new URL(value);
    if (url.protocol === 'http:' || url.protocol === 'https:') return url.toString();
  } catch {
    // An issue key has no link until the engineer pastes the Jira URL.
  }
  return null;
}

export function jiraTicketLabel(value: string): string {
  const href = jiraTicketHref(value);
  if (!href) return value;
  const browse = /\/browse\/([A-Za-z][A-Za-z0-9_]*-\d+)/i.exec(href);
  return browse?.[1] ?? value;
}

@Component({
  selector: 'app-jira-ticket-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.initialValue ? 'Update Jira Ticket' : 'Attach Jira Ticket' }}</h2>
    <mat-dialog-content>
      <p class="hint">Save the Jira ticket for this application. Share includes it in the Outlook message and in the PDF.</p>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Jira ticket</mat-label>
          <input matInput formControlName="value" placeholder="ABC-123 or https://…/browse/ABC-123" />
        </mat-form-field>
      </form>
      @if (error()) {
        <p class="field-error">{{ error() }}</p>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      @if (data.initialValue) {
        <button mat-button type="button" color="warn" (click)="remove()">Remove</button>
      }
      <button mat-button type="button" (click)="dialogRef.close()">Cancel</button>
      <button mat-flat-button color="primary" type="button" (click)="submit()">Save</button>
    </mat-dialog-actions>
  `,
  styles: `
    .full-width { width: 100%; min-width: min(420px, 70vw); }
    mat-dialog-content { padding-top: 8px; }
    .hint { margin: 0 0 12px; color: var(--text-secondary); font-size: 13px; }
    .field-error { margin: 0; color: #b91c1c; font-size: 13px; }
  `,
})
export class JiraTicketDialogComponent {
  readonly data = inject<JiraTicketDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<JiraTicketDialogComponent, string | null>);
  private readonly fb = inject(FormBuilder);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    value: [this.data.initialValue || ''],
  });

  submit(): void {
    const value = this.form.controls.value.value;
    const error = jiraTicketError(value);
    this.error.set(error);
    if (error) {
      this.form.controls.value.markAsTouched();
      return;
    }
    this.dialogRef.close(value.trim());
  }

  remove(): void {
    this.dialogRef.close(null);
  }
}
