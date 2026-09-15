import { ChevronDown, ChevronRight, Tags } from 'lucide-react'
import { useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import type { CategorySummaryCategory, CategorySummaryReport } from '@/features/reports/types'
import { formatCurrency } from '@/lib/utils/currency'

interface DrilldownTarget {
  title: string
  categoryId?: string
  subCategoryId?: string
}

interface CategorySummaryReportViewProps {
  data: CategorySummaryReport | undefined
  isLoading: boolean
  onDrilldown: (target: DrilldownTarget) => void
}

function CategoryList({
  title,
  categories,
  onDrilldown,
}: {
  title: string
  categories: CategorySummaryCategory[]
  onDrilldown: (target: DrilldownTarget) => void
}) {
  const [expanded, setExpanded] = useState<Record<string, boolean>>({})
  const total = categories.reduce((sum, c) => sum + c.amount, 0)

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {categories.length === 0 ? (
          <p className="text-sm text-muted-foreground">No transactions in this period.</p>
        ) : (
          <div className="divide-y divide-border">
            {categories.map((category) => {
              const isExpanded = expanded[category.id] ?? false
              const percent = total > 0 ? Math.round((category.amount / total) * 100) : 0

              return (
                <div key={category.id}>
                  <button
                    type="button"
                    onClick={() => setExpanded((prev) => ({ ...prev, [category.id]: !isExpanded }))}
                    className="flex w-full items-center gap-2 py-2.5 text-left"
                  >
                    {category.subCategories.length > 0 ? (
                      isExpanded ? (
                        <ChevronDown className="size-4 shrink-0 text-muted-foreground" />
                      ) : (
                        <ChevronRight className="size-4 shrink-0 text-muted-foreground" />
                      )
                    ) : (
                      <span className="size-4 shrink-0" />
                    )}
                    <span
                      className="flex-1 truncate text-sm font-medium text-foreground hover:underline"
                      onClick={(e) => {
                        e.stopPropagation()
                        onDrilldown({ title: category.name, categoryId: category.id })
                      }}
                    >
                      {category.name}
                    </span>
                    <span className="text-xs text-muted-foreground">{percent}%</span>
                    <span className="w-24 text-right text-sm font-medium tabular-nums">{formatCurrency(category.amount)}</span>
                  </button>

                  {isExpanded && category.subCategories.length > 0 && (
                    <div className="space-y-1.5 pb-2 pl-6">
                      {category.subCategories.map((sub) => (
                        <button
                          key={sub.id}
                          type="button"
                          onClick={() => onDrilldown({ title: `${category.name} · ${sub.name}`, subCategoryId: sub.id })}
                          className="flex w-full items-center justify-between rounded px-2 py-1 text-left text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                        >
                          <span>{sub.name}</span>
                          <span className="tabular-nums">{formatCurrency(sub.amount)}</span>
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

export function CategorySummaryReportView({ data, isLoading, onDrilldown }: CategorySummaryReportViewProps) {
  if (isLoading) {
    return <Skeleton className="h-80 rounded-xl" />
  }

  if (!data || (data.expenseCategories.length === 0 && data.incomeCategories.length === 0)) {
    return (
      <PlaceholderPage
        icon={Tags}
        title="No categorized activity"
        description="Categories with transactions in this period will appear here."
      />
    )
  }

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <CategoryList title="Expense Categories" categories={data.expenseCategories} onDrilldown={onDrilldown} />
      <CategoryList title="Income Categories" categories={data.incomeCategories} onDrilldown={onDrilldown} />
    </div>
  )
}
