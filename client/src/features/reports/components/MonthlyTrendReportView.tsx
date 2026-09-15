import { CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type { MonthlyTrendReport } from '@/features/reports/types'
import { formatCurrency } from '@/lib/utils/currency'

const MONTH_LABELS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

interface MonthlyTrendReportViewProps {
  data: MonthlyTrendReport | undefined
  isLoading: boolean
}

export function MonthlyTrendReportView({ data, isLoading }: MonthlyTrendReportViewProps) {
  if (isLoading || !data) {
    return <Skeleton className="h-80 rounded-xl" />
  }

  const chartData = data.monthlySeries.map((m) => ({
    month: `${MONTH_LABELS[m.month - 1]} ${String(m.year).slice(2)}`,
    Income: m.income,
    Expense: m.expense,
  }))

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Monthly Trend</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-80">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData} margin={{ left: 4, right: 8 }}>
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
              <Line type="monotone" dataKey="Income" stroke="var(--chart-2)" strokeWidth={2} dot={{ r: 3 }} />
              <Line type="monotone" dataKey="Expense" stroke="var(--chart-4)" strokeWidth={2} dot={{ r: 3 }} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  )
}
