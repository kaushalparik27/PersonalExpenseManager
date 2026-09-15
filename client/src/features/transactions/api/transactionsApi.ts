import { apiClient } from '@/lib/api/client'
import type {
  CreateExpensePayload,
  CreateIncomePayload,
  CreateTransferPayload,
  PagedResult,
  Transaction,
  TransactionListFilters,
  UpdateTransactionPayload,
} from '@/features/transactions/types'

function buildQueryString(filters: TransactionListFilters): string {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(filters)) {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  }

  const query = params.toString()
  return query ? `?${query}` : ''
}

export const transactionsApi = {
  list: (filters: TransactionListFilters) =>
    apiClient.get<PagedResult<Transaction>>(`/api/transactions${buildQueryString(filters)}`),

  getById: (id: string) => apiClient.get<Transaction>(`/api/transactions/${id}`),

  createExpense: (payload: CreateExpensePayload) =>
    apiClient.post<Transaction>('/api/transactions/expense', payload),

  createIncome: (payload: CreateIncomePayload) =>
    apiClient.post<Transaction>('/api/transactions/income', payload),

  createTransfer: (payload: CreateTransferPayload) =>
    apiClient.post<Transaction>('/api/transactions/transfer', payload),

  update: (id: string, payload: UpdateTransactionPayload) =>
    apiClient.put<Transaction>(`/api/transactions/${id}`, payload),

  remove: (id: string) => apiClient.delete<void>(`/api/transactions/${id}`),
}
