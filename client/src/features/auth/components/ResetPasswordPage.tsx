import { zodResolver } from '@hookform/resolvers/zod'
import { AlertCircle, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { authApi } from '@/features/auth/api/authApi'
import { ApiRequestError } from '@/lib/api/client'
import { resetPasswordSchema, type ResetPasswordFormValues } from '@/lib/validation/authSchemas'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [formError, setFormError] = useState<string | null>(null)

  const email = searchParams.get('email')
  const token = searchParams.get('token')

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '', confirmNewPassword: '' },
  })

  if (!email || !token) {
    return (
      <div className="space-y-4 text-center">
        <AlertCircle className="mx-auto size-8 text-destructive" />
        <div className="space-y-1">
          <h1 className="text-lg font-semibold text-foreground">Invalid reset link</h1>
          <p className="text-sm text-muted-foreground">This password reset link is missing or malformed.</p>
        </div>
        <Link to="/forgot-password" className="text-sm font-medium text-primary hover:underline">
          Request a new link
        </Link>
      </div>
    )
  }

  async function onSubmit(values: ResetPasswordFormValues) {
    setFormError(null)
    try {
      await authApi.resetPassword({ email: email!, token: token!, ...values })
      navigate('/login', { replace: true })
    } catch (error) {
      setFormError(
        error instanceof ApiRequestError ? error.message : 'Unable to reset your password. Please try again.',
      )
    }
  }

  return (
    <div className="space-y-6">
      <div className="space-y-1 text-center">
        <h1 className="text-lg font-semibold text-foreground">Reset password</h1>
        <p className="text-sm text-muted-foreground">Choose a new password for your account.</p>
      </div>

      {formError && (
        <div className="flex items-start gap-2 rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
          <AlertCircle className="mt-0.5 size-4 shrink-0" />
          <span>{formError}</span>
        </div>
      )}

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="newPassword"
            render={({ field }) => (
              <FormItem>
                <FormLabel>New Password</FormLabel>
                <FormControl>
                  <Input type="password" autoComplete="new-password" autoFocus {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="confirmNewPassword"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Confirm Password</FormLabel>
                <FormControl>
                  <Input type="password" autoComplete="new-password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button type="submit" className="w-full" disabled={form.formState.isSubmitting}>
            {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
            Reset Password
          </Button>
        </form>
      </Form>
    </div>
  )
}
