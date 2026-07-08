import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Client } from '../../../models/client.model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-client-tile',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatIconModule, DatePipe],
  templateUrl: './client-tile.component.html',
  styleUrl: './client-tile.component.scss',
})
export class ClientTileComponent {
  readonly client = input.required<Client>();

  readonly applicationCount = input.required<number>();
  readonly pendingCount = input.required<number>();
  readonly completedCount = input.required<number>();

  readonly lastUpdated = input.required<string>();
  readonly progress = input.required<number>(); // 0..1

  readonly progressSegments = 10;
  readonly Math = Math;

  segments(): number[] {
    return Array.from({ length: this.progressSegments }, (_, i) => i);
  }

  stageColor(): string {
    // Simple mapping for now.
    if (this.completedCount() === 0) return '#2563eb';
    if (this.pendingCount() === 0) return '#16a34a';
    return '#f59e0b';
  }
}

