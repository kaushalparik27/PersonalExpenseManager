import { apiClient } from '@/lib/api/client'
import type { Account, AccountFormValues, UpdateAccountPayload } from '@/features/accounts/types'

export const accountsApi = {
  list: () => apiClient.get<Account[]>('/api/accounts'),

  getById: (id: string) => apiClient.get<Account>(`/api/accounts/${id}`),

  create: (payload: AccountFormValues) => apiClient.post<Account>('/api/accounts', payload),

  update: (id: string, payload: UpdateAccountPayload) =>
    apiClient.put<Account>(`/api/accounts/${id}`, payload),

  archive: (id: string) => apiClient.patch<void>(`/api/accounts/${id}/archive`),

  remove: (id: string) => apiClient.delete<void>(`/api/accounts/${id}`),
}
