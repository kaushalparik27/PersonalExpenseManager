import type { Account } from '@/features/accounts/types'
import type { Transaction } from '@/features/transactions/types'

export interface AccountTypeBalance {
  accountType: string
  balance: number
}

export interface CategoryAmount {
  categoryName: string
  amount: number
}

export interface MonthlyAmount {
  year: number
  month: number
  income: number
  expense: number
}

export type BalanceTrendGranularity = 'Daily' | 'Weekly' | 'BiWeekly' | 'Monthly' | 'Yearly'

export interface BalanceTrendPoint {
  periodStart: string
  balance: number
}

export type SummaryPeriod =
  | 'Today'
  | 'Yesterday'
  | 'Last7Days'
  | 'ThisMonth'
  | 'Last31Days'
  | 'LastMonth'
  | 'ThisYear'
  | 'LastYear'
  | 'Total'

export interface PeriodSummary {
  period: SummaryPeriod
  income: number
  expense: number
}

export interface Dashboard {
  totalBalance: number
  balanceByAccountType: AccountTypeBalance[]
  thisMonthIncome: number
  thisMonthExpense: number
  netCashFlow: number
  savingsRatePercent: number | null
  expenseGrowthPercent: number | null
  topExpenseCategories: CategoryAmount[]
  incomeVsExpenseTrend: MonthlyAmount[]
  recentTransactions: Transaction[]
  accounts: Account[]
  insights: string[]
}
