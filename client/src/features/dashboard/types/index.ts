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

export interface BalanceTrendPoint {
  year: number
  month: number
  balance: number
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
  balanceTrend: BalanceTrendPoint[]
  recentTransactions: Transaction[]
  accounts: Account[]
  insights: string[]
}
