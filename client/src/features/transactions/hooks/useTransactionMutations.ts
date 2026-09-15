import { useMutation, useQueryClient } from '@tanstack/react-query'
import { transactionsApi } from '@/features/transactions/api/transactionsApi'
import type { UpdateTransactionPayload } from '@/features/transactions/types'
import { accountsQueryKey } from '@/features/accounts/hooks/useAccounts'

// Any transaction mutation changes account balances too, so both caches need
// invalidating — not just the transaction list.
function useInvalidateAfterMutation() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['transactions'] })
    queryClient.invalidateQueries({ queryKey: accountsQueryKey })
  }
}

export function useCreateExpense() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: transactionsApi.createExpense, onSuccess: invalidate })
}

export function useCreateIncome() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: transactionsApi.createIncome, onSuccess: invalidate })
}

export function useCreateTransfer() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: transactionsApi.createTransfer, onSuccess: invalidate })
}

export function useUpdateTransaction(id: string) {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({
    mutationFn: (payload: UpdateTransactionPayload) => transactionsApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function useDeleteTransaction() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: transactionsApi.remove, onSuccess: invalidate })
}
