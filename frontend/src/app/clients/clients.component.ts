import { Component, inject, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { DatePipe } from '@angular/common';
import { ClientService } from '../services/client.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner/loading-spinner.component';
import { NotificationService } from '../services/notification.service';
import { Client } from '../models/client.model';

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [MatCardModule, MatTableModule, MatIconModule, LoadingSpinnerComponent, DatePipe],
  templateUrl: './clients.component.html',
  styleUrl: './clients.component.scss',
})
export class ClientsComponent implements OnInit {
  private readonly clientService = inject(ClientService);
  private readonly notification = inject(NotificationService);

  readonly loading = signal(true);
  readonly clients = signal<Client[]>([]);

  readonly displayedColumns = ['companyName', 'industry', 'primaryContact', 'email', 'phone', 'country', 'createdAt'];

  ngOnInit(): void {
    this.clientService.getAll().subscribe({
      next: (res) => {
        if (res.success) {
          this.clients.set(res.data);
        }
        this.loading.set(false);
      },
      error: () => {
        this.notification.error('Failed to load clients');
        this.loading.set(false);
      },
    });
  }
}

