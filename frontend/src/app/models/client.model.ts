export interface Client {
  id: string;
  companyName: string;
  industry: string;
  primaryContact: string;
  email: string;
  phone: string;
  country: string;
  address: string;
  notes: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateClientRequest {
  companyName: string;
  industry: string;
  primaryContact: string;
  email: string;
  phone: string;
  country: string;
  address: string;
  notes: string;
}

