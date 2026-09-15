import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { transactionsApi } from '@/features/transactions/api/transactionsApi'
import type { TransactionListFilters } from '@/features/transactions/types'

export function transactionsQueryKey(filters: TransactionListFilters) {
  return ['transactions', filters] as const
}

export function useTransactions(filters: TransactionListFilters) {
  return useQuery({
    queryKey: transactionsQueryKey(filters),
    queryFn: () => transactionsApi.list(filters),
    placeholderData: keepPreviousData,
  })
}
