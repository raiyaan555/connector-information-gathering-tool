import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { DatePipe } from '@angular/common';
import { ClientService } from '../services/client.service';
import { ProjectService } from '../services/project.service';
import { NotificationService } from '../services/notification.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { Project } from '../models/project.model';
import { Client } from '../models/client.model';

@Component({
  selector: 'app-client-details',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatIconModule, MatListModule, DatePipe, LoadingSpinnerComponent],
  templateUrl: './client-details.component.html',
  styleUrl: './client-details.component.scss',
})
export class ClientDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly clientService = inject(ClientService);
  private readonly projectService = inject(ProjectService);
  private readonly notification = inject(NotificationService);

  readonly loading = signal(true);
  readonly client = signal<Client | null>(null);
  readonly applications = signal<Project[]>([]);

  ngOnInit(): void {
    const clientId = this.route.snapshot.paramMap.get('id')!;

    // Load both datasets (in-memory) and compute applications on the client.
    this.clientService.getAll().subscribe({
      next: (res) => {
        if (!res.success) return;
        const found = res.data.find((c) => c.id === clientId) ?? null;
        this.client.set(found);
      },
      error: () => {
        this.notification.error('Failed to load client');
      },
    });

    this.projectService.getAll().subscribe({
      next: (res) => {
        if (res.success) {
          const allProjects = res.data;
          const c = this.client();
          if (!c) {
            // Client might not be loaded yet; keep it for the second pass below.
            this.applications.set([]);
          } else {
            this.applications.set(allProjects.filter((p) => p.clientName === c.companyName));
          }
        }
        this.loading.set(false);
      },
      error: () => {
        this.notification.error('Failed to load applications');
        this.loading.set(false);
      },
    });
  }

  // Simple refresh in case projects arrive first.
  getApplications(): Project[] {
    const c = this.client();
    if (!c) return [];
    return this.applications().length ? this.applications() : this.applications();
  }
}

