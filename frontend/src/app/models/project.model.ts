export type ProjectStatus = 'Draft' | 'In Progress' | 'Completed' | 'Pending Review';

export interface Project {
  id: string;
  name: string;
  clientName: string;
  applicationName: string;
  description?: string;
  implementationEngineer?: string;
  priority?: string;
  expectedCompletionDate?: string;
  completionPercent?: number;
  status: ProjectStatus;
  formToken?: string;
  formLink?: string;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
}

export interface CreateProjectRequest {
  name: string;
  clientName: string;
  applicationName: string;
  description?: string;
  implementationEngineer?: string;
  priority?: string;
  expectedCompletionDate?: string;
  status?: string;
}

export interface UpdateProjectRequest {
  name: string;
  clientName: string;
  applicationName: string;
  status: string;
}

export interface GenerateLinkResponse {
  token: string;
  formLink: string;
}
