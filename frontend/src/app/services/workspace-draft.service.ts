import { Injectable, signal } from '@angular/core';

export interface WorkspaceDraft {
  projectId: string;
  formData: Record<string, unknown>;
  attachments: Array<{ name: string; type: string; size: number; uploadedAt: string }>;
  lastSavedAt: string;
  completionPercent: number;
}

const STORAGE_PREFIX = 'cigt_workspace_';

@Injectable({ providedIn: 'root' })
export class WorkspaceDraftService {
  private readonly lastSaved = signal<Date | null>(null);
  readonly lastSavedAt = this.lastSaved.asReadonly();

  clear(projectId: string): void {
    try {
      localStorage.removeItem(STORAGE_PREFIX + projectId);
    } finally {
      this.lastSaved.set(null);
    }
  }

  load(projectId: string): WorkspaceDraft | null {
    try {
      const raw = localStorage.getItem(STORAGE_PREFIX + projectId);
      if (!raw) return null;
      const draft = JSON.parse(raw) as WorkspaceDraft;
      if (draft.lastSavedAt) {
        this.lastSaved.set(new Date(draft.lastSavedAt));
      }
      return draft;
    } catch {
      return null;
    }
  }

  save(projectId: string, formData: Record<string, unknown>, attachments: WorkspaceDraft['attachments'], completionPercent: number): void {
    const draft: WorkspaceDraft = {
      projectId,
      formData,
      attachments,
      lastSavedAt: new Date().toISOString(),
      completionPercent,
    };
    localStorage.setItem(STORAGE_PREFIX + projectId, JSON.stringify(draft));
    this.lastSaved.set(new Date(draft.lastSavedAt));
  }

  lastSavedLabel(): string {
    const d = this.lastSaved();
    if (!d) return 'Not saved yet';
    const mins = Math.floor((Date.now() - d.getTime()) / 60000);
    if (mins < 1) return 'Just now';
    if (mins === 1) return '1 minute ago';
    return `${mins} minutes ago`;
  }

  computeCompletion(formData: Record<string, unknown>, requiredKeys: string[]): number {
    if (!requiredKeys.length) return 0;
    const filled = requiredKeys.filter((k) => {
      const v = formData[k];
      if (Array.isArray(v)) return v.length > 0;
      return v !== null && v !== undefined && String(v).trim() !== '';
    }).length;
    return Math.round((filled / requiredKeys.length) * 100);
  }
}
