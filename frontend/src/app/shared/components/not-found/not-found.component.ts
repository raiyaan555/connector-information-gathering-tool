import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <div class="not-found">
      <mat-icon>error_outline</mat-icon>
      <h1>404</h1>
      <p>Page not found</p>
      <a mat-flat-button color="primary" routerLink="/dashboard">Go to Dashboard</a>
    </div>
  `,
  styles: `
    .not-found {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
      text-align: center;
      mat-icon { font-size: 64px; width: 64px; height: 64px; color: #9ca3af; }
      h1 { margin: 16px 0 8px; font-size: 48px; }
      p { color: #6b7280; margin-bottom: 24px; }
    }
  `,
})
export class NotFoundComponent {}
