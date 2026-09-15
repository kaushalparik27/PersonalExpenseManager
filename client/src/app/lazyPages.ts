import { lazy } from 'react'

// Route-level code splitting (spec §10) for everything behind auth: these are the
// pages that pull in Recharts and the bulk of the app's feature code, and they're
// never needed for the first (unauthenticated) paint. Kept in their own .ts module
// (not router.tsx) so oxlint's fast-refresh rule doesn't flag component-valued
// consts sharing a file with the non-component `router` export.
export const DashboardPage = lazy(() =>
  import('@/features/dashboard/components/DashboardPage').then((m) => ({ default: m.DashboardPage })),
)
export const TransactionsPage = lazy(() =>
  import('@/features/transactions/components/TransactionsPage').then((m) => ({ default: m.TransactionsPage })),
)
export const NewTransactionPage = lazy(() =>
  import('@/features/transactions/components/NewTransactionPage').then((m) => ({ default: m.NewTransactionPage })),
)
export const AccountsPage = lazy(() =>
  import('@/features/accounts/components/AccountsPage').then((m) => ({ default: m.AccountsPage })),
)
export const AccountDetailPage = lazy(() =>
  import('@/features/accounts/components/AccountDetailPage').then((m) => ({ default: m.AccountDetailPage })),
)
export const CategoriesPage = lazy(() =>
  import('@/features/categories/components/CategoriesPage').then((m) => ({ default: m.CategoriesPage })),
)
export const ReportsPage = lazy(() =>
  import('@/features/reports/components/ReportsPage').then((m) => ({ default: m.ReportsPage })),
)
export const RecurringPage = lazy(() =>
  import('@/features/recurring/components/RecurringPage').then((m) => ({ default: m.RecurringPage })),
)
export const SettingsPage = lazy(() =>
  import('@/features/settings/components/SettingsPage').then((m) => ({ default: m.SettingsPage })),
)
