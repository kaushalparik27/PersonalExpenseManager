import { ArrowLeft } from 'lucide-react'
import { useSearchParams } from 'react-router-dom'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ExpenseForm, type ExpenseFormSubmitValues } from '@/features/transactions/components/ExpenseForm'
import { IncomeForm, type IncomeFormSubmitValues } from '@/features/transactions/components/IncomeForm'
import { TransferForm, type TransferFormSubmitValues } from '@/features/transactions/components/TransferForm'
import {
  useCreateExpense,
  useCreateIncome,
  useCreateTransfer,
} from '@/features/transactions/hooks/useTransactionMutations'
import { getRecentSelections, rememberSelections } from '@/features/transactions/lib/recentSelections'
import { ApiRequestError } from '@/lib/api/client'

export function NewTransactionPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const navigate = useNavigate()
  const createExpense = useCreateExpense()
  const createIncome = useCreateIncome()
  const createTransfer = useCreateTransfer()
  const recent = getRecentSelections()

  const activeTab = searchParams.get('type') ?? 'expense'
  // Deep-linked from an account (e.g. "Add Transaction" on the Accounts page)
  // takes priority over the last-used account remembered for quick entry.
  const preselectedAccountId = searchParams.get('accountId') ?? undefined
  const defaultAccountId = preselectedAccountId ?? recent.accountId

  // Deep-linked from a category/subcategory (e.g. "Add Transaction" on the
  // Categories page) similarly takes priority over the last-used category.
  const preselectedCategoryId = searchParams.get('categoryId') ?? undefined
  const preselectedSubCategoryId = searchParams.get('subCategoryId') ?? undefined
  const defaultExpenseCategoryId =
    activeTab === 'expense' && preselectedCategoryId ? preselectedCategoryId : recent.expenseCategoryId
  const defaultIncomeCategoryId =
    activeTab === 'income' && preselectedCategoryId ? preselectedCategoryId : recent.incomeCategoryId

  async function handleExpenseSubmit(values: ExpenseFormSubmitValues, addAnother: boolean) {
    try {
      await createExpense.mutateAsync(values)
      rememberSelections({ accountId: values.accountId, expenseCategoryId: values.expenseCategoryId })
      toast.success(addAnother ? 'Expense added. Ready for the next one.' : 'Expense added.')
      if (!addAnother) {
        navigate('/transactions')
      }
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the expense.')
      throw error
    }
  }

  async function handleIncomeSubmit(values: IncomeFormSubmitValues, addAnother: boolean) {
    try {
      await createIncome.mutateAsync(values)
      rememberSelections({ accountId: values.accountId, incomeCategoryId: values.incomeCategoryId })
      toast.success(addAnother ? 'Income added. Ready for the next one.' : 'Income added.')
      if (!addAnother) {
        navigate('/transactions')
      }
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save the income.')
      throw error
    }
  }

  async function handleTransferSubmit(values: TransferFormSubmitValues) {
    try {
      await createTransfer.mutateAsync(values)
      toast.success('Transfer recorded.')
      navigate('/transactions')
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to record the transfer.')
      throw error
    }
  }

  return (
    <div className="mx-auto max-w-md space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to="/transactions">
          <ArrowLeft className="size-4" />
          Transactions
        </Link>
      </Button>

      <div>
        <h1 className="text-lg font-semibold text-foreground">Add Transaction</h1>
        <p className="text-sm text-muted-foreground">Record an expense, income, or transfer.</p>
      </div>

      <Tabs value={activeTab} onValueChange={(value) => setSearchParams({ type: value })}>
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
          <ExpenseForm
            submitLabel="Save Expense"
            showSaveAndAddAnother
            defaultValues={{
              accountId: defaultAccountId,
              expenseCategoryId: defaultExpenseCategoryId,
              expenseSubCategoryId:
                activeTab === 'expense' ? (preselectedSubCategoryId ?? undefined) : undefined,
            }}
            onSubmit={handleExpenseSubmit}
          />
        </TabsContent>

        <TabsContent value="income" className="mt-4">
          <IncomeForm
            submitLabel="Save Income"
            showSaveAndAddAnother
            defaultValues={{
              accountId: defaultAccountId,
              incomeCategoryId: defaultIncomeCategoryId,
              incomeSubCategoryId:
                activeTab === 'income' ? (preselectedSubCategoryId ?? undefined) : undefined,
            }}
            onSubmit={handleIncomeSubmit}
          />
        </TabsContent>

        <TabsContent value="transfer" className="mt-4">
          <TransferForm
            submitLabel="Save Transfer"
            defaultValues={{ accountId: defaultAccountId }}
            onSubmit={handleTransferSubmit}
          />
        </TabsContent>
      </Tabs>
    </div>
  )
}
