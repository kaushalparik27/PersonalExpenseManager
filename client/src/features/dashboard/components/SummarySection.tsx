import { useState } from 'react'
import { SlidersHorizontal } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
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

export function SummarySection() {
  const { data, isLoading } = useSummary()
  const [selected, setSelected] = useState<SummaryPeriod[]>(getSummaryPeriods)

  function toggle(period: SummaryPeriod) {
    setSelected((prev) => {
      const isSelected = prev.includes(period)
      if (isSelected && prev.length <= MIN_PERIODS) return prev
      if (!isSelected && prev.length >= MAX_PERIODS) return prev

      const next = isSelected ? prev.filter((p) => p !== period) : [...prev, period]
      saveSummaryPeriods(next)
      return next
    })
  }

  const byPeriod = new Map((data ?? []).map((s) => [s.period, s]))
  const visibleOptions = SUMMARY_PERIOD_OPTIONS.filter((o) => selected.includes(o.value))

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <CardTitle className="text-base">Summary</CardTitle>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="size-8" aria-label="Choose summary periods">
              <SlidersHorizontal className="size-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuLabel>Show periods (1–6)</DropdownMenuLabel>
            <DropdownMenuSeparator />
            {SUMMARY_PERIOD_OPTIONS.map((option) => {
              const checked = selected.includes(option.value)
              const disabled = (checked && selected.length <= MIN_PERIODS) || (!checked && selected.length >= MAX_PERIODS)
              return (
                <DropdownMenuCheckboxItem
                  key={option.value}
                  checked={checked}
                  disabled={disabled}
                  onSelect={(e) => {
                    e.preventDefault()
                    toggle(option.value)
                  }}
                >
                  {option.label}
                </DropdownMenuCheckboxItem>
              )
            })}
          </DropdownMenuContent>
        </DropdownMenu>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {isLoading
            ? visibleOptions.map((option) => <Skeleton key={option.value} className="h-24 rounded-lg" />)
            : visibleOptions.map((option) => {
                const summary = byPeriod.get(option.value)
                return (
                  <div key={option.value} className="rounded-lg border border-border p-3">
                    <p className="text-xs font-medium text-muted-foreground">{option.label}</p>
                    <div className="mt-2 flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Income</span>
                      <span className="font-semibold tabular-nums text-success">{formatCurrency(summary?.income ?? 0)}</span>
                    </div>
                    <div className="mt-1 flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">Expense</span>
                      <span className="font-semibold tabular-nums text-destructive">{formatCurrency(summary?.expense ?? 0)}</span>
                    </div>
                  </div>
                )
              })}
        </div>
      </CardContent>
    </Card>
  )
}
