import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { authApi } from '@/features/auth/api/authApi'
import { applyServerErrors } from '@/lib/api/applyServerErrors'
import { ApiRequestError } from '@/lib/api/client'
import { changePasswordSchema, type ChangePasswordFormValues } from '@/lib/validation/authSchemas'

export function ChangePasswordForm() {
  const form = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: '', newPassword: '', confirmNewPassword: '' },
  })

  async function onSubmit(values: ChangePasswordFormValues) {
    try {
      await authApi.changePassword(values)
      toast.success('Password changed successfully.')
      form.reset()
    } catch (error) {
      if (error instanceof ApiRequestError) {
        const general = applyServerErrors<ChangePasswordFormValues>(error.errors, form.setError)
        toast.error(general ?? error.message)
      } else {
        toast.error('Unable to change your password. Please try again.')
      }
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="max-w-sm space-y-4">
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Current Password</FormLabel>
              <FormControl>
                <Input type="password" autoComplete="current-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>New Password</FormLabel>
              <FormControl>
                <Input type="password" autoComplete="new-password" {...field} />
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
              <FormLabel>Confirm New Password</FormLabel>
              <FormControl>
                <Input type="password" autoComplete="new-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
          Change Password
        </Button>
      </form>
    </Form>
  )
}
