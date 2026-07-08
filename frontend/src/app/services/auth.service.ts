import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AuthResponse,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  User,
  VerifyEmailRequest,
} from '../models/user.model';
import { ApiResponse } from '../models/api-response.model';

const TOKEN_KEY = 'crgp_auth_token';
const USER_KEY = 'crgp_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly token = signal<string | null>(this.getStoredToken());
  private readonly currentUser = signal<User | null>(this.getStoredUser());

  readonly isAuthenticated = computed(() => !!this.token());
  readonly user = this.currentUser.asReadonly();

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, credentials)
      .pipe(tap((response) => {
        if (response.success && response.data) {
          this.setSession(response.data, credentials.rememberMe);
        }
      }));
  }

  register(payload: RegisterRequest): Observable<ApiResponse<{ message: string }>> {
    return this.http.post<ApiResponse<{ message: string }>>(`${environment.apiUrl}/auth/register`, payload);
  }

  forgotPassword(payload: ForgotPasswordRequest): Observable<ApiResponse<{ message: string }>> {
    return this.http.post<ApiResponse<{ message: string }>>(`${environment.apiUrl}/auth/forgot-password`, payload);
  }

  verifyEmail(payload: VerifyEmailRequest): Observable<ApiResponse<{ message: string }>> {
    return this.http.post<ApiResponse<{ message: string }>>(`${environment.apiUrl}/auth/verify-email`, payload);
  }

  logout(): void {
    this.token.set(null);
    this.currentUser.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.token();
  }

  private setSession(auth: AuthResponse, rememberMe = false): void {
    const nameParts = auth.fullName.split(' ');
    const user: User = {
      email: auth.email,
      firstName: nameParts[0] || 'User',
      lastName: nameParts.slice(1).join(' ') || '',
      fullName: auth.fullName,
      emailVerified: auth.isEmailVerified,
    };

    this.token.set(auth.token);
    this.currentUser.set(user);

    const storage = rememberMe ? localStorage : sessionStorage;
    storage.setItem(TOKEN_KEY, auth.token);
    storage.setItem(USER_KEY, JSON.stringify(user));

    if (!rememberMe) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    } else {
      sessionStorage.removeItem(TOKEN_KEY);
      sessionStorage.removeItem(USER_KEY);
    }
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) ?? sessionStorage.getItem(TOKEN_KEY);
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem(USER_KEY) ?? sessionStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as User;
    } catch {
      return null;
    }
  }
}
