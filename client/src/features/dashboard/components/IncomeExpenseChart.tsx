import { useState } from 'react'
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  getChartPreferences,
  saveChartPreferences,
  type ChartSeries,
  type ChartStyle,
} from '@/features/dashboard/lib/chartPreferences'
import type { MonthlyAmount } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'

const MONTH_LABELS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

const STYLE_OPTIONS: { value: ChartStyle; label: string }[] = [
  { value: 'bar', label: 'Bar' },
  { value: 'line', label: 'Line' },
  { value: 'area', label: 'Area' },
]

const SERIES_OPTIONS: { value: ChartSeries; label: string }[] = [
  { value: 'both', label: 'Income & Expense' },
  { value: 'income', label: 'Income only' },
  { value: 'expense', label: 'Expense only' },
]

interface IncomeExpenseChartProps {
  data: MonthlyAmount[]
}

export function IncomeExpenseChart({ data }: IncomeExpenseChartProps) {
  const [preferences, setPreferences] = useState(getChartPreferences)

  function updatePreferences(update: Partial<{ style: ChartStyle; series: ChartSeries }>) {
    setPreferences((prev) => {
      const next = { ...prev, ...update }
      saveChartPreferences(next)
      return next
    })
  }

  const chartData = data.map((d) => ({
    month: `${MONTH_LABELS[d.month - 1]} ${String(d.year).slice(2)}`,
    Income: d.income,
    Expense: d.expense,
  }))

  const showIncome = preferences.series !== 'expense'
  const showExpense = preferences.series !== 'income'

  const yAxisTickFormatter = (value: number) =>
    new Intl.NumberFormat('en-IN', { notation: 'compact' }).format(value)

  const tooltipContentStyle = {
    background: 'var(--popover)',
    border: '1px solid var(--border)',
    borderRadius: 8,
    fontSize: 12,
    color: 'var(--popover-foreground)',
  }

  function renderChart() {
    if (preferences.style === 'line') {
      return (
        <LineChart data={chartData} margin={{ left: 4, right: 8 }}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border)" />
          <XAxis dataKey="month" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} axisLine={{ stroke: 'var(--border)' }} tickLine={false} />
          <YAxis tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }} axisLine={false} tickLine={false} width={56} tickFormatter={yAxisTickFormatter} />
          <Tooltip formatter={(value) => formatCurrency(Number(value))} contentStyle={tooltipContentStyle} />
          <Legend wrapperStyle={{ fontSize: 12 }} />
          {showIncome && <Line type="monotone" dataKey="Income" stroke="var(--chart-2)" strokeWidth={2} dot={{ r: 3 }} />}
          {showExpense && <Line type="monotone" dataKey="Expense" stroke="var(--chart-4)" strokeWidth={2} dot={{ r: 3 }} />}
        </LineChart>
      )
    }

    if (preferences.style === 'area') {
      return (
        <AreaChart data={chartData} margin={{ left: 4, right: 8 }}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border)" />
          <XAxis dataKey="month" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} axisLine={{ stroke: 'var(--border)' }} tickLine={false} />
          <YAxis tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }} axisLine={false} tickLine={false} width={56} tickFormatter={yAxisTickFormatter} />
          <Tooltip formatter={(value) => formatCurrency(Number(value))} contentStyle={tooltipContentStyle} />
          <Legend wrapperStyle={{ fontSize: 12 }} />
          {showIncome && <Area type="monotone" dataKey="Income" stroke="var(--chart-2)" fill="var(--chart-2)" fillOpacity={0.25} strokeWidth={2} />}
          {showExpense && <Area type="monotone" dataKey="Expense" stroke="var(--chart-4)" fill="var(--chart-4)" fillOpacity={0.25} strokeWidth={2} />}
        </AreaChart>
      )
    }

    return (
      <BarChart data={chartData} margin={{ left: 4, right: 8 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--border)" />
        <XAxis dataKey="month" tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }} axisLine={{ stroke: 'var(--border)' }} tickLine={false} />
        <YAxis tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }} axisLine={false} tickLine={false} width={56} tickFormatter={yAxisTickFormatter} />
        <Tooltip formatter={(value) => formatCurrency(Number(value))} contentStyle={tooltipContentStyle} />
        <Legend wrapperStyle={{ fontSize: 12 }} />
        {showIncome && <Bar dataKey="Income" fill="var(--chart-2)" radius={[4, 4, 0, 0]} />}
        {showExpense && <Bar dataKey="Expense" fill="var(--chart-4)" radius={[4, 4, 0, 0]} />}
      </BarChart>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <CardTitle className="text-base">Income vs Expense</CardTitle>
        <div className="flex flex-wrap gap-2">
          <Select value={preferences.series} onValueChange={(value) => updatePreferences({ series: value as ChartSeries })}>
            <SelectTrigger size="sm" aria-label="Series to show">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {SERIES_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select value={preferences.style} onValueChange={(value) => updatePreferences({ style: value as ChartStyle })}>
            <SelectTrigger size="sm" aria-label="Chart style">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {STYLE_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      <CardContent>
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            {renderChart()}
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  )
}
