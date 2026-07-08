import { Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [MatCardModule, MatSlideToggleModule, MatButtonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  private readonly notification = inject(NotificationService);

  readonly enableMockNotifications = signal(false);

  save(): void {
    this.notification.success('Settings saved (mock)');
  }
}

