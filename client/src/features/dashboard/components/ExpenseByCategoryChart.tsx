import { useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  getCategoryChartStyle,
  saveCategoryChartStyle,
  type CategoryChartStyle,
} from '@/features/dashboard/lib/categoryChartPreferences'
import type { CategoryAmount } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'
import { PieChart as PieChartIcon } from 'lucide-react'

const COLORS = ['var(--chart-1)', 'var(--chart-2)', 'var(--chart-3)', 'var(--chart-4)', 'var(--chart-5)']

const STYLE_OPTIONS: { value: CategoryChartStyle; label: string }[] = [
  { value: 'donut', label: 'Donut' },
  { value: 'pie', label: 'Pie' },
  { value: 'bar', label: 'Bar' },
]

interface ExpenseByCategoryChartProps {
  data: CategoryAmount[]
}

export function ExpenseByCategoryChart({ data }: ExpenseByCategoryChartProps) {
  const [style, setStyle] = useState(getCategoryChartStyle)

  function updateStyle(value: CategoryChartStyle) {
    setStyle(value)
    saveCategoryChartStyle(value)
  }

  const tooltipContentStyle = {
    background: 'var(--popover)',
    border: '1px solid var(--border)',
    borderRadius: 8,
    fontSize: 12,
    color: 'var(--popover-foreground)',
  }

  function renderChart() {
    if (style === 'bar') {
      const sorted = [...data].sort((a, b) => b.amount - a.amount)
      return (
        <BarChart data={sorted} layout="vertical" margin={{ left: 8, right: 16 }}>
          <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="var(--border)" />
          <XAxis
            type="number"
            tick={{ fontSize: 11, fill: 'var(--muted-foreground)' }}
            axisLine={false}
            tickLine={false}
            tickFormatter={(value: number) => new Intl.NumberFormat('en-IN', { notation: 'compact' }).format(value)}
          />
          <YAxis
            type="category"
            dataKey="categoryName"
            tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }}
            axisLine={false}
            tickLine={false}
            width={90}
          />
          <Tooltip formatter={(value) => formatCurrency(Number(value))} contentStyle={tooltipContentStyle} />
          <Bar dataKey="amount" radius={[0, 4, 4, 0]}>
            {sorted.map((entry, index) => (
              <Cell key={entry.categoryName} fill={COLORS[index % COLORS.length]} />
            ))}
          </Bar>
        </BarChart>
      )
    }

    return (
      <PieChart>
        <Pie
          data={data}
          dataKey="amount"
          nameKey="categoryName"
          innerRadius={style === 'donut' ? '55%' : 0}
          outerRadius="80%"
          paddingAngle={2}
        >
          {data.map((entry, index) => (
            <Cell key={entry.categoryName} fill={COLORS[index % COLORS.length]} />
          ))}
        </Pie>
        <Tooltip formatter={(value) => formatCurrency(Number(value))} contentStyle={tooltipContentStyle} />
        <Legend wrapperStyle={{ fontSize: 12 }} />
      </PieChart>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <CardTitle className="text-base">Expense by Category</CardTitle>
        {data.length > 0 && (
          <Select value={style} onValueChange={(value) => updateStyle(value as CategoryChartStyle)}>
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
        )}
      </CardHeader>
      <CardContent>
        {data.length === 0 ? (
          <PlaceholderPage
            icon={PieChartIcon}
            title="No expenses this month"
            description="Categories will appear here once you record an expense."
          />
        ) : (
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              {renderChart()}
            </ResponsiveContainer>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
