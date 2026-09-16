import { useState } from 'react'
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { useBalanceTrend } from '@/features/dashboard/hooks/useDashboard'
import type { BalanceTrendGranularity } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'

const GRANULARITY_OPTIONS: { value: BalanceTrendGranularity; label: string }[] = [
  { value: 'Daily', label: 'Daily' },
  { value: 'Weekly', label: 'Weekly' },
  { value: 'BiWeekly', label: 'Bi-Weekly' },
  { value: 'Monthly', label: 'Monthly' },
  { value: 'Yearly', label: 'Yearly' },
]

function formatPeriodLabel(periodStart: string, granularity: BalanceTrendGranularity): string {
  const date = new Date(`${periodStart}T00:00:00`)

  if (granularity === 'Yearly') {
    return new Intl.DateTimeFormat('en-IN', { year: 'numeric' }).format(date)
  }
  if (granularity === 'Monthly') {
    return new Intl.DateTimeFormat('en-IN', { month: 'short', year: '2-digit' }).format(date)
  }
  return new Intl.DateTimeFormat('en-IN', { day: 'numeric', month: 'short' }).format(date)
}

interface BalanceTrendChartProps {
  totalBalance: number
}

export function BalanceTrendChart({ totalBalance }: BalanceTrendChartProps) {
  const [granularity, setGranularity] = useState<BalanceTrendGranularity>('Monthly')
  const { data, isLoading } = useBalanceTrend(granularity)

  const chartData = (data ?? []).map((d) => ({
    period: formatPeriodLabel(d.periodStart, granularity),
    Balance: d.balance,
  }))

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <CardTitle className="text-base">Balance Trend</CardTitle>
          <p className="mt-0.5 text-lg font-semibold tabular-nums text-foreground">{formatCurrency(totalBalance)}</p>
        </div>
        <Select value={granularity} onValueChange={(value) => setGranularity(value as BalanceTrendGranularity)}>
          <SelectTrigger size="sm" aria-label="Trend duration">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {GRANULARITY_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <Skeleton className="h-72 w-full rounded-lg" />
        ) : chartData.length === 0 ? (
          <div className="flex h-72 items-center justify-center text-sm text-muted-foreground">
            No balance history to show yet.
          </div>
        ) : (
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData} margin={{ left: 4, right: 8 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border)" />
                <XAxis dataKey="period" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} axisLine={{ stroke: 'var(--border)' }} tickLine={false} />
                <YAxis
                  tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }}
                  axisLine={false}
                  tickLine={false}
                  width={56}
                  tickFormatter={(value: number) => new Intl.NumberFormat('en-IN', { notation: 'compact' }).format(value)}
                />
                <Tooltip
                  formatter={(value) => formatCurrency(Number(value))}
                  contentStyle={{
                    background: 'var(--popover)',
                    border: '1px solid var(--border)',
                    borderRadius: 8,
                    fontSize: 12,
                    color: 'var(--popover-foreground)',
                  }}
                />
                <Line type="monotone" dataKey="Balance" stroke="var(--chart-1)" strokeWidth={2} dot={{ r: 3 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
