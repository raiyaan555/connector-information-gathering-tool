import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Attachment, UploadAttachmentRequest } from '../models/attachment.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AttachmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/attachments`;

  getByProjectId(projectId: string): Observable<ApiResponse<Attachment[]>> {
    return this.http.get<ApiResponse<Attachment[]>>(`${this.baseUrl}/project/${projectId}`);
  }

  upload(projectId: string, payload: UploadAttachmentRequest): Observable<ApiResponse<Attachment>> {
    return this.http.post<ApiResponse<Attachment>>(`${this.baseUrl}/project/${projectId}`, payload);
  }

  delete(id: string): Observable<ApiResponse<{ message: string }>> {
    return this.http.delete<ApiResponse<{ message: string }>>(`${this.baseUrl}/${id}`);
  }
}
