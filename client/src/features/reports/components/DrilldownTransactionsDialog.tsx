import { Loader2, Receipt } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { transactionsApi } from '@/features/transactions/api/transactionsApi'
import type { DateRange } from '@/lib/dateRangePresets'
import { formatCurrency } from '@/lib/utils/currency'

interface DrilldownTarget {
  title: string
  categoryId?: string
  subCategoryId?: string
}

interface DrilldownTransactionsDialogProps {
  target: DrilldownTarget | null
  range: DateRange
  onOpenChange: (open: boolean) => void
}

export function DrilldownTransactionsDialog({ target, range, onOpenChange }: DrilldownTransactionsDialogProps) {
  const { data, isLoading } = useQuery({
    queryKey: ['reports', 'drilldown', target?.categoryId, target?.subCategoryId, range],
    queryFn: () =>
      transactionsApi.list({
        categoryId: target?.categoryId,
        subCategoryId: target?.subCategoryId,
        fromDate: range.fromDate,
        toDate: range.toDate,
        pageSize: 50,
        sortBy: 'transactionDate',
        sortDirection: 'desc',
      }),
    enabled: target !== null,
  })

  return (
    <Dialog open={target !== null} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{target?.title}</DialogTitle>
        </DialogHeader>

        {isLoading && (
          <div className="flex justify-center py-8">
            <Loader2 className="size-6 animate-spin text-muted-foreground" />
          </div>
        )}

        {data && data.items.length === 0 && (
          <PlaceholderPage icon={Receipt} title="No transactions" description="Nothing recorded in this period." />
        )}

        {data && data.items.length > 0 && (
          <ul className="max-h-96 divide-y divide-border overflow-y-auto">
            {data.items.map((t) => (
              <li key={t.id} className="flex items-center justify-between gap-3 py-2.5">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium text-foreground">{t.description || t.accountName}</p>
                  <p className="text-xs text-muted-foreground">
                    {t.transactionDate} · {t.accountName}
                  </p>
                </div>
                <span className="shrink-0 text-sm font-medium tabular-nums">{formatCurrency(t.amount)}</span>
              </li>
            ))}
          </ul>
        )}
      </DialogContent>
    </Dialog>
  )
}
