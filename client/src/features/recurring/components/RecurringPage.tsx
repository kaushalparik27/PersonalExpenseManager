import { MoreVertical, Pause, Pencil, Play, Plus, Repeat, Trash2 } from 'lucide-react'
import { useState } from 'react'
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { RecurringFormDialog } from '@/features/recurring/components/RecurringFormDialog'
import {
  useDeleteRecurringTransaction,
  usePauseRecurringTransaction,
  useResumeRecurringTransaction,
} from '@/features/recurring/hooks/useRecurringMutations'
import { useRecurringTransactions } from '@/features/recurring/hooks/useRecurring'
import type { RecurringTransaction, RecurrenceFrequency } from '@/features/recurring/types'
import type { TransactionType } from '@/features/transactions/types'
import { ApiRequestError } from '@/lib/api/client'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

const typeBadgeVariant: Record<TransactionType, 'default' | 'secondary' | 'outline'> = {
  Income: 'default',
  Expense: 'secondary',
  Transfer: 'outline',
  Adjustment: 'outline',
}

const frequencyLabel: Record<RecurrenceFrequency, string> = {
  Daily: 'Daily',
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  Quarterly: 'Quarterly',
  Yearly: 'Yearly',
}

function formatDate(date: string) {
  return new Date(`${date}T00:00:00`).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })
}

export function RecurringPage() {
  const { data: recurring, isLoading, isError } = useRecurringTransactions()
  const pauseRecurring = usePauseRecurringTransaction()
  const resumeRecurring = useResumeRecurringTransaction()
  const deleteRecurring = useDeleteRecurringTransaction()

  const [formOpen, setFormOpen] = useState(false)
  const [editing, setEditing] = useState<RecurringTransaction | undefined>(undefined)
  const [deletingId, setDeletingId] = useState<string | null>(null)

  function openCreate() {
    setEditing(undefined)
    setFormOpen(true)
  }

  function openEdit(item: RecurringTransaction) {
    setEditing(item)
    setFormOpen(true)
  }

  async function handleToggleActive(item: RecurringTransaction) {
    try {
      if (item.isActive) {
        await pauseRecurring.mutateAsync(item.id)
        toast.success(`${item.description ?? 'Recurring transaction'} paused.`)
      } else {
        await resumeRecurring.mutateAsync(item.id)
        toast.success(`${item.description ?? 'Recurring transaction'} resumed.`)
      }
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to update this recurring transaction.')
    }
  }

  async function handleDelete() {
    if (!deletingId) return
    try {
      await deleteRecurring.mutateAsync(deletingId)
      toast.success('Recurring transaction deleted.')
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to delete this recurring transaction.')
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-lg font-semibold text-foreground">Recurring Transactions</h1>
          <p className="text-sm text-muted-foreground">Salary, rent, subscriptions, and other transactions that repeat automatically.</p>
        </div>
        <Button onClick={openCreate}>
          <Plus className="size-4" />
          New Recurring
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-14 rounded-lg" />
          ))}
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Unable to load recurring transactions. Please try again.</p>}

      {recurring && recurring.length === 0 && (
        <PlaceholderPage
          icon={Repeat}
          title="No recurring transactions yet"
          description="Set up salary, rent, subscriptions, or SIPs once and they'll be recorded automatically on schedule."
        />
      )}

      {recurring && recurring.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Description</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Category / Account</TableHead>
                <TableHead>Frequency</TableHead>
                <TableHead>Next Occurrence</TableHead>
                <TableHead className="text-right">Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="w-10" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {recurring.map((item) => {
                const categoryLabel =
                  item.transactionType === 'Transfer'
                    ? `${item.accountName} → ${item.transferToAccountName}`
                    : `${item.expenseCategoryName ?? item.incomeCategoryName ?? '—'} · ${item.accountName}`
                const isOutflow = item.transactionType === 'Expense'
                const isInflow = item.transactionType === 'Income'

                return (
                  <TableRow key={item.id} className={cn(!item.isActive && 'opacity-60')}>
                    <TableCell className="max-w-48 truncate text-sm font-medium">{item.description || '—'}</TableCell>
                    <TableCell>
                      <Badge variant={typeBadgeVariant[item.transactionType]}>{item.transactionType}</Badge>
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">{categoryLabel}</TableCell>
                    <TableCell className="text-sm text-muted-foreground">{frequencyLabel[item.frequency]}</TableCell>
                    <TableCell className="whitespace-nowrap text-sm text-muted-foreground">{formatDate(item.nextOccurrenceDate)}</TableCell>
                    <TableCell
                      className={cn(
                        'text-right text-sm font-medium tabular-nums',
                        isOutflow && 'text-destructive',
                        isInflow && 'text-success',
                      )}
                    >
                      {isOutflow ? '-' : isInflow ? '+' : ''}
                      {formatCurrency(item.amount)}
                    </TableCell>
                    <TableCell>
                      <Badge variant={item.isActive ? 'default' : 'secondary'}>{item.isActive ? 'Active' : 'Paused'}</Badge>
                    </TableCell>
                    <TableCell>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon" className="size-7" aria-label="Recurring transaction actions">
                            <MoreVertical className="size-3.5" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onSelect={() => openEdit(item)}>
                            <Pencil className="size-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem onSelect={() => handleToggleActive(item)}>
                            {item.isActive ? <Pause className="size-4" /> : <Play className="size-4" />}
                            {item.isActive ? 'Pause' : 'Resume'}
                          </DropdownMenuItem>
                          <DropdownMenuItem variant="destructive" onSelect={() => setDeletingId(item.id)}>
                            <Trash2 className="size-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </div>
      )}

      <RecurringFormDialog open={formOpen} onOpenChange={setFormOpen} recurring={editing} />

      <AlertDialog open={deletingId !== null} onOpenChange={(open) => !open && setDeletingId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete recurring transaction?</AlertDialogTitle>
            <AlertDialogDescription>
              This stops future transactions from being generated. Transactions already recorded from it are kept.
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
