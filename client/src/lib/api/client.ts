const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5299'

export interface ApiFieldError {
  field: string
  message: string
}

interface ApiEnvelope<T> {
  success: boolean
  data: T | null
  errors: ApiFieldError[]
}

export class ApiRequestError extends Error {
  status: number
  errors: ApiFieldError[]

  constructor(status: number, message: string, errors: ApiFieldError[] = []) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

// Dispatched whenever an authenticated request comes back 401 mid-session (not
// on the initial "am I logged in" check). AuthProvider listens for this to clear
// cached user state and let ProtectedRoute redirect to /login.
export const UNAUTHORIZED_EVENT = 'auth:unauthorized'

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent(UNAUTHORIZED_EVENT))
  }

  const isJson = response.headers.get('content-type')?.includes('application/json') ?? false
  const body = isJson ? ((await response.json()) as ApiEnvelope<T>) : null

  if (!response.ok) {
    const message = body?.errors?.[0]?.message ?? 'Something went wrong. Please try again.'
    throw new ApiRequestError(response.status, message, body?.errors ?? [])
  }

  return (body?.data ?? undefined) as T
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, data?: unknown) =>
    request<T>(path, {
      method: 'POST',
      body: data === undefined ? undefined : JSON.stringify(data),
    }),
  put: <T>(path: string, data?: unknown) =>
    request<T>(path, {
      method: 'PUT',
      body: data === undefined ? undefined : JSON.stringify(data),
    }),
  patch: <T>(path: string, data?: unknown) =>
    request<T>(path, {
      method: 'PATCH',
      body: data === undefined ? undefined : JSON.stringify(data),
    }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
