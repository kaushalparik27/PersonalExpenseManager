import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import type { IncomeExpenseReport } from '@/features/reports/types'
import { formatCurrency } from '@/lib/utils/currency'
import { FileQuestion } from 'lucide-react'

const MONTH_LABELS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

interface IncomeExpenseReportViewProps {
  data: IncomeExpenseReport | undefined
  isLoading: boolean
  valueKey: 'income' | 'expense'
  label: string
  barColor: string
}

export function IncomeExpenseReportView({ data, isLoading, valueKey, label, barColor }: IncomeExpenseReportViewProps) {
  if (isLoading) {
    return <Skeleton className="h-80 rounded-xl" />
  }

  if (!data || data.totalAmount === 0) {
    return (
      <PlaceholderPage
        icon={FileQuestion}
        title={`No ${label.toLowerCase()} in this period`}
        description="Try a different date range."
      />
    )
  }

  const chartData = data.monthlySeries.map((m) => ({
    month: `${MONTH_LABELS[m.month - 1]} ${String(m.year).slice(2)}`,
    value: m[valueKey],
  }))

  return (
    <div className="space-y-4">
      <Card>
        <CardContent className="flex items-baseline justify-between">
          <span className="text-sm text-muted-foreground">Total {label}</span>
          <span className="text-2xl font-semibold tabular-nums">{formatCurrency(data.totalAmount)}</span>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">{label} Trend</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="h-64">
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
                <Bar dataKey="value" name={label} fill={barColor} radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">By Category</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          {data.categories.length === 0 ? (
            <p className="text-sm text-muted-foreground">No categorized transactions in this period.</p>
          ) : (
            data.categories.map((c) => {
              const percent = data.totalAmount > 0 ? Math.round((c.amount / data.totalAmount) * 100) : 0
              return (
                <div key={c.categoryName} className="space-y-1">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-foreground">{c.categoryName}</span>
                    <span className="tabular-nums text-muted-foreground">{formatCurrency(c.amount)}</span>
                  </div>
                  <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
                    <div className="h-full rounded-full" style={{ width: `${percent}%`, backgroundColor: barColor }} />
                  </div>
                </div>
              )
            })
          )}
        </CardContent>
      </Card>
    </div>
  )
}
