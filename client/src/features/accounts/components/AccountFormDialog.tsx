import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { accountTypeOptions } from '@/features/accounts/accountTypeMeta'
import { useCreateAccount, useUpdateAccount } from '@/features/accounts/hooks/useAccountMutations'
import type { Account } from '@/features/accounts/types'
import { ApiRequestError } from '@/lib/api/client'
import { accountSchema, type AccountFormSchema } from '@/lib/validation/accountSchema'

interface AccountFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  account?: Account
}

export function AccountFormDialog({ open, onOpenChange, account }: AccountFormDialogProps) {
  const isEditMode = Boolean(account)
  const createAccount = useCreateAccount()
  const updateAccount = useUpdateAccount(account?.id ?? '')

  const form = useForm<AccountFormSchema>({
    resolver: zodResolver(accountSchema),
    defaultValues: {
      name: '',
      accountType: 'Bank',
      description: '',
      openingBalance: 0,
      currency: 'INR',
      icon: '',
      displayOrder: 0,
    },
  })

  useEffect(() => {
    if (!open) {
      return
    }

    form.reset(
      account
        ? {
            name: account.name,
            accountType: account.accountType,
            description: account.description ?? '',
            openingBalance: account.openingBalance,
            currency: account.currency,
            icon: account.icon ?? '',
            displayOrder: account.displayOrder,
          }
        : {
            name: '',
            accountType: 'Bank',
            description: '',
            openingBalance: 0,
            currency: 'INR',
            icon: '',
            displayOrder: 0,
          },
    )
  }, [open, account, form])

  async function onSubmit(values: AccountFormSchema) {
    try {
      if (isEditMode && account) {
        await updateAccount.mutateAsync({ ...values, isActive: account.isActive })
        toast.success('Account updated.')
      } else {
        await createAccount.mutateAsync(values)
        toast.success('Account created.')
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the account.')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditMode ? 'Edit Account' : 'New Account'}</DialogTitle>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input autoFocus placeholder="e.g. HDFC Bank" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-3">
              <FormField
                control={form.control}
                name="accountType"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Type</FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {accountTypeOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="currency"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Currency</FormLabel>
                    <FormControl>
                      <Input placeholder="INR" maxLength={3} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="openingBalance"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Opening Balance</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="0.01"
                      value={field.value}
                      onChange={(e) => field.onChange(e.target.valueAsNumber)}
                      onBlur={field.onBlur}
                      name={field.name}
                      ref={field.ref}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Description (optional)</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g. Primary salary account" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="submit" disabled={form.formState.isSubmitting}>
                {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
                {isEditMode ? 'Save Changes' : 'Create Account'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
