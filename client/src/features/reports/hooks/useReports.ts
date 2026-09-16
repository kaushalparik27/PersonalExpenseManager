import { useQuery } from '@tanstack/react-query'
import { reportsApi } from '@/features/reports/api/reportsApi'
import type { DateRange } from '@/lib/dateRangePresets'

export function useIncomeReport(range: DateRange) {
  return useQuery({ queryKey: ['reports', 'income', range], queryFn: () => reportsApi.income(range) })
}

export function useExpenseReport(range: DateRange) {
  return useQuery({ queryKey: ['reports', 'expenses', range], queryFn: () => reportsApi.expenses(range) })
}

export function useCashFlowReport(range: DateRange) {
  return useQuery({ queryKey: ['reports', 'cash-flow', range], queryFn: () => reportsApi.cashFlow(range) })
}

export function useCategorySummaryReport(range: DateRange, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ['reports', 'category-summary', range],
    queryFn: () => reportsApi.categorySummary(range),
    enabled: options?.enabled,
  })
}

export function useAccountSummaryReport(range: DateRange) {
  return useQuery({ queryKey: ['reports', 'account-summary', range], queryFn: () => reportsApi.accountSummary(range) })
}

export function useMonthlyTrendReport(range: DateRange) {
  return useQuery({ queryKey: ['reports', 'monthly-trend', range], queryFn: () => reportsApi.monthlyTrend(range) })
}
