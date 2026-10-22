import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateManagedUserRequest, ManagedUser } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserAdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getAll(): Observable<ApiResponse<ManagedUser[]>> {
    return this.http.get<ApiResponse<ManagedUser[]>>(this.baseUrl);
  }

  create(payload: CreateManagedUserRequest): Observable<ApiResponse<ManagedUser>> {
    return this.http.post<ApiResponse<ManagedUser>>(this.baseUrl, payload);
  }
}
