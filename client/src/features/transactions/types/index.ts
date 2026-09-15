export type TransactionType = 'Income' | 'Expense' | 'Transfer' | 'Adjustment'

export interface Transaction {
  id: string
  transactionDate: string
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
  notes: string | null
  payee: string | null
  referenceNumber: string | null
  tags: string[]
  createdAt: string
  updatedAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface TransactionListFilters {
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  fromDate?: string
  toDate?: string
  accountId?: string
  categoryId?: string
  subCategoryId?: string
  transactionType?: TransactionType
  search?: string
  minAmount?: number
  maxAmount?: number
}

export interface CreateExpensePayload {
  transactionDate: string
  accountId: string
  amount: number
  expenseCategoryId: string
  expenseSubCategoryId?: string | null
  description?: string | null
  notes?: string | null
  payee?: string | null
  referenceNumber?: string | null
  tags?: string[]
}

export interface CreateIncomePayload {
  transactionDate: string
  accountId: string
  amount: number
  incomeCategoryId: string
  incomeSubCategoryId?: string | null
  description?: string | null
  notes?: string | null
  payee?: string | null
  referenceNumber?: string | null
  tags?: string[]
}

export interface CreateTransferPayload {
  transactionDate: string
  accountId: string
  transferToAccountId: string
  amount: number
  description?: string | null
  notes?: string | null
  tags?: string[]
}

export interface UpdateTransactionPayload {
  transactionDate: string
  accountId: string
  amount: number
  transferToAccountId?: string | null
  expenseCategoryId?: string | null
  expenseSubCategoryId?: string | null
  incomeCategoryId?: string | null
  incomeSubCategoryId?: string | null
  description?: string | null
  notes?: string | null
  payee?: string | null
  referenceNumber?: string | null
  tags?: string[]
}
