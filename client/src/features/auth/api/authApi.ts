import { apiClient } from '@/lib/api/client'
import type {
  ChangePasswordPayload,
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
  UserProfile,
} from '@/features/auth/types'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5299'

export const authApi = {
  register: (payload: RegisterPayload) => apiClient.post<UserProfile>('/api/auth/register', payload),

  login: (payload: LoginPayload) => apiClient.post<UserProfile>('/api/auth/login', payload),

  logout: () => apiClient.post<Record<string, never>>('/api/auth/logout'),

  changePassword: (payload: ChangePasswordPayload) =>
    apiClient.post<Record<string, never>>('/api/auth/change-password', payload),

  forgotPassword: (payload: ForgotPasswordPayload) =>
    apiClient.post<{ message: string }>('/api/auth/forgot-password', payload),

  resetPassword: (payload: ResetPasswordPayload) =>
    apiClient.post<{ message: string }>('/api/auth/reset-password', payload),

  // Deliberately bypasses apiClient: a 401 here just means "not logged in yet",
  // which is a normal outcome on app load, not a session-expiry event to react to.
  async getCurrentUser(): Promise<UserProfile | null> {
    const response = await fetch(`${API_URL}/api/auth/me`, { credentials: 'include' })

    if (response.status === 401) {
      return null
    }

    if (!response.ok) {
      throw new Error('Unable to load your profile.')
    }

    const body = (await response.json()) as { data: UserProfile }
    return body.data
  },
}
