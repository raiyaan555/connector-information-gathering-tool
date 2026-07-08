import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { ClientContainerComponent } from '../shared/components/client-container/client-container.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';
import { InputDialogComponent } from '../shared/components/input-dialog/input-dialog.component';
import { ClientService } from '../services/client.service';
import { ProjectService } from '../services/project.service';
import { NotificationService } from '../services/notification.service';
import { Client } from '../models/client.model';
import { Project } from '../models/project.model';

interface ClientGroup {
  client: Client;
  applications: Project[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [ClientContainerComponent, LoadingSpinnerComponent, MatButtonModule, MatIconModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly clientService = inject(ClientService);
  private readonly projectService = inject(ProjectService);
  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  readonly clients = signal<Client[]>([]);
  readonly projects = signal<Project[]>([]);
  readonly loading = signal(true);
  readonly searchQuery = signal('');

  readonly clientGroups = computed<ClientGroup[]>(() => {
    return this.clients().map((client) => ({
      client,
      applications: this.projects().filter(
        (p) => p.clientName.toLowerCase() === client.companyName.toLowerCase(),
      ),
    }));
  });

  readonly filteredGroups = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.clientGroups();

    return this.clientGroups().filter((group) => {
      const clientMatch =
        group.client.companyName.toLowerCase().includes(q) ||
        group.client.industry.toLowerCase().includes(q) ||
        group.client.primaryContact.toLowerCase().includes(q);

      const appMatch = group.applications.some((app) => {
        return (
          app.name.toLowerCase().includes(q) ||
          app.applicationName.toLowerCase().includes(q) ||
          (app.implementationEngineer || '').toLowerCase().includes(q) ||
          (app.createdBy || '').toLowerCase().includes(q) ||
          app.status.toLowerCase().includes(q)
        );
      });

      return clientMatch || appMatch;
    });
  });

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (params['q']) this.searchQuery.set(params['q']);
    });
    this.loadAll();
  }

  loadAll(): void {
    this.loading.set(true);
    let loaded = 0;
    const done = () => {
      loaded += 1;
      if (loaded === 2) this.loading.set(false);
    };

    this.clientService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.clients.set(res.data);
        done();
      },
      error: () => {
        this.notification.error('Failed to load clients');
        done();
      },
    });

    this.projectService.getAll().subscribe({
      next: (res) => {
        if (res.success) this.projects.set(res.data);
        done();
      },
      error: () => {
        this.notification.error('Failed to load applications');
        done();
      },
    });
  }

  addClient(): void {
    const ref = this.dialog.open(InputDialogComponent, {
      width: '440px',
      data: {
        title: 'Add Client',
        label: 'Company Name',
        placeholder: 'e.g. Axis Bank',
        confirmText: 'Add Client',
      },
    });

    ref.afterClosed().subscribe((companyName) => {
      if (!companyName) return;

      this.clientService
        .create({
          companyName,
          industry: 'General',
          primaryContact: '—',
          email: 'contact@sample.com',
          phone: '—',
          country: '—',
          address: '—',
          notes: '',
        })
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.notification.success('Client added');
              this.loadAll();
            }
          },
          error: () => this.notification.error('Failed to add client'),
        });
    });
  }

  addApplication(client: Client): void {
    const ref = this.dialog.open(InputDialogComponent, {
      width: '440px',
      data: {
        title: 'Add Application',
        label: 'Application Name',
        placeholder: 'e.g. Salesforce',
        confirmText: 'Add Application',
      },
    });

    ref.afterClosed().subscribe((applicationName) => {
      if (!applicationName) return;

      this.projectService
        .create({
          name: `${client.companyName} - ${applicationName}`,
          clientName: client.companyName,
          applicationName,
          status: 'Draft',
        })
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.notification.success('Application added');
              this.router.navigate(['/project', res.data.id, 'edit']);
            }
          },
          error: () => this.notification.error('Failed to add application'),
        });
    });
  }

  confirmDeleteClient(client: Client): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '440px',
      data: {
        title: 'Delete Client',
        message: `Are you sure you want to delete "${client.companyName}" and all its applications?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.clientService.delete(client.id).subscribe({
        next: (res) => {
          const ok = (res as unknown as { success?: boolean; Success?: boolean })?.success ?? (res as any)?.Success;
          if (ok) this.notification.success('Client deleted');
          else this.notification.error(res?.message ?? (res as any)?.Message ?? 'Failed to delete client');
          this.loadAll();
        },
        error: () => {
          this.notification.error('Failed to delete client');
          this.loadAll();
        },
      });
    });
  }

  confirmDeleteApplication(app: Project): void {
    const name = app.applicationName || app.name;
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '440px',
      data: {
        title: 'Delete Application',
        message: `Are you sure you want to delete "${name}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.projectService.delete(app.id).subscribe({
        next: (res) => {
          const ok = (res as unknown as { success?: boolean; Success?: boolean })?.success ?? (res as any)?.Success;
          if (ok) this.notification.success('Application deleted');
          else this.notification.error(res?.message ?? (res as any)?.Message ?? 'Failed to delete application');
          this.loadAll();
        },
        error: () => {
          this.notification.error('Failed to delete application');
          this.loadAll();
        },
      });
    });
  }
}
