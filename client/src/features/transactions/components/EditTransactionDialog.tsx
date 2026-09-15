import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { ExpenseForm, type ExpenseFormSubmitValues } from '@/features/transactions/components/ExpenseForm'
import { IncomeForm, type IncomeFormSubmitValues } from '@/features/transactions/components/IncomeForm'
import { TransferForm, type TransferFormSubmitValues } from '@/features/transactions/components/TransferForm'
import { useTransaction } from '@/features/transactions/hooks/useTransaction'
import { useUpdateTransaction } from '@/features/transactions/hooks/useTransactionMutations'
import { ApiRequestError } from '@/lib/api/client'

interface EditTransactionDialogProps {
  transactionId: string | null
  onOpenChange: (open: boolean) => void
}

export function EditTransactionDialog({ transactionId, onOpenChange }: EditTransactionDialogProps) {
  const { data: transaction, isLoading } = useTransaction(transactionId ?? undefined)
  const updateTransaction = useUpdateTransaction(transactionId ?? '')

  async function handleExpenseSubmit(values: ExpenseFormSubmitValues) {
    try {
      await updateTransaction.mutateAsync({
        transactionDate: values.transactionDate,
        accountId: values.accountId,
        amount: values.amount,
        expenseCategoryId: values.expenseCategoryId,
        expenseSubCategoryId: values.expenseSubCategoryId ?? null,
        description: values.description ?? null,
        tags: values.tags,
      })
      toast.success('Transaction updated.')
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to update the transaction.')
      throw error
    }
  }

  async function handleIncomeSubmit(values: IncomeFormSubmitValues) {
    try {
      await updateTransaction.mutateAsync({
        transactionDate: values.transactionDate,
        accountId: values.accountId,
        amount: values.amount,
        incomeCategoryId: values.incomeCategoryId,
        incomeSubCategoryId: values.incomeSubCategoryId ?? null,
        description: values.description ?? null,
        tags: values.tags,
      })
      toast.success('Transaction updated.')
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to update the transaction.')
      throw error
    }
  }

  async function handleTransferSubmit(values: TransferFormSubmitValues) {
    try {
      await updateTransaction.mutateAsync({
        transactionDate: values.transactionDate,
        accountId: values.accountId,
        transferToAccountId: values.transferToAccountId,
        amount: values.amount,
        description: values.description ?? null,
      })
      toast.success('Transaction updated.')
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to update the transaction.')
      throw error
    }
  }

  return (
    <Dialog open={transactionId !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Edit {transaction?.transactionType ?? 'Transaction'}</DialogTitle>
        </DialogHeader>

        {isLoading && (
          <div className="flex justify-center py-8">
            <Loader2 className="size-6 animate-spin text-muted-foreground" />
          </div>
        )}

        {transaction?.transactionType === 'Expense' && (
          <ExpenseForm
            submitLabel="Save Changes"
            defaultValues={{
              transactionDate: transaction.transactionDate,
              accountId: transaction.accountId,
              amount: transaction.amount,
              expenseCategoryId: transaction.expenseCategoryId ?? '',
              expenseSubCategoryId: transaction.expenseSubCategoryId ?? undefined,
              description: transaction.description ?? '',
              tagsText: transaction.tags.join(', '),
            }}
            onSubmit={handleExpenseSubmit}
          />
        )}

        {transaction?.transactionType === 'Income' && (
          <IncomeForm
            submitLabel="Save Changes"
            defaultValues={{
              transactionDate: transaction.transactionDate,
              accountId: transaction.accountId,
              amount: transaction.amount,
              incomeCategoryId: transaction.incomeCategoryId ?? '',
              incomeSubCategoryId: transaction.incomeSubCategoryId ?? undefined,
              description: transaction.description ?? '',
              tagsText: transaction.tags.join(', '),
            }}
            onSubmit={handleIncomeSubmit}
          />
        )}

        {transaction?.transactionType === 'Transfer' && (
          <TransferForm
            submitLabel="Save Changes"
            defaultValues={{
              transactionDate: transaction.transactionDate,
              accountId: transaction.accountId,
              transferToAccountId: transaction.transferToAccountId ?? '',
              amount: transaction.amount,
              description: transaction.description ?? '',
            }}
            onSubmit={handleTransferSubmit}
          />
        )}
      </DialogContent>
    </Dialog>
  )
}
