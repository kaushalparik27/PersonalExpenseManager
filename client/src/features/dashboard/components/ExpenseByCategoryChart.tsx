import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import type { CategoryAmount } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'
import { PieChart as PieChartIcon } from 'lucide-react'

const COLORS = ['var(--chart-1)', 'var(--chart-2)', 'var(--chart-3)', 'var(--chart-4)', 'var(--chart-5)']

interface ExpenseByCategoryChartProps {
  data: CategoryAmount[]
}

export function ExpenseByCategoryChart({ data }: ExpenseByCategoryChartProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Expense by Category</CardTitle>
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
              <PieChart>
                <Pie
                  data={data}
                  dataKey="amount"
                  nameKey="categoryName"
                  innerRadius="55%"
                  outerRadius="80%"
                  paddingAngle={2}
                >
                  {data.map((entry, index) => (
                    <Cell key={entry.categoryName} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
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
                <Legend wrapperStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
