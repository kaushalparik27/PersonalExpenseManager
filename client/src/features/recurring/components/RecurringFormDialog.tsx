import { useState } from 'react'
import { toast } from 'sonner'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { RecurringExpenseForm, type RecurringExpenseFormSubmitValues } from '@/features/recurring/components/RecurringExpenseForm'
import { RecurringIncomeForm, type RecurringIncomeFormSubmitValues } from '@/features/recurring/components/RecurringIncomeForm'
import { RecurringTransferForm, type RecurringTransferFormSubmitValues } from '@/features/recurring/components/RecurringTransferForm'
import {
  useCreateRecurringExpense,
  useCreateRecurringIncome,
  useCreateRecurringTransfer,
  useUpdateRecurringTransaction,
} from '@/features/recurring/hooks/useRecurringMutations'
import type { RecurringTransaction } from '@/features/recurring/types'
import { ApiRequestError } from '@/lib/api/client'

interface RecurringFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  recurring?: RecurringTransaction
}

export function RecurringFormDialog({ open, onOpenChange, recurring }: RecurringFormDialogProps) {
  const [tab, setTab] = useState<'expense' | 'income' | 'transfer'>('expense')

  const createExpense = useCreateRecurringExpense()
  const createIncome = useCreateRecurringIncome()
  const createTransfer = useCreateRecurringTransfer()
  const updateRecurring = useUpdateRecurringTransaction(recurring?.id ?? '')

  async function handleExpenseSubmit(values: RecurringExpenseFormSubmitValues) {
    try {
      if (recurring) {
        await updateRecurring.mutateAsync({
          accountId: values.accountId,
          amount: values.amount,
          expenseCategoryId: values.expenseCategoryId,
          expenseSubCategoryId: values.expenseSubCategoryId ?? null,
          description: values.description ?? null,
          frequency: values.frequency,
          startDate: values.startDate,
          endDate: values.endDate ?? null,
        })
        toast.success('Recurring expense updated.')
      } else {
        await createExpense.mutateAsync(values)
        toast.success('Recurring expense created.')
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the recurring expense.')
      throw error
    }
  }

  async function handleIncomeSubmit(values: RecurringIncomeFormSubmitValues) {
    try {
      if (recurring) {
        await updateRecurring.mutateAsync({
          accountId: values.accountId,
          amount: values.amount,
          incomeCategoryId: values.incomeCategoryId,
          incomeSubCategoryId: values.incomeSubCategoryId ?? null,
          description: values.description ?? null,
          frequency: values.frequency,
          startDate: values.startDate,
          endDate: values.endDate ?? null,
        })
        toast.success('Recurring income updated.')
      } else {
        await createIncome.mutateAsync(values)
        toast.success('Recurring income created.')
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the recurring income.')
      throw error
    }
  }

  async function handleTransferSubmit(values: RecurringTransferFormSubmitValues) {
    try {
      if (recurring) {
        await updateRecurring.mutateAsync({
          accountId: values.accountId,
          amount: values.amount,
          transferToAccountId: values.transferToAccountId,
          description: values.description ?? null,
          frequency: values.frequency,
          startDate: values.startDate,
          endDate: values.endDate ?? null,
        })
        toast.success('Recurring transfer updated.')
      } else {
        await createTransfer.mutateAsync(values)
        toast.success('Recurring transfer created.')
      }
      onOpenChange(false)
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the recurring transfer.')
      throw error
    }
  }

  const isEditMode = Boolean(recurring)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditMode ? `Edit Recurring ${recurring!.transactionType}` : 'New Recurring Transaction'}</DialogTitle>
        </DialogHeader>

        {isEditMode ? (
          <>
            {recurring!.transactionType === 'Expense' && (
              <RecurringExpenseForm
                submitLabel="Save Changes"
                defaultValues={{
                  accountId: recurring!.accountId,
                  amount: recurring!.amount,
                  expenseCategoryId: recurring!.expenseCategoryId ?? '',
                  expenseSubCategoryId: recurring!.expenseSubCategoryId ?? undefined,
                  description: recurring!.description ?? '',
                  frequency: recurring!.frequency,
                  startDate: recurring!.startDate,
                  endDate: recurring!.endDate ?? '',
                }}
                onSubmit={handleExpenseSubmit}
              />
            )}

            {recurring!.transactionType === 'Income' && (
              <RecurringIncomeForm
                submitLabel="Save Changes"
                defaultValues={{
                  accountId: recurring!.accountId,
                  amount: recurring!.amount,
                  incomeCategoryId: recurring!.incomeCategoryId ?? '',
                  incomeSubCategoryId: recurring!.incomeSubCategoryId ?? undefined,
                  description: recurring!.description ?? '',
                  frequency: recurring!.frequency,
                  startDate: recurring!.startDate,
                  endDate: recurring!.endDate ?? '',
                }}
                onSubmit={handleIncomeSubmit}
              />
            )}

            {recurring!.transactionType === 'Transfer' && (
              <RecurringTransferForm
                submitLabel="Save Changes"
                defaultValues={{
                  accountId: recurring!.accountId,
                  transferToAccountId: recurring!.transferToAccountId ?? '',
                  amount: recurring!.amount,
                  description: recurring!.description ?? '',
                  frequency: recurring!.frequency,
                  startDate: recurring!.startDate,
                  endDate: recurring!.endDate ?? '',
                }}
                onSubmit={handleTransferSubmit}
              />
            )}
          </>
        ) : (
          <Tabs value={tab} onValueChange={(value) => setTab(value as typeof tab)}>
            <TabsList className="w-full">
              <TabsTrigger value="expense" className="flex-1">
                Expense
              </TabsTrigger>
              <TabsTrigger value="income" className="flex-1">
                Income
              </TabsTrigger>
              <TabsTrigger value="transfer" className="flex-1">
                Transfer
              </TabsTrigger>
            </TabsList>

            <TabsContent value="expense" className="mt-4">
              <RecurringExpenseForm submitLabel="Create Recurring Expense" onSubmit={handleExpenseSubmit} />
            </TabsContent>

            <TabsContent value="income" className="mt-4">
              <RecurringIncomeForm submitLabel="Create Recurring Income" onSubmit={handleIncomeSubmit} />
            </TabsContent>

            <TabsContent value="transfer" className="mt-4">
              <RecurringTransferForm submitLabel="Create Recurring Transfer" onSubmit={handleTransferSubmit} />
            </TabsContent>
          </Tabs>
        )}
      </DialogContent>
    </Dialog>
  )
}
