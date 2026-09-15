import { useMutation, useQueryClient } from '@tanstack/react-query'
import { recurringApi } from '@/features/recurring/api/recurringApi'
import { recurringQueryKey } from '@/features/recurring/hooks/useRecurring'
import type { UpdateRecurringTransactionPayload } from '@/features/recurring/types'

function useInvalidateAfterMutation() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: recurringQueryKey })
}

export function useCreateRecurringExpense() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.createExpense, onSuccess: invalidate })
}

export function useCreateRecurringIncome() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.createIncome, onSuccess: invalidate })
}

export function useCreateRecurringTransfer() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.createTransfer, onSuccess: invalidate })
}

export function useUpdateRecurringTransaction(id: string) {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({
    mutationFn: (payload: UpdateRecurringTransactionPayload) => recurringApi.update(id, payload),
    onSuccess: invalidate,
  })
}

export function usePauseRecurringTransaction() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.pause, onSuccess: invalidate })
}

export function useResumeRecurringTransaction() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.resume, onSuccess: invalidate })
}

export function useDeleteRecurringTransaction() {
  const invalidate = useInvalidateAfterMutation()
  return useMutation({ mutationFn: recurringApi.remove, onSuccess: invalidate })
}
