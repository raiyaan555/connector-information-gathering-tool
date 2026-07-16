import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PdfService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/projects`;

  generatePdf(projectId: string, formData: Record<string, string>): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${projectId}/generate-pdf`, { formData }, {
      responseType: 'blob',
    });
  }

  shareEmail(projectId: string): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/${projectId}/share-email`, {}, {
      responseType: 'blob',
    });
  }
}
