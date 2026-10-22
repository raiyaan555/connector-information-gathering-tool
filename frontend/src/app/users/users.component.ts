import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotificationService } from '../services/notification.service';
import { UserAdminService } from '../services/user-admin.service';
import { ManagedUser } from '../models/user.model';
import { ARCON_EMAIL_PATTERN } from '../validators/arcon-email.validator';
import { passwordPolicyValidator } from '../validators/password-policy.validator';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
})
export class UsersComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userAdmin = inject(UserAdminService);
  private readonly notification = inject(NotificationService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly hidePassword = signal(true);
  readonly users = signal<ManagedUser[]>([]);
  readonly loadError = signal('');

  readonly roles = ['Engineer', 'Viewer', 'Admin'];

  readonly form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email, Validators.pattern(ARCON_EMAIL_PATTERN)]],
    password: ['', [Validators.required, passwordPolicyValidator]],
    role: ['Engineer', Validators.required],
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.loadError.set('');
    this.userAdmin.getAll().subscribe({
      next: (res) => {
        this.users.set(res.data ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loadError.set(err?.error?.message || 'Could not load users');
        this.loading.set(false);
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.userAdmin.create(this.form.getRawValue()).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.notification.error(res.message || 'Could not create the user');
          this.saving.set(false);
          return;
        }

        this.users.update((current) =>
          [...current, res.data!].sort((a, b) => a.email.localeCompare(b.email)),
        );
        this.form.reset({ firstName: '', lastName: '', email: '', password: '', role: 'Engineer' });
        this.notification.success(`${res.data.fullName} can now sign in`);
        this.saving.set(false);
      },
      error: (err) => {
        this.notification.error(err?.error?.message || 'Could not create the user');
        this.saving.set(false);
      },
    });
  }
}
