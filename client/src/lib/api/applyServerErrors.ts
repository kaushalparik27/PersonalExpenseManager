import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import type { ApiFieldError } from '@/lib/api/client'

// Maps backend field errors (PascalCase, e.g. "Email") onto RHF field names
// (camelCase, e.g. "email") and returns any error that isn't field-specific.
export function applyServerErrors<T extends FieldValues>(
  errors: ApiFieldError[],
  setError: UseFormSetError<T>,
): string | null {
  let generalMessage: string | null = null

  for (const error of errors) {
    if (!error.field) {
      generalMessage = error.message
      continue
    }

    const fieldName = (error.field.charAt(0).toLowerCase() + error.field.slice(1)) as Path<T>
    setError(fieldName, { type: 'server', message: error.message })
  }

  return generalMessage
}
