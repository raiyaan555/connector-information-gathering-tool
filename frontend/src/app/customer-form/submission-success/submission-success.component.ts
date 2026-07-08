import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-submission-success',
  standalone: true,
  imports: [MatCardModule, MatIconModule],
  template: `
    <div class="success-page">
      <mat-card class="success-card">
        <mat-icon class="success-icon">check_circle</mat-icon>
        <h1>Submission Successful</h1>
        <p>Thank you! Your connector requirement information has been submitted successfully.</p>
        <p class="note">You cannot submit this form again. Our team will review your responses.</p>
      </mat-card>
    </div>
  `,
  styles: `
    .success-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f5f7fa;
      padding: 24px;
    }
    .success-card {
      text-align: center;
      padding: 48px;
      max-width: 480px;
      border-radius: 12px !important;
    }
    .success-icon {
      font-size: 72px;
      width: 72px;
      height: 72px;
      color: #22c55e;
      margin-bottom: 16px;
    }
    h1 { margin: 0 0 12px; font-size: 24px; }
    p { color: #6b7280; margin: 0 0 8px; }
    .note { font-size: 13px; margin-top: 16px; }
  `,
})
export class SubmissionSuccessComponent {}
