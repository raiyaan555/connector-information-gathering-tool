import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Project } from '../../../models/project.model';

@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [RouterLink, MatIconModule, DatePipe],
  templateUrl: './project-card.component.html',
  styleUrl: './project-card.component.scss',
})
export class ProjectCardComponent {
  readonly project = input.required<Project>();
  readonly completionPercent = input(0);

  statusClass(): string {
    const status = this.project().status;
    if (status === 'Completed') return 'status--completed';
    if (status === 'Pending Review') return 'status--review';
    if (status === 'In Progress') return 'status--progress';
    return 'status--draft';
  }
}
