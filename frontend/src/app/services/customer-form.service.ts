import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CustomerFormInfo,
  CustomerFormResponse,
  SubmitCustomerFormRequest,
} from '../models/customer-form.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class CustomerFormService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/customer-form`;

  getForm(token: string): Observable<ApiResponse<CustomerFormInfo>> {
    return this.http.get<ApiResponse<CustomerFormInfo>>(`${this.baseUrl}/${token}`);
  }

  submitForm(token: string, payload: SubmitCustomerFormRequest): Observable<ApiResponse<CustomerFormResponse>> {
    return this.http.post<ApiResponse<CustomerFormResponse>>(`${this.baseUrl}/${token}`, payload);
  }

  getResponses(projectId: string): Observable<ApiResponse<CustomerFormResponse[]>> {
    return this.http.get<ApiResponse<CustomerFormResponse[]>>(`${this.baseUrl}/project/${projectId}/responses`);
  }
}
