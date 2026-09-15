import { z } from 'zod'

const frequencySchema = z.enum(['Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly'])

function withEndDateRule<T extends z.ZodRawShape>(schema: z.ZodObject<T>) {
  return schema.refine(
    (data) => {
      const { startDate, endDate } = data as { startDate: string; endDate: string }
      return !endDate || endDate >= startDate
    },
    { message: 'End date must be on or after the start date.', path: ['endDate'] },
  )
}

export const recurringExpenseFormSchema = withEndDateRule(
  z.object({
    accountId: z.string().min(1, 'Account is required.'),
    amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
    expenseCategoryId: z.string().min(1, 'Category is required.'),
    expenseSubCategoryId: z.string().optional(),
    description: z.string().max(500),
    frequency: frequencySchema,
    startDate: z.string().min(1, 'Start date is required.'),
    endDate: z.string(),
  }),
)

export type RecurringExpenseFormValues = z.infer<typeof recurringExpenseFormSchema>

export const recurringIncomeFormSchema = withEndDateRule(
  z.object({
    accountId: z.string().min(1, 'Account is required.'),
    amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
    incomeCategoryId: z.string().min(1, 'Category is required.'),
    incomeSubCategoryId: z.string().optional(),
    description: z.string().max(500),
    frequency: frequencySchema,
    startDate: z.string().min(1, 'Start date is required.'),
    endDate: z.string(),
  }),
)

export type RecurringIncomeFormValues = z.infer<typeof recurringIncomeFormSchema>

export const recurringTransferFormSchema = withEndDateRule(
  z.object({
    accountId: z.string().min(1, 'Source account is required.'),
    transferToAccountId: z.string().min(1, 'Destination account is required.'),
    amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
    description: z.string().max(500),
    frequency: frequencySchema,
    startDate: z.string().min(1, 'Start date is required.'),
    endDate: z.string(),
  }),
).refine((data) => data.accountId !== data.transferToAccountId, {
  message: 'Cannot transfer an account to itself.',
  path: ['transferToAccountId'],
})

export type RecurringTransferFormValues = z.infer<typeof recurringTransferFormSchema>
