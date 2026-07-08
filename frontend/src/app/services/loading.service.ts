import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly count = signal(0);
  readonly isLoading = signal(false);

  show(): void {
    this.count.update((c) => c + 1);
    this.isLoading.set(true);
  }

  hide(): void {
    this.count.update((c) => Math.max(0, c - 1));
    if (this.count() === 0) this.isLoading.set(false);
  }
}
