import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ProjectService } from '../../services/project.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-new-project',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    PageHeaderComponent,
  ],
  templateUrl: './new-project.component.html',
  styleUrl: './new-project.component.scss',
})
export class NewProjectComponent {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly saving = signal(false);
  readonly priorities = ['Low', 'Medium', 'High', 'Critical'];

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    clientName: ['', Validators.required],
    applicationName: ['', Validators.required],
    implementationEngineer: ['', Validators.required],
    priority: ['Medium', Validators.required],
    description: [''],
    expectedCompletionDate: [null as Date | null],
  });

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.saving.set(true);
    this.projectService
      .create({
        name: raw.name,
        clientName: raw.clientName,
        applicationName: raw.applicationName,
        description: raw.description || undefined,
        implementationEngineer: raw.implementationEngineer,
        priority: raw.priority,
        expectedCompletionDate: raw.expectedCompletionDate?.toISOString(),
        status: 'Draft',
      })
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.notification.success('Project created successfully');
            this.router.navigate(['/project', res.data.id, 'edit']);
          }
          this.saving.set(false);
        },
        error: () => {
          this.notification.error('Failed to create project');
          this.saving.set(false);
        },
      });
  }
}
