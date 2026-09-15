import { useQuery } from '@tanstack/react-query'
import { transactionsApi } from '@/features/transactions/api/transactionsApi'

export function useTransaction(id: string | undefined) {
  return useQuery({
    queryKey: ['transactions', 'detail', id],
    queryFn: () => transactionsApi.getById(id!),
    enabled: Boolean(id),
  })
}
