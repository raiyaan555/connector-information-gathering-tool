import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Matches ASP.NET Identity password policy in API/Program.cs */
export const passwordPolicyValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string;
  if (!value) return null;

  const errors: ValidationErrors = {};
  if (value.length < 8) errors['minLength'] = true;
  if (!/[A-Z]/.test(value)) errors['uppercase'] = true;
  if (!/[a-z]/.test(value)) errors['lowercase'] = true;
  if (!/\d/.test(value)) errors['digit'] = true;

  return Object.keys(errors).length ? errors : null;
};
