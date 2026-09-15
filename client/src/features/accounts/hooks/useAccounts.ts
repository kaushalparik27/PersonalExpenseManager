import { useQuery } from '@tanstack/react-query'
import { accountsApi } from '@/features/accounts/api/accountsApi'

export const accountsQueryKey = ['accounts'] as const

export function useAccounts() {
  return useQuery({
    queryKey: accountsQueryKey,
    queryFn: accountsApi.list,
  })
}
