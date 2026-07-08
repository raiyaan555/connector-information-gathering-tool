export interface CustomerFormInfo {
  projectId: string;
  projectName: string;
  clientName: string;
  applicationName: string;
  token: string;
  isSubmitted: boolean;
}

export interface CustomerFormResponse {
  id: string;
  projectId: string;
  token: string;
  formData: Record<string, string>;
  submittedAt: string;
}

export interface SubmitCustomerFormRequest {
  formData: Record<string, string>;
}
