export interface MonthlyAmount {
  year: number
  month: number
  income: number
  expense: number
}

export interface CategoryAmount {
  categoryName: string
  amount: number
}

export interface IncomeExpenseReport {
  totalAmount: number
  monthlySeries: MonthlyAmount[]
  categories: CategoryAmount[]
}

export interface CashFlowReport {
  totalIncome: number
  totalExpense: number
  netCashFlow: number
  savingsRatePercent: number | null
  monthlySeries: MonthlyAmount[]
}

export interface CategorySummarySubCategory {
  id: string
  name: string
  amount: number
}

export interface CategorySummaryCategory {
  id: string
  name: string
  amount: number
  subCategories: CategorySummarySubCategory[]
}

export interface CategorySummaryReport {
  expenseCategories: CategorySummaryCategory[]
  incomeCategories: CategorySummaryCategory[]
}

export interface AccountSummaryItem {
  accountId: string
  accountName: string
  accountType: string
  periodIncome: number
  periodExpense: number
  periodTransferIn: number
  periodTransferOut: number
  periodNetChange: number
  currentBalance: number
}

export interface AccountSummaryReport {
  accounts: AccountSummaryItem[]
}

export interface MonthlyTrendReport {
  monthlySeries: MonthlyAmount[]
}
