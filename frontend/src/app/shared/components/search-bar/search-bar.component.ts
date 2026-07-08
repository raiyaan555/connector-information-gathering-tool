import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatIconModule, MatButtonModule],
  template: `
    <mat-form-field appearance="outline" class="search-bar" subscriptSizing="dynamic">
      <mat-icon matPrefix>search</mat-icon>
      <input
        matInput
        [placeholder]="placeholder()"
        [ngModel]="query()"
        (ngModelChange)="onQueryChange($event)"
        (keyup.enter)="onSearch()"
      />
      @if (query()) {
        <button mat-icon-button matSuffix type="button" (click)="clear()">
          <mat-icon>close</mat-icon>
        </button>
      }
    </mat-form-field>
  `,
  styleUrl: './search-bar.component.scss',
})
export class SearchBarComponent {
  readonly placeholder = input('Search...');
  readonly live = input(false);
  readonly search = output<string>();

  readonly query = signal('');

  onQueryChange(value: string): void {
    this.query.set(value);
    if (this.live()) {
      this.search.emit(value.trim());
    }
  }

  onSearch(): void {
    this.search.emit(this.query().trim());
  }

  clear(): void {
    this.query.set('');
    this.search.emit('');
  }
}
