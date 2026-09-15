import { useMutation, useQueryClient } from '@tanstack/react-query'
import { accountsApi } from '@/features/accounts/api/accountsApi'
import { accountsQueryKey } from '@/features/accounts/hooks/useAccounts'
import type { AccountFormValues, UpdateAccountPayload } from '@/features/accounts/types'

function useInvalidateAccounts() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: accountsQueryKey })
    queryClient.invalidateQueries({ queryKey: ['dashboard'] })
  }
}

export function useCreateAccount() {
  const invalidate = useInvalidateAccounts()
  return useMutation({
    mutationFn: (payload: AccountFormValues) => accountsApi.create(payload),
    onSuccess: invalidate,
  })
}

export function useUpdateAccount(id: string) {
  const invalidate = useInvalidateAccounts()
  return useMutation({
    mutationFn: (payload: UpdateAccountPayload) => accountsApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useArchiveAccount() {
  const invalidate = useInvalidateAccounts()
  return useMutation({
    mutationFn: (id: string) => accountsApi.archive(id),
    onSuccess: invalidate,
  })
}

export function useDeleteAccount() {
  const invalidate = useInvalidateAccounts()
  return useMutation({
    mutationFn: (id: string) => accountsApi.remove(id),
    onSuccess: invalidate,
  })
}
