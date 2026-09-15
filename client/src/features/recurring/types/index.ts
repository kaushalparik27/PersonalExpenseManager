import type { TransactionType } from '@/features/transactions/types'

export type RecurrenceFrequency = 'Daily' | 'Weekly' | 'Monthly' | 'Quarterly' | 'Yearly'

export interface RecurringTransaction {
  id: string
  transactionType: TransactionType
  accountId: string
  accountName: string
  transferToAccountId: string | null
  transferToAccountName: string | null
  amount: number
  expenseCategoryId: string | null
  expenseCategoryName: string | null
  expenseSubCategoryId: string | null
  expenseSubCategoryName: string | null
  incomeCategoryId: string | null
  incomeCategoryName: string | null
  incomeSubCategoryId: string | null
  incomeSubCategoryName: string | null
  description: string | null
  frequency: RecurrenceFrequency
  startDate: string
  endDate: string | null
  nextOccurrenceDate: string
  isActive: boolean
}

export interface CreateRecurringExpensePayload {
  accountId: string
  amount: number
  expenseCategoryId: string
  expenseSubCategoryId?: string | null
  description?: string | null
  frequency: RecurrenceFrequency
  startDate: string
  endDate?: string | null
}

export interface CreateRecurringIncomePayload {
  accountId: string
  amount: number
  incomeCategoryId: string
  incomeSubCategoryId?: string | null
  description?: string | null
  frequency: RecurrenceFrequency
  startDate: string
  endDate?: string | null
}

export interface CreateRecurringTransferPayload {
  accountId: string
  transferToAccountId: string
  amount: number
  description?: string | null
  frequency: RecurrenceFrequency
  startDate: string
  endDate?: string | null
}

export interface UpdateRecurringTransactionPayload {
  accountId: string
  amount: number
  transferToAccountId?: string | null
  expenseCategoryId?: string | null
  expenseSubCategoryId?: string | null
  incomeCategoryId?: string | null
  incomeSubCategoryId?: string | null
  description?: string | null
  frequency: RecurrenceFrequency
  startDate: string
  endDate?: string | null
}
