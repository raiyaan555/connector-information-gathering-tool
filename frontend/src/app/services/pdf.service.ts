import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface ShareCompose {
  to: string;
  toName: string;
  subject: string;
  body: string;
  attachmentFileName: string;
}

export function outlookWebComposeUrl(to: string, subject: string, body: string): string {
  return `https://outlook.office.com/mail/deeplink/compose?to=${encodeURIComponent(to)}&subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
}

@Injectable({ providedIn: 'root' })
export class PdfService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/projects`;

  generatePdf(projectId: string, formData: Record<string, string>): Observable<HttpResponse<Blob>> {
    return this.http.post(`${this.baseUrl}/${projectId}/generate-pdf`, { formData }, {
      responseType: 'blob',
      observe: 'response',
    });
  }

  shareCompose(projectId: string): Observable<ApiResponse<ShareCompose>> {
    return this.http.post<ApiResponse<ShareCompose>>(`${this.baseUrl}/${projectId}/share-email`, {});
  }
}

/** Prefer the API file name (`CIGT_Client_Application.pdf` or `.zip`). */
export function cigtDownloadName(
  response: HttpResponse<Blob>,
  clientName: string,
  applicationName: string,
): string {
  const headerName = fileNameFromContentDisposition(response.headers.get('content-disposition'));
  if (headerName) return headerName;

  const type = `${response.headers.get('content-type') ?? ''} ${response.body?.type ?? ''}`.toLowerCase();
  const extension = type.includes('zip') ? 'zip' : 'pdf';
  const safe = (value: string) => value.trim().replace(/[^\w.\-]+/g, '_') || 'Document';
  return `CIGT_${safe(clientName)}_${safe(applicationName)}.${extension}`;
}

function fileNameFromContentDisposition(header: string | null): string | null {
  if (!header) return null;

  const encoded = /filename\*=(?:UTF-8''|utf-8'')([^;]+)/i.exec(header);
  if (encoded?.[1]) {
    try {
      return decodeURIComponent(encoded[1].trim().replace(/"/g, ''));
    } catch {
      // Use the plain filename parameter below.
    }
  }

  const quoted = /filename="([^"]+)"/i.exec(header);
  if (quoted?.[1]) return quoted[1];

  const plain = /filename=([^;]+)/i.exec(header);
  const name = plain?.[1]?.trim().replace(/"/g, '');
  return name || null;
}
