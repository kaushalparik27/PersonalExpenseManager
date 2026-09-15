import { apiClient } from '@/lib/api/client'
import type { DateRange } from '@/lib/dateRangePresets'
import type {
  AccountSummaryReport,
  CashFlowReport,
  CategorySummaryReport,
  IncomeExpenseReport,
  MonthlyTrendReport,
} from '@/features/reports/types'

function query(range: DateRange) {
  return `?fromDate=${range.fromDate}&toDate=${range.toDate}`
}

export const reportsApi = {
  income: (range: DateRange) => apiClient.get<IncomeExpenseReport>(`/api/reports/income${query(range)}`),
  expenses: (range: DateRange) => apiClient.get<IncomeExpenseReport>(`/api/reports/expenses${query(range)}`),
  cashFlow: (range: DateRange) => apiClient.get<CashFlowReport>(`/api/reports/cash-flow${query(range)}`),
  categorySummary: (range: DateRange) =>
    apiClient.get<CategorySummaryReport>(`/api/reports/category-summary${query(range)}`),
  accountSummary: (range: DateRange) =>
    apiClient.get<AccountSummaryReport>(`/api/reports/account-summary${query(range)}`),
  monthlyTrend: (range: DateRange) =>
    apiClient.get<MonthlyTrendReport>(`/api/reports/monthly-trend${query(range)}`),
}
