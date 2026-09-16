import { useState } from 'react'
import { ArrowDown, ArrowUp, Plus, SlidersHorizontal, X } from 'lucide-react'
import { Cell, Pie, PieChart } from 'recharts'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Skeleton } from '@/components/ui/skeleton'
import { useSummary } from '@/features/dashboard/hooks/useDashboard'
import {
  MAX_PERIODS,
  MIN_PERIODS,
  SUMMARY_PERIOD_OPTIONS,
  getSummaryPeriods,
  saveSummaryPeriods,
} from '@/features/dashboard/lib/summaryPreferences'
import type { SummaryPeriod } from '@/features/dashboard/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

const PERIOD_LABELS = Object.fromEntries(
  SUMMARY_PERIOD_OPTIONS.map((o) => [o.value, o.label]),
) as Record<SummaryPeriod, string>

function PeriodMiniChart({ income, expense }: { income: number; expense: number }) {
  const isEmpty = income === 0 && expense === 0
  const data = isEmpty
    ? [{ name: 'Empty', value: 1, fill: 'var(--muted)' }]
    : [
        { name: 'Income', value: income, fill: 'var(--chart-2)' },
        { name: 'Expense', value: expense, fill: 'var(--chart-4)' },
      ]

  return (
    <PieChart width={48} height={48}>
      <Pie data={data} dataKey="value" cx="100%" cy="50%" startAngle={90} endAngle={270} innerRadius={14} outerRadius={24}>
        {data.map((entry) => (
          <Cell key={entry.name} fill={entry.fill} />
        ))}
      </Pie>
    </PieChart>
  )
}

export function SummarySection() {
  const { data, isLoading } = useSummary()
  const [selected, setSelected] = useState<SummaryPeriod[]>(getSummaryPeriods)

  function persist(next: SummaryPeriod[]) {
    setSelected(next)
    saveSummaryPeriods(next)
  }

  function addPeriod(period: SummaryPeriod) {
    if (selected.length >= MAX_PERIODS) return
    persist([...selected, period])
  }

  function removePeriod(period: SummaryPeriod) {
    if (selected.length <= MIN_PERIODS) return
    persist(selected.filter((p) => p !== period))
  }

  function moveUp(period: SummaryPeriod) {
    const index = selected.indexOf(period)
    if (index <= 0) return
    const next = [...selected]
    ;[next[index - 1], next[index]] = [next[index], next[index - 1]]
    persist(next)
  }

  function moveDown(period: SummaryPeriod) {
    const index = selected.indexOf(period)
    if (index === -1 || index >= selected.length - 1) return
    const next = [...selected]
    ;[next[index], next[index + 1]] = [next[index + 1], next[index]]
    persist(next)
  }

  const byPeriod = new Map((data ?? []).map((s) => [s.period, s]))
  const availableOptions = SUMMARY_PERIOD_OPTIONS.filter((o) => !selected.includes(o.value))

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-base">Summary</CardTitle>
        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon" className="size-8" aria-label="Configure summary periods">
              <SlidersHorizontal className="size-4" />
            </Button>
          </PopoverTrigger>
          <PopoverContent align="end" className="w-72">
            <p className="mb-1.5 px-1 text-xs font-medium text-muted-foreground">Shown periods ({selected.length}/{MAX_PERIODS})</p>
            <div className="space-y-0.5">
              {selected.map((period, index) => (
                <div key={period} className="flex items-center justify-between gap-2 rounded-md px-1 py-1 text-sm hover:bg-accent">
                  <span>{PERIOD_LABELS[period]}</span>
                  <div className="flex items-center gap-0.5">
                    <Button
                      variant="ghost"
                      size="icon-xs"
                      aria-label={`Move ${PERIOD_LABELS[period]} up`}
                      disabled={index === 0}
                      onClick={() => moveUp(period)}
                    >
                      <ArrowUp className="size-3.5" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-xs"
                      aria-label={`Move ${PERIOD_LABELS[period]} down`}
                      disabled={index === selected.length - 1}
                      onClick={() => moveDown(period)}
                    >
                      <ArrowDown className="size-3.5" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-xs"
                      aria-label={`Remove ${PERIOD_LABELS[period]}`}
                      disabled={selected.length <= MIN_PERIODS}
                      onClick={() => removePeriod(period)}
                    >
                      <X className="size-3.5" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>

            {availableOptions.length > 0 && (
              <>
                <p className="mt-3 mb-1.5 px-1 text-xs font-medium text-muted-foreground">Add a period</p>
                <div className="space-y-0.5">
                  {availableOptions.map((option) => (
                    <button
                      key={option.value}
                      type="button"
                      disabled={selected.length >= MAX_PERIODS}
                      onClick={() => addPeriod(option.value)}
                      className="flex w-full items-center gap-2 rounded-md px-1 py-1 text-left text-sm hover:bg-accent disabled:pointer-events-none disabled:opacity-50"
                    >
                      <Plus className="size-3.5 text-muted-foreground" />
                      {option.label}
                    </button>
                  ))}
                </div>
              </>
            )}
          </PopoverContent>
        </Popover>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          {isLoading
            ? selected.map((period) => <Skeleton key={period} className="h-32 rounded-lg" />)
            : selected.map((period) => {
                const summary = byPeriod.get(period)
                const income = summary?.income ?? 0
                const expense = summary?.expense ?? 0
                const total = income - expense
                return (
                  <div key={period} className="flex items-center gap-3 rounded-lg border border-border p-3">
                    <PeriodMiniChart income={income} expense={expense} />
                    <div className="min-w-0 flex-1">
                      <p className="text-xs font-medium text-muted-foreground">{PERIOD_LABELS[period]}</p>
                      <div className="mt-1.5 flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">Income</span>
                        <span className="font-semibold tabular-nums text-success">{formatCurrency(income)}</span>
                      </div>
                      <div className="mt-0.5 flex items-center justify-between text-sm">
                        <span className="text-muted-foreground">Expense</span>
                        <span className="font-semibold tabular-nums text-destructive">{formatCurrency(expense)}</span>
                      </div>
                      <div className="mt-1 flex items-center justify-between border-t border-border pt-1 text-sm">
                        <span className="font-medium text-muted-foreground">Total</span>
                        <span
                          className={cn(
                            'font-bold tabular-nums',
                            total < 0 ? 'text-destructive' : 'text-success',
                          )}
                        >
                          {formatCurrency(total)}
                        </span>
                      </div>
                    </div>
                  </div>
                )
              })}
        </div>
      </CardContent>
    </Card>
  )
}
