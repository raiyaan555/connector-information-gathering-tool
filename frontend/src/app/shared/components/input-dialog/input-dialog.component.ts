import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export interface InputDialogData {
  title: string;
  label: string;
  placeholder?: string;
  confirmText?: string;
  cancelText?: string;
  initialValue?: string;
}

@Component({
  selector: 'app-input-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ data.label }}</mat-label>
          <input matInput formControlName="value" [placeholder]="data.placeholder || ''" />
          @if (form.controls.value.hasError('required') && form.controls.value.touched) {
            <mat-error>Required</mat-error>
          }
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close()">{{ data.cancelText || 'Cancel' }}</button>
      <button mat-flat-button color="primary" type="button" (click)="submit()" [disabled]="form.invalid">
        {{ data.confirmText || 'Save' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .full-width { width: 100%; min-width: 320px; }
    mat-dialog-content { padding-top: 8px; }
  `,
})
export class InputDialogComponent {
  readonly data = inject<InputDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<InputDialogComponent>);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    value: [this.data.initialValue || '', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.controls.value.value.trim());
  }
}
