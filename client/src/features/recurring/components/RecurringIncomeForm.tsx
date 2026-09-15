import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/common/Combobox'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { RecurrenceFields } from '@/features/recurring/components/RecurrenceFields'
import { useAccounts } from '@/features/accounts/hooks/useAccounts'
import { useCategories } from '@/features/categories/hooks/useCategories'
import { recurringIncomeFormSchema, type RecurringIncomeFormValues } from '@/lib/validation/recurringTransactionSchema'

export interface RecurringIncomeFormSubmitValues {
  accountId: string
  amount: number
  incomeCategoryId: string
  incomeSubCategoryId?: string
  description?: string
  frequency: RecurringIncomeFormValues['frequency']
  startDate: string
  endDate?: string
}

interface RecurringIncomeFormProps {
  defaultValues?: Partial<RecurringIncomeFormValues>
  submitLabel: string
  onSubmit: (values: RecurringIncomeFormSubmitValues) => Promise<void>
}

const today = () => new Date().toISOString().slice(0, 10)

export function RecurringIncomeForm({ defaultValues, submitLabel, onSubmit }: RecurringIncomeFormProps) {
  const { data: accounts } = useAccounts()
  const { data: categories } = useCategories('income')

  const form = useForm<RecurringIncomeFormValues>({
    resolver: zodResolver(recurringIncomeFormSchema),
    defaultValues: {
      accountId: '',
      amount: undefined as unknown as number,
      incomeCategoryId: '',
      incomeSubCategoryId: undefined,
      description: '',
      frequency: 'Monthly',
      startDate: today(),
      endDate: '',
      ...defaultValues,
    },
  })

  const selectedCategoryId = form.watch('incomeCategoryId')
  const subCategoryOptions = useMemo(() => {
    const category = categories?.find((c) => c.id === selectedCategoryId)
    return (category?.subCategories ?? [])
      .filter((s) => !s.isArchived)
      .map((s) => ({ value: s.id, label: s.name }))
  }, [categories, selectedCategoryId])

  useEffect(() => {
    if (!subCategoryOptions.some((o) => o.value === form.getValues('incomeSubCategoryId'))) {
      form.setValue('incomeSubCategoryId', undefined)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedCategoryId])

  const accountOptions = (accounts ?? [])
    .filter((a) => a.isActive)
    .map((a) => ({ value: a.id, label: a.name, hint: a.accountType }))

  const categoryOptions = (categories ?? [])
    .filter((c) => !c.isArchived)
    .map((c) => ({ value: c.id, label: c.name }))

  async function handleSubmit(values: RecurringIncomeFormValues) {
    await onSubmit({
      accountId: values.accountId,
      amount: values.amount,
      incomeCategoryId: values.incomeCategoryId,
      incomeSubCategoryId: values.incomeSubCategoryId || undefined,
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

        <FormField
          control={form.control}
          name="accountId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Account</FormLabel>
              <Combobox
                options={accountOptions}
                value={field.value}
                onChange={field.onChange}
                placeholder="Select account"
                searchPlaceholder="Search accounts…"
              />
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-3">
          <FormField
            control={form.control}
            name="incomeCategoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Category</FormLabel>
                <Combobox
                  options={categoryOptions}
                  value={field.value}
                  onChange={field.onChange}
                  placeholder="Select category"
                  searchPlaceholder="Search categories…"
                />
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="incomeSubCategoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Subcategory</FormLabel>
                <Combobox
                  options={subCategoryOptions}
                  value={field.value}
                  onChange={field.onChange}
                  placeholder="Optional"
                  searchPlaceholder="Search…"
                  disabled={subCategoryOptions.length === 0}
                />
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
                <Input placeholder="e.g. Salary" {...field} />
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
