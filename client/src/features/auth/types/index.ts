export interface UserProfile {
  id: string
  firstName: string
  lastName: string
  email: string
}

export interface RegisterPayload {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
}

export interface LoginPayload {
  email: string
  password: string
}

export interface ForgotPasswordPayload {
  email: string
}

export interface ResetPasswordPayload {
  email: string
  token: string
  newPassword: string
  confirmNewPassword: string
}

export interface ChangePasswordPayload {
  currentPassword: string
  newPassword: string
  confirmNewPassword: string
}
