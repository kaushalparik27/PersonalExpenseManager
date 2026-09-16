import { useState } from 'react'
import { ArrowLeft, PieChart as PieChartIcon } from 'lucide-react'
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
import { Button } from '@/components/ui/button'
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
import { useCategorySummaryReport } from '@/features/reports/hooks/useReports'
import type { DateRange } from '@/lib/dateRangePresets'
import { formatCurrency } from '@/lib/utils/currency'

const COLORS = ['var(--chart-1)', 'var(--chart-2)', 'var(--chart-3)', 'var(--chart-4)', 'var(--chart-5)']

// A tiny local equivalent of dateRangePresets' computeDateRange('thisMonth') —
// written by hand instead of importing date-fns so this drilldown doesn't pull
// the whole date-fns bundle into the eagerly-needed Dashboard chunk.
function thisMonthRange(): DateRange {
  const now = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0).getDate()
  return {
    fromDate: `${now.getFullYear()}-${pad(now.getMonth() + 1)}-01`,
    toDate: `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(lastDay)}`,
  }
}

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
  const [drilldownCategory, setDrilldownCategory] = useState<string | null>(null)

  // Lazy: only fetched once the user actually drills into a category, reusing
  // the Reports feature's category-summary endpoint (categories + subcategory
  // breakdown for the period) instead of adding a dashboard-specific endpoint.
  const summaryQuery = useCategorySummaryReport(thisMonthRange(), {
    enabled: drilldownCategory !== null,
  })

  function updateStyle(value: CategoryChartStyle) {
    setStyle(value)
    saveCategoryChartStyle(value)
  }

  const drilldownSubCategories = drilldownCategory
    ? summaryQuery.data?.expenseCategories.find((c) => c.name === drilldownCategory)?.subCategories
    : undefined

  const displayData: CategoryAmount[] = drilldownCategory
    ? (drilldownSubCategories ?? []).map((s) => ({ categoryName: s.name, amount: s.amount }))
    : data

  const isDrilldownLoading = drilldownCategory !== null && summaryQuery.isLoading

  function handleSliceClick(categoryName: string | undefined) {
    if (!drilldownCategory && categoryName) {
      setDrilldownCategory(categoryName)
    }
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
      const sorted = [...displayData].sort((a, b) => b.amount - a.amount)
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
          <Bar
            dataKey="amount"
            radius={[0, 4, 4, 0]}
            cursor={drilldownCategory ? 'default' : 'pointer'}
            onClick={(entry) => handleSliceClick((entry?.payload as CategoryAmount | undefined)?.categoryName)}
          >
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
          data={displayData}
          dataKey="amount"
          nameKey="categoryName"
          innerRadius={style === 'donut' ? '55%' : 0}
          outerRadius="80%"
          paddingAngle={2}
          cursor={drilldownCategory ? 'default' : 'pointer'}
          onClick={(entry) => handleSliceClick(entry?.payload?.categoryName as string | undefined)}
        >
          {displayData.map((entry, index) => (
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
        <div className="flex items-center gap-1">
          {drilldownCategory && (
            <Button
              variant="ghost"
              size="icon"
              className="-ml-2 size-7"
              aria-label="Back to categories"
              onClick={() => setDrilldownCategory(null)}
            >
              <ArrowLeft className="size-4" />
            </Button>
          )}
          <CardTitle className="text-base">
            {drilldownCategory ? `Expense by Category — ${drilldownCategory}` : 'Expense by Category'}
          </CardTitle>
        </div>
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
        ) : isDrilldownLoading ? (
          <div className="flex h-72 items-center justify-center text-sm text-muted-foreground">Loading…</div>
        ) : drilldownCategory && displayData.length === 0 ? (
          <div className="flex h-72 flex-col items-center justify-center gap-1 text-center">
            <p className="text-sm font-medium text-foreground">No subcategory data</p>
            <p className="text-sm text-muted-foreground">
              {drilldownCategory} has no subcategory breakdown for this month.
            </p>
          </div>
        ) : (
          <>
            <div className="h-72">
              <ResponsiveContainer width="100%" height="100%">
                {renderChart()}
              </ResponsiveContainer>
            </div>
            {!drilldownCategory && (
              <p className="mt-2 text-center text-xs text-muted-foreground">Click a category to see subcategories</p>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}
