import { useQuery } from '@tanstack/react-query'
import { recurringApi } from '@/features/recurring/api/recurringApi'

export const recurringQueryKey = ['recurring'] as const

export function useRecurringTransactions() {
  return useQuery({
    queryKey: recurringQueryKey,
    queryFn: recurringApi.list,
  })
}

export function useRecurringTransaction(id: string | undefined) {
  return useQuery({
    queryKey: [...recurringQueryKey, id],
    queryFn: () => recurringApi.getById(id!),
    enabled: id !== undefined,
  })
}
