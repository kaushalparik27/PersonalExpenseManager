import { z } from 'zod'

const passwordSchema = z
  .string()
  .min(8, 'Password must be at least 8 characters.')
  .regex(/[A-Z]/, 'Password must contain an uppercase letter.')
  .regex(/[a-z]/, 'Password must contain a lowercase letter.')
  .regex(/[0-9]/, 'Password must contain a digit.')
  .regex(/[^a-zA-Z0-9]/, 'Password must contain a special character.')

export const registerSchema = z
  .object({
    firstName: z.string().min(1, 'First name is required.'),
    lastName: z.string().min(1, 'Last name is required.'),
    email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
    password: passwordSchema,
    confirmPassword: z.string().min(1, 'Confirm your password.'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })

export type RegisterFormValues = z.infer<typeof registerSchema>

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
  password: z.string().min(1, 'Password is required.'),
})

export type LoginFormValues = z.infer<typeof loginSchema>

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
})

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>

export const resetPasswordSchema = z
  .object({
    newPassword: passwordSchema,
    confirmNewPassword: z.string().min(1, 'Confirm your new password.'),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    message: 'Passwords do not match.',
    path: ['confirmNewPassword'],
  })

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required.'),
    newPassword: passwordSchema,
    confirmNewPassword: z.string().min(1, 'Confirm your new password.'),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    message: 'Passwords do not match.',
    path: ['confirmNewPassword'],
  })

export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>
