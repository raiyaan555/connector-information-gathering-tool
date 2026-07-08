import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatButtonModule, MatIconModule],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <div class="success-state">
          <mat-icon class="success-icon">mark_email_read</mat-icon>
          <h1>Email Verified Successfully</h1>
          <p>Your email has been verified. You can now sign in to your account.</p>
          <a mat-flat-button color="primary" routerLink="/login">Go to Login</a>
        </div>
      </mat-card>
    </div>
  `,
  styles: `
    @import '../login/login.component.scss';
    .success-state {
      text-align: center;
      padding: 24px 0;
      .success-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        color: #22c55e;
        margin-bottom: 16px;
      }
      h1 { margin: 0 0 12px; font-size: 22px; font-weight: 600; }
      p { color: #6b7280; margin-bottom: 24px; }
    }
  `,
})
export class VerifyEmailComponent {}
