import { ArrowLeftRight, PiggyBank, TrendingDown, TrendingUp, Wallet } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import type { Dashboard } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

interface SummaryCardsProps {
  dashboard: Dashboard
}

export function SummaryCards({ dashboard }: SummaryCardsProps) {
  const cards = [
    {
      label: 'Total Balance',
      value: dashboard.totalBalance,
      icon: Wallet,
      sub: dashboard.balanceByAccountType.map((b) => `${b.accountType} ${formatCurrency(b.balance)}`).join(' · '),
      tone: dashboard.totalBalance < 0 ? 'negative' : 'neutral',
    },
    {
      label: 'This Month Income',
      value: dashboard.thisMonthIncome,
      icon: TrendingUp,
      sub: 'Income recorded this month',
      tone: 'positive',
    },
    {
      label: 'This Month Expenses',
      value: dashboard.thisMonthExpense,
      icon: TrendingDown,
      sub: 'Spending recorded this month',
      tone: 'negative',
    },
    {
      label: 'Net Cash Flow',
      value: dashboard.netCashFlow,
      icon: dashboard.netCashFlow >= 0 ? PiggyBank : ArrowLeftRight,
      sub: dashboard.savingsRatePercent !== null ? `${dashboard.savingsRatePercent}% savings rate` : 'Income − Expenses',
      tone: dashboard.netCashFlow >= 0 ? 'positive' : 'negative',
    },
  ] as const

  return (
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
      {cards.map(({ label, value, icon: Icon, sub, tone }) => (
        <Card key={label}>
          <CardContent className="space-y-2">
            <div className="flex items-center justify-between">
              <span className="text-xs font-medium text-muted-foreground">{label}</span>
              <Icon className="size-4 text-muted-foreground" />
            </div>
            <p
              className={cn(
                'text-xl font-semibold tabular-nums',
                tone === 'positive' && 'text-success',
                tone === 'negative' && 'text-destructive',
                tone === 'neutral' && 'text-foreground',
              )}
            >
              {formatCurrency(value)}
            </p>
            <p className="truncate text-xs text-muted-foreground" title={sub}>
              {sub}
            </p>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
