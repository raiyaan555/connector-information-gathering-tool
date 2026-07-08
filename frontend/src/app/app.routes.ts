import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: '',
    loadComponent: () => import('./layouts/auth-layout/auth-layout.component').then((m) => m.AuthLayoutComponent),
    canActivate: [guestGuard],
    children: [
      { path: 'login', loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./auth/register/register.component').then((m) => m.RegisterComponent) },
      { path: 'verify-email', loadComponent: () => import('./auth/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent) },
      { path: 'forgot-password', loadComponent: () => import('./auth/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent) },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./dashboard/dashboard.component').then((m) => m.DashboardComponent) },
      { path: 'project/new', loadComponent: () => import('./projects/new-project/new-project.component').then((m) => m.NewProjectComponent) },
      { path: 'project/:id', loadComponent: () => import('./projects/project-document-workspace/project-document-workspace.component').then((m) => m.ProjectDocumentWorkspaceComponent) },
      { path: 'project/:id/edit', loadComponent: () => import('./projects/project-workspace/project-workspace.component').then((m) => m.ProjectWorkspaceComponent) },
      { path: 'applications/new', redirectTo: 'project/new', pathMatch: 'full' },
      { path: 'applications/:id', redirectTo: 'project/:id/edit', pathMatch: 'full' },
      { path: 'clients/new', loadComponent: () => import('./clients/client-new.component').then((m) => m.ClientNewComponent) },
      { path: 'clients/:id', loadComponent: () => import('./clients/client-details.component').then((m) => m.ClientDetailsComponent) },
      { path: 'settings', loadComponent: () => import('./settings/settings.component').then((m) => m.SettingsComponent) },
    ],
  },
  { path: 'form/:token', loadComponent: () => import('./customer-form/customer-form.component').then((m) => m.CustomerFormComponent) },
  { path: 'form/:token/success', loadComponent: () => import('./customer-form/submission-success/submission-success.component').then((m) => m.SubmissionSuccessComponent) },
  { path: 'not-found', loadComponent: () => import('./shared/components/not-found/not-found.component').then((m) => m.NotFoundComponent) },
  { path: '**', redirectTo: 'not-found' },
];
