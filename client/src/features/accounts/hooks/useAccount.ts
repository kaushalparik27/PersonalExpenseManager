import { useQuery } from '@tanstack/react-query'
import { accountsApi } from '@/features/accounts/api/accountsApi'

export function useAccount(id: string | undefined) {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: () => accountsApi.getById(id!),
    enabled: Boolean(id),
  })
}
