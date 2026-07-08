import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../services/auth.service';
import { LoadingService } from '../../services/loading.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { SearchBarComponent } from '../../shared/components/search-bar/search-bar.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatButtonModule, LoadingSpinnerComponent, SearchBarComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  readonly loadingService = inject(LoadingService);

  readonly userInitials = computed(() => {
    const user = this.authService.user();
    if (!user) return 'AU';
    const first = user.firstName?.charAt(0) ?? '';
    const last = user.lastName?.charAt(0) ?? '';
    const initials = `${first}${last}`.toUpperCase();
    if (initials) return initials;
    const parts = user.fullName?.trim().split(/\s+/) ?? [];
    if (parts.length >= 2) return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
    return parts[0]?.slice(0, 2).toUpperCase() || 'AU';
  });

  readonly userName = computed(() => this.authService.user()?.fullName ?? 'Admin User');

  onSearch(query: string): void {
    this.router.navigate(['/dashboard'], { queryParams: { q: query || null } });
  }

  logout(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Logout',
        message: 'Are you sure you want to logout?',
        confirmText: 'Logout',
        cancelText: 'Cancel',
      },
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) this.authService.logout();
    });
  }
}
