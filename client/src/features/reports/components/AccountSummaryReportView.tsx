import { Landmark } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import type { AccountSummaryReport } from '@/features/reports/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

interface AccountSummaryReportViewProps {
  data: AccountSummaryReport | undefined
  isLoading: boolean
}

export function AccountSummaryReportView({ data, isLoading }: AccountSummaryReportViewProps) {
  if (isLoading) {
    return <Skeleton className="h-80 rounded-xl" />
  }

  if (!data || data.accounts.length === 0) {
    return <PlaceholderPage icon={Landmark} title="No accounts" description="Create an account to see its activity here." />
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Account</TableHead>
            <TableHead className="text-right">Income</TableHead>
            <TableHead className="text-right">Expense</TableHead>
            <TableHead className="text-right">Transfers In</TableHead>
            <TableHead className="text-right">Transfers Out</TableHead>
            <TableHead className="text-right">Net Change</TableHead>
            <TableHead className="text-right">Current Balance</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.accounts.map((a) => (
            <TableRow key={a.accountId}>
              <TableCell>
                <div className="text-sm font-medium text-foreground">{a.accountName}</div>
                <div className="text-xs text-muted-foreground">{a.accountType}</div>
              </TableCell>
              <TableCell className="text-right text-sm tabular-nums text-success">{formatCurrency(a.periodIncome)}</TableCell>
              <TableCell className="text-right text-sm tabular-nums text-destructive">{formatCurrency(a.periodExpense)}</TableCell>
              <TableCell className="text-right text-sm tabular-nums text-muted-foreground">{formatCurrency(a.periodTransferIn)}</TableCell>
              <TableCell className="text-right text-sm tabular-nums text-muted-foreground">{formatCurrency(a.periodTransferOut)}</TableCell>
              <TableCell
                className={cn(
                  'text-right text-sm font-medium tabular-nums',
                  a.periodNetChange >= 0 ? 'text-success' : 'text-destructive',
                )}
              >
                {formatCurrency(a.periodNetChange)}
              </TableCell>
              <TableCell className="text-right text-sm font-semibold tabular-nums">{formatCurrency(a.currentBalance)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
