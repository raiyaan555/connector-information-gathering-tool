import { Injectable } from '@angular/core';

export type DocType = 'pdf' | 'word' | 'ppt';
export type SaveType = 'Official' | 'Change Request';

export interface AttachmentMeta {
  name: string;
  type: string;
  size: number;
  uploadedAt: string; // ISO string
}

export interface DocumentGenerationOptions {
  includeConnectorForm: boolean;
  includeUploadedApiDocumentation: boolean;
  includeSwaggerFiles: boolean;
  includePostmanCollection: boolean;
  includeArchitectureDiagrams: boolean;
  includeScreenshots: boolean;
  includeCredentialsDocument: boolean;
  includeAdditionalUploadedFiles: boolean;
}

export interface GeneratedDocumentEntry {
  id: string;
  versionNumber: number;
  docType: DocType;
  fileName: string;
  createdAt: string; // ISO
  options: DocumentGenerationOptions;
  includedAttachmentNames: string[];
}

export interface ProjectVersionEntry {
  versionNumber: number;
  saveType: SaveType;
  createdAt: string; // ISO
  modifiedAt: string; // ISO
  formData: Record<string, unknown>;
  attachments: AttachmentMeta[];
  completionPercent: number;
}

export interface ProjectDocumentState {
  projectId: string;
  createdAt: string; // ISO
  versions: ProjectVersionEntry[];
  generatedDocuments: GeneratedDocumentEntry[];
}

const STORAGE_PREFIX = 'cigt_project_doc_';

function storageKey(projectId: string): string {
  return STORAGE_PREFIX + projectId;
}

function newId(): string {
  return Math.random().toString(16).slice(2) + '_' + Date.now().toString(16);
}

@Injectable({ providedIn: 'root' })
export class ProjectDocumentRepository {
  private readState(projectId: string): ProjectDocumentState | null {
    try {
      const raw = localStorage.getItem(storageKey(projectId));
      if (!raw) return null;
      return JSON.parse(raw) as ProjectDocumentState;
    } catch {
      return null;
    }
  }

  private writeState(state: ProjectDocumentState): void {
    localStorage.setItem(storageKey(state.projectId), JSON.stringify(state));
  }

  getOrCreateState(projectId: string): ProjectDocumentState {
    const existing = this.readState(projectId);
    if (existing) return existing;
    const state: ProjectDocumentState = {
      projectId,
      createdAt: new Date().toISOString(),
      versions: [],
      generatedDocuments: [],
    };
    this.writeState(state);
    return state;
  }

  getVersions(projectId: string): ProjectVersionEntry[] {
    // Latest versions first (higher versionNumber on top).
    return this.getOrCreateState(projectId).versions.slice().sort((a, b) => b.versionNumber - a.versionNumber);
  }

  getLatestVersion(projectId: string): ProjectVersionEntry | null {
    const versions = this.getVersions(projectId);
    return versions.length ? versions[0] : null;
  }

  saveVersion(params: {
    projectId: string;
    formData: Record<string, unknown>;
    attachments: AttachmentMeta[];
    completionPercent: number;
  }): ProjectVersionEntry {
    const state = this.getOrCreateState(params.projectId);
    const nextVersion = state.versions.length ? state.versions[state.versions.length - 1].versionNumber + 1 : 1;
    const saveType: SaveType = nextVersion === 1 ? 'Official' : 'Change Request';

    const now = new Date().toISOString();
    const version: ProjectVersionEntry = {
      versionNumber: nextVersion,
      saveType,
      createdAt: now,
      modifiedAt: now,
      formData: params.formData,
      attachments: params.attachments,
      completionPercent: params.completionPercent,
    };

    state.versions.push(version);
    this.writeState(state);
    return version;
  }

  listGeneratedDocuments(projectId: string, versionNumber?: number): GeneratedDocumentEntry[] {
    const state = this.getOrCreateState(projectId);
    const docs = state.generatedDocuments.slice();
    docs.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    return versionNumber ? docs.filter((d) => d.versionNumber === versionNumber) : docs;
  }

  addGeneratedDocument(params: {
    projectId: string;
    versionNumber: number;
    docType: DocType;
    fileName: string;
    options: DocumentGenerationOptions;
    includedAttachmentNames: string[];
  }): GeneratedDocumentEntry {
    const state = this.getOrCreateState(params.projectId);
    const entry: GeneratedDocumentEntry = {
      id: newId(),
      versionNumber: params.versionNumber,
      docType: params.docType,
      fileName: params.fileName,
      createdAt: new Date().toISOString(),
      options: params.options,
      includedAttachmentNames: params.includedAttachmentNames,
    };
    state.generatedDocuments.push(entry);
    this.writeState(state);
    return entry;
  }

  deleteGeneratedDocument(projectId: string, docId: string): boolean {
    const state = this.getOrCreateState(projectId);
    const before = state.generatedDocuments.length;
    state.generatedDocuments = state.generatedDocuments.filter((d) => d.id !== docId);
    const after = state.generatedDocuments.length;
    this.writeState(state);
    return before !== after;
  }
}

