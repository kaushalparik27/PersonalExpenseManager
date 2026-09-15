import { useState } from 'react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { DateRangePicker } from '@/components/common/DateRangePicker'
import { AccountSummaryReportView } from '@/features/reports/components/AccountSummaryReportView'
import { CashFlowReportView } from '@/features/reports/components/CashFlowReportView'
import { CategorySummaryReportView } from '@/features/reports/components/CategorySummaryReportView'
import { DrilldownTransactionsDialog } from '@/features/reports/components/DrilldownTransactionsDialog'
import { IncomeExpenseReportView } from '@/features/reports/components/IncomeExpenseReportView'
import { MonthlyTrendReportView } from '@/features/reports/components/MonthlyTrendReportView'
import {
  useAccountSummaryReport,
  useCashFlowReport,
  useCategorySummaryReport,
  useExpenseReport,
  useIncomeReport,
  useMonthlyTrendReport,
} from '@/features/reports/hooks/useReports'
import { computeDateRange, type DateRange, type DateRangePreset } from '@/lib/dateRangePresets'

interface DrilldownTarget {
  title: string
  categoryId?: string
  subCategoryId?: string
}

export function ReportsPage() {
  const [preset, setPreset] = useState<DateRangePreset>('thisMonth')
  const [range, setRange] = useState<DateRange>(() => computeDateRange('thisMonth'))
  const [drilldown, setDrilldown] = useState<DrilldownTarget | null>(null)

  function handleRangeChange(nextPreset: DateRangePreset, nextRange: DateRange) {
    setPreset(nextPreset)
    setRange(nextRange)
  }

  const incomeQuery = useIncomeReport(range)
  const expenseQuery = useExpenseReport(range)
  const cashFlowQuery = useCashFlowReport(range)
  const categorySummaryQuery = useCategorySummaryReport(range)
  const accountSummaryQuery = useAccountSummaryReport(range)
  const monthlyTrendQuery = useMonthlyTrendReport(range)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-lg font-semibold text-foreground">Reports</h1>
        <p className="text-sm text-muted-foreground">Analyze your income, expenses, and account activity.</p>
      </div>

      <DateRangePicker preset={preset} range={range} onChange={handleRangeChange} />

      <Tabs defaultValue="cash-flow">
        <TabsList className="flex-wrap">
          <TabsTrigger value="cash-flow">Cash Flow</TabsTrigger>
          <TabsTrigger value="income">Income</TabsTrigger>
          <TabsTrigger value="expenses">Expenses</TabsTrigger>
          <TabsTrigger value="categories">Category Analysis</TabsTrigger>
          <TabsTrigger value="accounts">Account Analysis</TabsTrigger>
          <TabsTrigger value="trends">Monthly Trends</TabsTrigger>
        </TabsList>

        <TabsContent value="cash-flow" className="mt-4">
          <CashFlowReportView data={cashFlowQuery.data} isLoading={cashFlowQuery.isLoading} />
        </TabsContent>

        <TabsContent value="income" className="mt-4">
          <IncomeExpenseReportView
            data={incomeQuery.data}
            isLoading={incomeQuery.isLoading}
            valueKey="income"
            label="Income"
            barColor="var(--chart-2)"
          />
        </TabsContent>

        <TabsContent value="expenses" className="mt-4">
          <IncomeExpenseReportView
            data={expenseQuery.data}
            isLoading={expenseQuery.isLoading}
            valueKey="expense"
            label="Expenses"
            barColor="var(--chart-4)"
          />
        </TabsContent>

        <TabsContent value="categories" className="mt-4">
          <CategorySummaryReportView
            data={categorySummaryQuery.data}
            isLoading={categorySummaryQuery.isLoading}
            onDrilldown={setDrilldown}
          />
        </TabsContent>

        <TabsContent value="accounts" className="mt-4">
          <AccountSummaryReportView data={accountSummaryQuery.data} isLoading={accountSummaryQuery.isLoading} />
        </TabsContent>

        <TabsContent value="trends" className="mt-4">
          <MonthlyTrendReportView data={monthlyTrendQuery.data} isLoading={monthlyTrendQuery.isLoading} />
        </TabsContent>
      </Tabs>

      <DrilldownTransactionsDialog target={drilldown} range={range} onOpenChange={(open) => !open && setDrilldown(null)} />
    </div>
  )
}
