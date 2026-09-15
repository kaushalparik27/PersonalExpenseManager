import { ArrowLeftRight, ChevronLeft, ChevronRight, Pencil, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { EditTransactionDialog } from '@/features/transactions/components/EditTransactionDialog'
import { TransactionFilters } from '@/features/transactions/components/TransactionFilters'
import { useDeleteTransaction } from '@/features/transactions/hooks/useTransactionMutations'
import { useTransactions } from '@/features/transactions/hooks/useTransactions'
import type { TransactionListFilters, TransactionType } from '@/features/transactions/types'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import { ApiRequestError } from '@/lib/api/client'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

const typeBadgeVariant: Record<TransactionType, 'default' | 'secondary' | 'outline'> = {
  Income: 'default',
  Expense: 'secondary',
  Transfer: 'outline',
  Adjustment: 'outline',
}

export function TransactionsPage() {
  const [filters, setFilters] = useState<TransactionListFilters>({ page: 1, pageSize: 25 })
  const debouncedFilters = useDebouncedValue(filters, 300)
  const { data, isLoading, isError } = useTransactions(debouncedFilters)
  const deleteTransaction = useDeleteTransaction()

  const [editingId, setEditingId] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)

  async function handleDelete() {
    if (!deletingId) return
    try {
      await deleteTransaction.mutateAsync(deletingId)
      toast.success('Transaction deleted.')
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to delete the transaction.')
    } finally {
      setDeletingId(null)
    }
  }

  const page = data?.page ?? 1
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-foreground">Transactions</h1>
          <p className="text-sm text-muted-foreground">Search, filter, and manage your income, expenses, and transfers.</p>
        </div>
        <Button asChild>
          <Link to="/transactions/new">
            <Plus className="size-4" />
            Add Transaction
          </Link>
        </Button>
      </div>

      <TransactionFilters filters={filters} onChange={setFilters} />

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-12 rounded-lg" />
          ))}
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Unable to load transactions. Please try again.</p>}

      {data && data.items.length === 0 && (
        <PlaceholderPage
          icon={ArrowLeftRight}
          title="No transactions yet"
          description="Record your first expense or income to start understanding your financial flow."
        />
      )}

      {data && data.items.length > 0 && (
        <>
          <div className="overflow-x-auto rounded-lg border border-border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Date</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Account</TableHead>
                  <TableHead className="text-right">Amount</TableHead>
                  <TableHead className="w-20" />
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((t) => {
                  const categoryLabel =
                    t.transactionType === 'Transfer'
                      ? `${t.accountName} → ${t.transferToAccountName}`
                      : (t.expenseCategoryName ?? t.incomeCategoryName ?? '—')
                  const isOutflow = t.transactionType === 'Expense'
                  const isInflow = t.transactionType === 'Income'

                  return (
                    <TableRow key={t.id}>
                      <TableCell className="whitespace-nowrap text-sm text-muted-foreground">{t.transactionDate}</TableCell>
                      <TableCell>
                        <Badge variant={typeBadgeVariant[t.transactionType]}>{t.transactionType}</Badge>
                      </TableCell>
                      <TableCell className="max-w-56 truncate text-sm">{t.description || '—'}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {categoryLabel}
                        {t.expenseSubCategoryName || t.incomeSubCategoryName ? (
                          <span className="text-xs"> · {t.expenseSubCategoryName ?? t.incomeSubCategoryName}</span>
                        ) : null}
                      </TableCell>
                      <TableCell className="text-sm text-muted-foreground">{t.accountName}</TableCell>
                      <TableCell
                        className={cn(
                          'text-right text-sm font-medium tabular-nums',
                          isOutflow && 'text-destructive',
                          isInflow && 'text-success',
                        )}
                      >
                        {isOutflow ? '-' : isInflow ? '+' : ''}
                        {formatCurrency(t.amount)}
                      </TableCell>
                      <TableCell>
                        <div className="flex justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7"
                            aria-label="Edit transaction"
                            onClick={() => setEditingId(t.id)}
                          >
                            <Pencil className="size-3.5" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7"
                            aria-label="Delete transaction"
                            onClick={() => setDeletingId(t.id)}
                          >
                            <Trash2 className="size-3.5" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </div>

          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>
              {data.totalCount} transaction{data.totalCount === 1 ? '' : 's'} · Page {page} of {totalPages}
            </span>
            <div className="flex gap-1">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setFilters((f) => ({ ...f, page: page - 1 }))}
              >
                <ChevronLeft className="size-4" />
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setFilters((f) => ({ ...f, page: page + 1 }))}
              >
                Next
                <ChevronRight className="size-4" />
              </Button>
            </div>
          </div>
        </>
      )}

      <EditTransactionDialog transactionId={editingId} onOpenChange={(open) => !open && setEditingId(null)} />

      <AlertDialog open={deletingId !== null} onOpenChange={(open) => !open && setDeletingId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete transaction?</AlertDialogTitle>
            <AlertDialogDescription>
              This transaction will be removed from your balances and reports. It can be recovered later if needed.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-white hover:bg-destructive/90">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
