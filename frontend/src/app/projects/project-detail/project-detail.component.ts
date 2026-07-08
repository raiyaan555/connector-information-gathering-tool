import { DatePipe, KeyValuePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { ProjectService } from '../../services/project.service';
import { CustomerFormService } from '../../services/customer-form.service';
import { AttachmentService } from '../../services/attachment.service';
import { NotificationService } from '../../services/notification.service';
import { Project } from '../../models/project.model';
import { CustomerFormResponse } from '../../models/customer-form.model';
import { Attachment } from '../../models/attachment.model';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    DatePipe,
    KeyValuePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatChipsModule,
    PageHeaderComponent,
    StatusBadgeComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly projectService = inject(ProjectService);
  private readonly formService = inject(CustomerFormService);
  private readonly attachmentService = inject(AttachmentService);
  private readonly notification = inject(NotificationService);

  readonly project = signal<Project | null>(null);
  readonly responses = signal<CustomerFormResponse[]>([]);
  readonly attachments = signal<Attachment[]>([]);
  readonly loading = signal(true);
  readonly selectedTab = signal(0);

  readonly attachmentColumns = ['fileName', 'contentType', 'fileSize', 'uploadedAt'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    const tab = this.route.snapshot.queryParamMap.get('tab');
    if (tab === 'responses') this.selectedTab.set(1);

    this.projectService.getById(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.project.set(res.data);
          this.loadResponses(id);
          this.loadAttachments(id);
        }
        this.loading.set(false);
      },
      error: () => {
        this.notification.error('Project not found');
        this.loading.set(false);
      },
    });
  }

  loadResponses(projectId: string): void {
    this.formService.getResponses(projectId).subscribe({
      next: (res) => {
        if (res.success) this.responses.set(res.data || []);
      },
    });
  }

  loadAttachments(projectId: string): void {
    this.attachmentService.getByProjectId(projectId).subscribe({
      next: (res) => {
        if (res.success) this.attachments.set(res.data || []);
      },
    });
  }

  generateLink(): void {
    const p = this.project();
    if (!p) return;
    this.projectService.generateLink(p.id).subscribe({
      next: (res) => {
        if (res.success) {
          navigator.clipboard.writeText(res.data.formLink);
          this.notification.success('Form link copied to clipboard');
          this.project.set({ ...p, formLink: res.data.formLink, formToken: res.data.token });
        }
      },
    });
  }

  generateDocument(): void {
    this.notification.info('Document generation will be available in a future phase.');
  }

  downloadAttachments(): void {
    this.notification.info('Attachment download will be available in a future phase.');
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }
}
