export interface Attachment {
  id: string;
  projectId: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
  uploadedBy: string;
}

export interface UploadAttachmentRequest {
  fileName: string;
  contentType: string;
  fileSize: number;
}
