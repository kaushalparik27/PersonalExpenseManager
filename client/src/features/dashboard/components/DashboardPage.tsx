import { LayoutDashboard } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { AccountBalancesRow } from '@/features/dashboard/components/AccountBalancesRow'
import { BalanceTrendChart } from '@/features/dashboard/components/BalanceTrendChart'
import { ExpenseByCategoryChart } from '@/features/dashboard/components/ExpenseByCategoryChart'
import { IncomeExpenseChart } from '@/features/dashboard/components/IncomeExpenseChart'
import { InsightsBanner } from '@/features/dashboard/components/InsightsBanner'
import { RecentTransactionsList } from '@/features/dashboard/components/RecentTransactionsList'
import { SummaryCards } from '@/features/dashboard/components/SummaryCards'
import { SummarySection } from '@/features/dashboard/components/SummarySection'
import { useDashboard } from '@/features/dashboard/hooks/useDashboard'

export function DashboardPage() {
  const { data: dashboard, isLoading, isError } = useDashboard()

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-xl" />
          ))}
        </div>
        <Skeleton className="h-80 rounded-xl" />
      </div>
    )
  }

  if (isError || !dashboard) {
    return <p className="text-sm text-destructive">Unable to load your dashboard. Please try again.</p>
  }

  const hasAnyAccounts = dashboard.accounts.length > 0

  if (!hasAnyAccounts) {
    return (
      <PlaceholderPage
        icon={LayoutDashboard}
        title="Welcome — your financial dashboard is ready"
        description="Start by creating your first account, then record an expense or income to see it here."
      />
    )
  }

  return (
    <div className="space-y-6">
      <AccountBalancesRow accounts={dashboard.accounts} />

      <SummaryCards dashboard={dashboard} />

      <SummarySection />

      <InsightsBanner insights={dashboard.insights} />

      <div className="grid gap-6 lg:grid-cols-2">
        <BalanceTrendChart totalBalance={dashboard.totalBalance} />
        <IncomeExpenseChart data={dashboard.incomeVsExpenseTrend} />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <ExpenseByCategoryChart data={dashboard.topExpenseCategories} />
        <RecentTransactionsList transactions={dashboard.recentTransactions} />
      </div>
    </div>
  )
}
