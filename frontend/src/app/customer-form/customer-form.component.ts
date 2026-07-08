import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { CustomerFormService } from '../services/customer-form.service';
import { NotificationService } from '../services/notification.service';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import {
  createCustomerForm,
  LIFECYCLE_OPTIONS,
  APPLICATION_TYPES,
  SOT_ATTRIBUTES,
  ALLOWED_FILE_TYPES,
} from './customer-form.model';
import { CustomerFormInfo } from '../models/customer-form.model';

interface MockFile {
  name: string;
  type: string;
  size: number;
  uploadedAt: Date;
}

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    KeyValuePipe,
    DatePipe,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTableModule,
    MatDialogModule,
    MatDatepickerModule,
    MatNativeDateModule,
    LoadingSpinnerComponent,
  ],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
})
export class CustomerFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formService = inject(CustomerFormService);
  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  readonly form = createCustomerForm(this.fb);
  readonly loading = signal(true);
  readonly formInfo = signal<CustomerFormInfo | null>(null);
  readonly mockFiles = signal<MockFile[]>([]);
  readonly fileColumns = ['name', 'type', 'size', 'uploadedAt', 'actions'];

  readonly lifecycleOptions = LIFECYCLE_OPTIONS;
  readonly applicationTypes = APPLICATION_TYPES;
  readonly sotAttributes = SOT_ATTRIBUTES;
  readonly allowedTypes = ALLOWED_FILE_TYPES.join(', ');

  private token = '';

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token')!;
    this.formService.getForm(this.token).subscribe({
      next: (res) => {
        if (res.success) {
          this.formInfo.set(res.data);
          if (res.data.isSubmitted) {
            this.notification.info('This form has already been submitted.');
            this.form.disable();
          }
        }
        this.loading.set(false);
      },
      error: () => {
        this.notification.error('Invalid or expired form link');
        this.loading.set(false);
      },
    });
  }

  onLifecycleChange(feature: string, checked: boolean): void {
    const current = [...(this.form.get('lifecycleFeatures')?.value || [])];
    if (checked && !current.includes(feature)) current.push(feature);
    else if (!checked) current.splice(current.indexOf(feature), 1);
    this.form.patchValue({ lifecycleFeatures: current });
  }

  isLifecycleSelected(feature: string): boolean {
    return (this.form.get('lifecycleFeatures')?.value || []).includes(feature);
  }

  onSotAttributeChange(attr: string, checked: boolean): void {
    const current = [...(this.form.get('sotAttributes')?.value || [])];
    if (checked && !current.includes(attr)) current.push(attr);
    else if (!checked) current.splice(current.indexOf(attr), 1);
    this.form.patchValue({ sotAttributes: current });
  }

  isSotAttributeSelected(attr: string): boolean {
    return (this.form.get('sotAttributes')?.value || []).includes(attr);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    this.mockFiles.update((files) => [
      ...files,
      { name: file.name, type: file.type || 'application/octet-stream', size: file.size, uploadedAt: new Date() },
    ]);
    this.notification.success(`${file.name} added (mock upload)`);
    input.value = '';
  }

  removeFile(index: number): void {
    this.mockFiles.update((files) => files.filter((_, i) => i !== index));
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  submitForm(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '480px',
      data: {
        title: 'Confirm Submission',
        message: 'Please verify all the information. You can submit this form only once.',
        confirmText: 'Submit',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      const formData: Record<string, string> = {};
      Object.entries(this.form.getRawValue()).forEach(([key, value]) => {
        formData[key] = Array.isArray(value) ? value.join(', ') : String(value ?? '');
      });
      this.mockFiles().forEach((f, i) => {
        formData[`attachment_${i}`] = f.name;
      });

      this.formService.submitForm(this.token, { formData }).subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigate(['/form', this.token, 'success']);
          }
        },
        error: (err) => this.notification.error(err?.error?.message || 'Submission failed'),
      });
    });
  }
}
