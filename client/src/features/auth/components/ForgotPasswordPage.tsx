import { zodResolver } from '@hookform/resolvers/zod'
import { CheckCircle2, Loader2 } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { authApi } from '@/features/auth/api/authApi'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '@/lib/validation/authSchemas'

export function ForgotPasswordPage() {
  const [submitted, setSubmitted] = useState(false)

  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: '' },
  })

  async function onSubmit(values: ForgotPasswordFormValues) {
    // Backend always returns a generic success response regardless of whether
    // the email exists, so there is nothing user-specific to branch on here.
    await authApi.forgotPassword(values)
    setSubmitted(true)
  }

  if (submitted) {
    return (
      <div className="space-y-4 text-center">
        <CheckCircle2 className="mx-auto size-8 text-success" />
        <div className="space-y-1">
          <h1 className="text-lg font-semibold text-foreground">Check your email</h1>
          <p className="text-sm text-muted-foreground">
            If an account exists for that email, we've sent a link to reset your password.
          </p>
        </div>
        <Link to="/login" className="text-sm font-medium text-primary hover:underline">
          Back to sign in
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="space-y-1 text-center">
        <h1 className="text-lg font-semibold text-foreground">Forgot password</h1>
        <p className="text-sm text-muted-foreground">Enter your email and we'll send you a reset link.</p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input type="email" autoComplete="email" autoFocus placeholder="you@example.com" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button type="submit" className="w-full" disabled={form.formState.isSubmitting}>
            {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
            Send reset link
          </Button>
        </form>
      </Form>

      <p className="text-center text-sm text-muted-foreground">
        <Link to="/login" className="font-medium text-primary hover:underline">
          Back to sign in
        </Link>
      </p>
    </div>
  )
}
