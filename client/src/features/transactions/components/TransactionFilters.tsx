import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/common/Combobox'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useAccounts } from '@/features/accounts/hooks/useAccounts'
import { useCategories } from '@/features/categories/hooks/useCategories'
import type { TransactionListFilters } from '@/features/transactions/types'

interface TransactionFiltersProps {
  filters: TransactionListFilters
  onChange: (filters: TransactionListFilters) => void
}

export function TransactionFilters({ filters, onChange }: TransactionFiltersProps) {
  const { data: accounts } = useAccounts()
  const { data: expenseCategories } = useCategories('expense')
  const { data: incomeCategories } = useCategories('income')

  const accountOptions = (accounts ?? []).map((a) => ({ value: a.id, label: a.name }))
  const categoryOptions = [
    ...(expenseCategories ?? []).map((c) => ({ value: c.id, label: c.name, hint: 'Expense' })),
    ...(incomeCategories ?? []).map((c) => ({ value: c.id, label: c.name, hint: 'Income' })),
  ]

  function update(patch: Partial<TransactionListFilters>) {
    onChange({ ...filters, ...patch, page: 1 })
  }

  const hasActiveFilters =
    filters.search || filters.accountId || filters.categoryId || filters.transactionType || filters.fromDate || filters.toDate

  return (
    <div className="flex flex-wrap items-end gap-3">
      <div className="min-w-48 flex-1 space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">Search</label>
        <Input
          placeholder="Description, payee, tags…"
          value={filters.search ?? ''}
          onChange={(e) => update({ search: e.target.value })}
        />
      </div>

      <div className="w-36 space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">Type</label>
        <Select
          value={filters.transactionType ?? 'all'}
          onValueChange={(value) => update({ transactionType: value === 'all' ? undefined : (value as TransactionListFilters['transactionType']) })}
        >
          <SelectTrigger className="w-full">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All types</SelectItem>
            <SelectItem value="Expense">Expense</SelectItem>
            <SelectItem value="Income">Income</SelectItem>
            <SelectItem value="Transfer">Transfer</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="w-44 space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">Account</label>
        <Combobox
          options={accountOptions}
          value={filters.accountId}
          onChange={(value) => update({ accountId: value })}
          placeholder="All accounts"
          searchPlaceholder="Search accounts…"
        />
      </div>

      <div className="w-44 space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">Category</label>
        <Combobox
          options={categoryOptions}
          value={filters.categoryId}
          onChange={(value) => update({ categoryId: value })}
          placeholder="All categories"
          searchPlaceholder="Search categories…"
        />
      </div>

      <div className="space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">From</label>
        <Input type="date" value={filters.fromDate ?? ''} onChange={(e) => update({ fromDate: e.target.value || undefined })} />
      </div>

      <div className="space-y-1.5">
        <label className="text-xs font-medium text-muted-foreground">To</label>
        <Input type="date" value={filters.toDate ?? ''} onChange={(e) => update({ toDate: e.target.value || undefined })} />
      </div>

      {hasActiveFilters && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() =>
            onChange({ page: 1, pageSize: filters.pageSize, sortBy: filters.sortBy, sortDirection: filters.sortDirection })
          }
        >
          <X className="size-4" />
          Clear filters
        </Button>
      )}
    </div>
  )
}
