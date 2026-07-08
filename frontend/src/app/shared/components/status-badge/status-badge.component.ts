import { Component, input } from '@angular/core';
import { Project, ProjectStatus } from '../../../models/project.model';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span class="badge" [class]="'badge--' + statusClass()">{{ status() }}</span>
  `,
  styles: `
    .badge {
      display: inline-flex;
      align-items: center;
      padding: 4px 12px;
      border-radius: 16px;
      font-size: 12px;
      font-weight: 500;
      &--draft { background: #f3f4f6; color: #6b7280; }
      &--in-progress { background: #dbeafe; color: #2563eb; }
      &--completed { background: #dcfce7; color: #16a34a; }
      &--pending-review { background: #fef3c7; color: #d97706; }
    }
  `,
})
export class StatusBadgeComponent {
  readonly status = input.required<ProjectStatus>();

  statusClass(): string {
    return this.status().toLowerCase().replace(/\s+/g, '-');
  }
}
