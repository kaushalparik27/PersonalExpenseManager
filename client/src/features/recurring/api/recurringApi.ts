import { apiClient } from '@/lib/api/client'
import type {
  CreateRecurringExpensePayload,
  CreateRecurringIncomePayload,
  CreateRecurringTransferPayload,
  RecurringTransaction,
  UpdateRecurringTransactionPayload,
} from '@/features/recurring/types'

export const recurringApi = {
  list: () => apiClient.get<RecurringTransaction[]>('/api/recurring'),

  getById: (id: string) => apiClient.get<RecurringTransaction>(`/api/recurring/${id}`),

  createExpense: (payload: CreateRecurringExpensePayload) =>
    apiClient.post<RecurringTransaction>('/api/recurring/expense', payload),

  createIncome: (payload: CreateRecurringIncomePayload) =>
    apiClient.post<RecurringTransaction>('/api/recurring/income', payload),

  createTransfer: (payload: CreateRecurringTransferPayload) =>
    apiClient.post<RecurringTransaction>('/api/recurring/transfer', payload),

  update: (id: string, payload: UpdateRecurringTransactionPayload) =>
    apiClient.put<RecurringTransaction>(`/api/recurring/${id}`, payload),

  pause: (id: string) => apiClient.patch<RecurringTransaction>(`/api/recurring/${id}/pause`),

  resume: (id: string) => apiClient.patch<RecurringTransaction>(`/api/recurring/${id}/resume`),

  remove: (id: string) => apiClient.delete<void>(`/api/recurring/${id}`),
}
