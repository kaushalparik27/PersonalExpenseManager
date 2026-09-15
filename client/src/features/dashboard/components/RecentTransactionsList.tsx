import { ArrowLeftRight } from 'lucide-react'
import { Link } from 'react-router-dom'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import type { Transaction, TransactionType } from '@/features/transactions/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

const typeBadgeVariant: Record<TransactionType, 'default' | 'secondary' | 'outline'> = {
  Income: 'default',
  Expense: 'secondary',
  Transfer: 'outline',
  Adjustment: 'outline',
}

interface RecentTransactionsListProps {
  transactions: Transaction[]
}

export function RecentTransactionsList({ transactions }: RecentTransactionsListProps) {
  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <CardTitle className="text-base">Recent Transactions</CardTitle>
        <Link to="/transactions" className="text-xs font-medium text-primary hover:underline">
          View all
        </Link>
      </CardHeader>
      <CardContent>
        {transactions.length === 0 ? (
          <PlaceholderPage
            icon={ArrowLeftRight}
            title="No transactions yet"
            description="Recent activity will show up here."
          />
        ) : (
          <ul className="divide-y divide-border">
            {transactions.map((t) => {
              const isOutflow = t.transactionType === 'Expense'
              const isInflow = t.transactionType === 'Income'
              const label =
                t.transactionType === 'Transfer'
                  ? `${t.accountName} → ${t.transferToAccountName}`
                  : (t.expenseCategoryName ?? t.incomeCategoryName ?? t.accountName)

              return (
                <li key={t.id} className="flex items-center justify-between gap-3 py-2.5">
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-foreground">{t.description || label}</p>
                    <div className="mt-0.5 flex items-center gap-1.5">
                      <Badge variant={typeBadgeVariant[t.transactionType]} className="text-[10px]">
                        {t.transactionType}
                      </Badge>
                      <span className="truncate text-xs text-muted-foreground">{t.transactionDate}</span>
                    </div>
                  </div>
                  <span
                    className={cn(
                      'shrink-0 text-sm font-medium tabular-nums',
                      isOutflow && 'text-destructive',
                      isInflow && 'text-success',
                    )}
                  >
                    {isOutflow ? '-' : isInflow ? '+' : ''}
                    {formatCurrency(t.amount)}
                  </span>
                </li>
              )
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}
