import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotificationService } from '../services/notification.service';
import { ClientService } from '../services/client.service';

@Component({
  selector: 'app-client-new',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './client-new.component.html',
  styleUrl: './client-new.component.scss',
})
export class ClientNewComponent {
  private readonly fb = inject(FormBuilder);
  private readonly clientService = inject(ClientService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    industry: ['', [Validators.required]],
    primaryContact: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required]],
    country: ['', [Validators.required]],
    address: ['', [Validators.required]],
    notes: [''],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.clientService.create(this.form.getRawValue()).subscribe({
      next: (res) => {
        if (res.success) {
          this.notification.success('Client saved successfully');
          this.router.navigate(['/clients']);
        } else {
          this.notification.error(res.message || 'Failed to save client');
        }
      },
      error: () => {
        this.notification.error('Failed to save client');
      },
      complete: () => this.saving.set(false),
    });
  }
}

