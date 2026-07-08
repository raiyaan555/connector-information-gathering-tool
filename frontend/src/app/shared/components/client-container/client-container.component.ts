import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Client } from '../../../models/client.model';
import { Project } from '../../../models/project.model';

@Component({
  selector: 'app-client-container',
  standalone: true,
  imports: [RouterLink, DatePipe, MatButtonModule, MatIconModule],
  templateUrl: './client-container.component.html',
  styleUrl: './client-container.component.scss',
})
export class ClientContainerComponent {
  readonly client = input.required<Client>();
  readonly applications = input.required<Project[]>();

  readonly addApplication = output<void>();
  readonly deleteClient = output<void>();
  readonly deleteApplication = output<Project>();

  lastUpdated(): string {
    const dates = this.applications().map((a) => a.updatedAt);
    if (!dates.length) return this.client().updatedAt;
    return dates.sort((a, b) => new Date(b).getTime() - new Date(a).getTime())[0];
  }
}
