import { z } from 'zod'

export const expenseFormSchema = z.object({
  transactionDate: z.string().min(1, 'Date is required.'),
  accountId: z.string().min(1, 'Account is required.'),
  amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
  expenseCategoryId: z.string().min(1, 'Category is required.'),
  expenseSubCategoryId: z.string().optional(),
  description: z.string().max(500),
  tagsText: z.string(),
})

export type ExpenseFormValues = z.infer<typeof expenseFormSchema>

export const incomeFormSchema = z.object({
  transactionDate: z.string().min(1, 'Date is required.'),
  accountId: z.string().min(1, 'Account is required.'),
  amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
  incomeCategoryId: z.string().min(1, 'Category is required.'),
  incomeSubCategoryId: z.string().optional(),
  description: z.string().max(500),
  tagsText: z.string(),
})

export type IncomeFormValues = z.infer<typeof incomeFormSchema>

export const transferFormSchema = z
  .object({
    transactionDate: z.string().min(1, 'Date is required.'),
    accountId: z.string().min(1, 'Source account is required.'),
    transferToAccountId: z.string().min(1, 'Destination account is required.'),
    amount: z.number({ message: 'Enter a valid amount.' }).positive('Amount must be greater than zero.'),
    description: z.string().max(500),
  })
  .refine((data) => data.accountId !== data.transferToAccountId, {
    message: 'Cannot transfer an account to itself.',
    path: ['transferToAccountId'],
  })

export type TransferFormValues = z.infer<typeof transferFormSchema>

export function parseTagsText(tagsText: string): string[] {
  return tagsText
    .split(',')
    .map((tag) => tag.trim())
    .filter((tag) => tag.length > 0)
}
