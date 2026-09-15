import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Loader2 } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/common/Combobox'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { RecurrenceFields } from '@/features/recurring/components/RecurrenceFields'
import { useAccounts } from '@/features/accounts/hooks/useAccounts'
import { recurringTransferFormSchema, type RecurringTransferFormValues } from '@/lib/validation/recurringTransactionSchema'

export interface RecurringTransferFormSubmitValues {
  accountId: string
  transferToAccountId: string
  amount: number
  description?: string
  frequency: RecurringTransferFormValues['frequency']
  startDate: string
  endDate?: string
}

interface RecurringTransferFormProps {
  defaultValues?: Partial<RecurringTransferFormValues>
  submitLabel: string
  onSubmit: (values: RecurringTransferFormSubmitValues) => Promise<void>
}

const today = () => new Date().toISOString().slice(0, 10)

export function RecurringTransferForm({ defaultValues, submitLabel, onSubmit }: RecurringTransferFormProps) {
  const { data: accounts } = useAccounts()

  const form = useForm<RecurringTransferFormValues>({
    resolver: zodResolver(recurringTransferFormSchema),
    defaultValues: {
      accountId: '',
      transferToAccountId: '',
      amount: undefined as unknown as number,
      description: '',
      frequency: 'Monthly',
      startDate: today(),
      endDate: '',
      ...defaultValues,
    },
  })

  const accountOptions = (accounts ?? [])
    .filter((a) => a.isActive)
    .map((a) => ({ value: a.id, label: a.name, hint: a.accountType }))

  async function handleSubmit(values: RecurringTransferFormValues) {
    await onSubmit({
      accountId: values.accountId,
      transferToAccountId: values.transferToAccountId,
      amount: values.amount,
      description: values.description || undefined,
      frequency: values.frequency,
      startDate: values.startDate,
      endDate: values.endDate || undefined,
    })
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="amount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Amount</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  step="0.01"
                  inputMode="decimal"
                  autoFocus
                  placeholder="0.00"
                  value={field.value ?? ''}
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

        <div className="flex items-end gap-2">
          <FormField
            control={form.control}
            name="accountId"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormLabel>From</FormLabel>
                <Combobox
                  options={accountOptions}
                  value={field.value}
                  onChange={field.onChange}
                  placeholder="Source account"
                  searchPlaceholder="Search accounts…"
                />
                <FormMessage />
              </FormItem>
            )}
          />

          <ArrowRight className="mb-2.5 size-4 shrink-0 text-muted-foreground" />

          <FormField
            control={form.control}
            name="transferToAccountId"
            render={({ field }) => (
              <FormItem className="flex-1">
                <FormLabel>To</FormLabel>
                <Combobox
                  options={accountOptions}
                  value={field.value}
                  onChange={field.onChange}
                  placeholder="Destination account"
                  searchPlaceholder="Search accounts…"
                />
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <RecurrenceFields control={form.control} />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description (optional)</FormLabel>
              <FormControl>
                <Input placeholder="e.g. Monthly SIP" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={form.formState.isSubmitting}>
          {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
          {submitLabel}
        </Button>
      </form>
    </Form>
  )
}
