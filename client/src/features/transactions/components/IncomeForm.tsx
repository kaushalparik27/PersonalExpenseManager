import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/common/Combobox'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { useAccounts } from '@/features/accounts/hooks/useAccounts'
import { useCategories } from '@/features/categories/hooks/useCategories'
import { incomeFormSchema, parseTagsText, type IncomeFormValues } from '@/lib/validation/transactionSchema'

export interface IncomeFormSubmitValues {
  transactionDate: string
  accountId: string
  amount: number
  incomeCategoryId: string
  incomeSubCategoryId?: string
  description?: string
  tags: string[]
}

interface IncomeFormProps {
  defaultValues?: Partial<IncomeFormValues>
  submitLabel: string
  onSubmit: (values: IncomeFormSubmitValues, addAnother: boolean) => Promise<void>
  showSaveAndAddAnother?: boolean
}

const today = () => new Date().toISOString().slice(0, 10)

export function IncomeForm({ defaultValues, submitLabel, onSubmit, showSaveAndAddAnother }: IncomeFormProps) {
  const { data: accounts } = useAccounts()
  const { data: categories } = useCategories('income')

  const form = useForm<IncomeFormValues>({
    resolver: zodResolver(incomeFormSchema),
    defaultValues: {
      transactionDate: today(),
      accountId: '',
      amount: undefined as unknown as number,
      incomeCategoryId: '',
      incomeSubCategoryId: undefined,
      description: '',
      tagsText: '',
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

  async function handleSubmit(values: IncomeFormValues, addAnother: boolean) {
    await onSubmit(
      {
        transactionDate: values.transactionDate,
        accountId: values.accountId,
        amount: values.amount,
        incomeCategoryId: values.incomeCategoryId,
        incomeSubCategoryId: values.incomeSubCategoryId || undefined,
        description: values.description || undefined,
        tags: parseTagsText(values.tagsText),
      },
      addAnother,
    )

    if (addAnother) {
      form.reset({
        transactionDate: values.transactionDate,
        accountId: values.accountId,
        amount: undefined as unknown as number,
        incomeCategoryId: values.incomeCategoryId,
        incomeSubCategoryId: values.incomeSubCategoryId,
        description: '',
        tagsText: '',
      })
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit((values) => handleSubmit(values, false))} className="space-y-4">
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

        <FormField
          control={form.control}
          name="transactionDate"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Date</FormLabel>
              <FormControl>
                <Input type="date" {...field} />
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
                <Input placeholder="e.g. Monthly salary" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="tagsText"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tags (optional, comma-separated)</FormLabel>
              <FormControl>
                <Input placeholder="e.g. recurring" {...field} />
              </FormControl>
            </FormItem>
          )}
        />

        <div className="flex gap-2">
          <Button type="submit" className="flex-1" disabled={form.formState.isSubmitting}>
            {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
            {submitLabel}
          </Button>
          {showSaveAndAddAnother && (
            <Button
              type="button"
              variant="outline"
              disabled={form.formState.isSubmitting}
              onClick={form.handleSubmit((values) => handleSubmit(values, true))}
            >
              Save & Add Another
            </Button>
          )}
        </div>
      </form>
    </Form>
  )
}
