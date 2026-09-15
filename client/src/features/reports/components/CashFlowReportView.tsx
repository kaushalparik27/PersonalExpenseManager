import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { CashFlowReport } from '@/features/reports/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

const MONTH_LABELS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

interface CashFlowReportViewProps {
  data: CashFlowReport | undefined
  isLoading: boolean
}

export function CashFlowReportView({ data, isLoading }: CashFlowReportViewProps) {
  if (isLoading || !data) {
    return <Skeleton className="h-80 rounded-xl" />
  }

  const chartData = data.monthlySeries.map((m) => ({
    month: `${MONTH_LABELS[m.month - 1]} ${String(m.year).slice(2)}`,
    Income: m.income,
    Expense: m.expense,
  }))

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card>
          <CardContent className="space-y-1">
            <p className="text-xs text-muted-foreground">Total Income</p>
            <p className="text-lg font-semibold tabular-nums text-success">{formatCurrency(data.totalIncome)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="space-y-1">
            <p className="text-xs text-muted-foreground">Total Expense</p>
            <p className="text-lg font-semibold tabular-nums text-destructive">{formatCurrency(data.totalExpense)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="space-y-1">
            <p className="text-xs text-muted-foreground">Net Cash Flow</p>
            <p className={cn('text-lg font-semibold tabular-nums', data.netCashFlow >= 0 ? 'text-success' : 'text-destructive')}>
              {formatCurrency(data.netCashFlow)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="space-y-1">
            <p className="text-xs text-muted-foreground">Savings Rate</p>
            <p className="text-lg font-semibold tabular-nums">
              {data.savingsRatePercent !== null ? `${data.savingsRatePercent}%` : '—'}
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Cash Flow Over Time</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} margin={{ left: 4, right: 8 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border)" />
                <XAxis dataKey="month" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} axisLine={{ stroke: 'var(--border)' }} tickLine={false} />
                <YAxis
                  tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }}
                  axisLine={false}
                  tickLine={false}
                  width={56}
                  tickFormatter={(value: number) => new Intl.NumberFormat('en-IN', { notation: 'compact' }).format(value)}
                />
                <Tooltip
                  formatter={(value) => formatCurrency(Number(value))}
                  contentStyle={{ background: 'var(--popover)', border: '1px solid var(--border)', borderRadius: 8, fontSize: 12, color: 'var(--popover-foreground)' }}
                />
                <Legend wrapperStyle={{ fontSize: 12 }} />
                <Bar dataKey="Income" fill="var(--chart-2)" radius={[4, 4, 0, 0]} />
                <Bar dataKey="Expense" fill="var(--chart-4)" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
