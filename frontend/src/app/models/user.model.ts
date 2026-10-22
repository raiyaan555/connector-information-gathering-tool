export interface User {
  email: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  emailVerified?: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  isEmailVerified: boolean;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface VerifyEmailRequest {
  email: string;
  verificationCode?: string;
}

export interface ManagedUser {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  fullName: string;
  createdAt: string;
  isEmailVerified: boolean;
  roles: string[];
}

export interface CreateManagedUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: string;
}
