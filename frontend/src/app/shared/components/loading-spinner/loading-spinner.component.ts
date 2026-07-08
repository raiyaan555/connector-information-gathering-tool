import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="spinner" [class.spinner--overlay]="overlay()">
      <mat-spinner diameter="48"></mat-spinner>
    </div>
  `,
  styles: `
    .spinner {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 48px;
      &--overlay {
        position: fixed;
        inset: 0;
        background: rgba(255, 255, 255, 0.7);
        z-index: 1000;
      }
    }
  `,
})
export class LoadingSpinnerComponent {
  readonly overlay = input(false);
}
