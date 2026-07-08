import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';

export interface ReviewSaveDialogItem {
  label: string;
  value: string;
}

export interface ReviewSaveDialogSection {
  title: string;
  items: ReviewSaveDialogItem[];
}

export interface ReviewSaveDialogData {
  title: string;
  sections: ReviewSaveDialogSection[];
  attachments: string[];
  note?: string;
}

@Component({
  selector: 'app-review-save-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatExpansionModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon class="icon">fact_check</mat-icon>
      {{ data.title }}
    </h2>

    <mat-dialog-content>
      @if (data.note) {
        <div class="note">{{ data.note }}</div>
      }
      <mat-accordion multi class="review-accordion">
        @for (s of data.sections; track s.title) {
          <mat-expansion-panel>
            <mat-expansion-panel-header>
              <mat-panel-title>{{ s.title }}</mat-panel-title>
            </mat-expansion-panel-header>
            @if (s.items.length === 0) {
              <div class="empty">No values yet.</div>
            } @else {
              <div class="review-items">
                @for (i of s.items; track i.label) {
                  <div class="review-item">
                    <div class="review-item__k">{{ i.label }}</div>
                    <div class="review-item__v">{{ i.value }}</div>
                  </div>
                }
              </div>
            }
          </mat-expansion-panel>
        }

        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Attachments</mat-panel-title>
          </mat-expansion-panel-header>
          @if (data.attachments.length === 0) {
            <div class="empty">No files uploaded.</div>
          } @else {
            <div class="attachments">
              @for (a of data.attachments; track a) {
                <div class="attachment-line">{{ a }}</div>
              }
            </div>
          }
        </mat-expansion-panel>
      </mat-accordion>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close(false)">Back to Edit</button>
      <button mat-flat-button color="primary" (click)="dialogRef.close(true)">Review &amp; Save</button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .icon {
        margin-right: 8px;
      }

      .review-accordion {
        max-width: 900px;
      }

      .review-items {
        display: flex;
        flex-direction: column;
        gap: 10px;
      }

      .review-item {
        display: grid;
        grid-template-columns: 240px 1fr;
        gap: 12px;
        padding: 6px 0;
        border-bottom: 1px solid var(--color-border);
      }

      .review-item__k {
        color: var(--text-secondary);
        font-size: 12px;
        font-weight: 700;
      }

      .review-item__v {
        font-size: 13px;
        word-break: break-word;
      }

      .attachments {
        display: flex;
        flex-direction: column;
        gap: 6px;
      }

      .attachment-line {
        font-size: 13px;
        font-weight: 600;
        color: var(--text-primary);
      }

      .empty {
        color: var(--text-secondary);
        font-size: 13px;
        padding: 8px 0;
      }

      .note {
        color: var(--text-secondary);
        font-size: 13px;
        margin-bottom: 12px;
        line-height: 1.4;
      }
    `,
  ],
})
export class ReviewSaveDialogComponent {
  readonly data = inject<ReviewSaveDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ReviewSaveDialogComponent, boolean>);
}

