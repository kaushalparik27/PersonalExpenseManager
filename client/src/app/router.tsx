import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { AuthLayout } from '@/components/layout/AuthLayout'
import { NotFoundPage } from '@/components/common/NotFoundPage'
import { ProtectedRoute } from '@/components/common/ProtectedRoute'
import { PublicRoute } from '@/components/common/PublicRoute'
import { DashboardPage } from '@/features/dashboard/components/DashboardPage'
import { TransactionsPage } from '@/features/transactions/components/TransactionsPage'
import { NewTransactionPage } from '@/features/transactions/components/NewTransactionPage'
import { AccountsPage } from '@/features/accounts/components/AccountsPage'
import { AccountDetailPage } from '@/features/accounts/components/AccountDetailPage'
import { CategoriesPage } from '@/features/categories/components/CategoriesPage'
import { ReportsPage } from '@/features/reports/components/ReportsPage'
import { RecurringPage } from '@/features/recurring/components/RecurringPage'
import { SettingsPage } from '@/features/settings/components/SettingsPage'
import { LoginPage } from '@/features/auth/components/LoginPage'
import { RegisterPage } from '@/features/auth/components/RegisterPage'
import { ForgotPasswordPage } from '@/features/auth/components/ForgotPasswordPage'
import { ResetPasswordPage } from '@/features/auth/components/ResetPasswordPage'

export const router = createBrowserRouter([
  {
    element: <PublicRoute />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { path: '/login', element: <LoginPage /> },
          { path: '/register', element: <RegisterPage /> },
          { path: '/forgot-password', element: <ForgotPasswordPage /> },
          { path: '/reset-password', element: <ResetPasswordPage /> },
        ],
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: '/', element: <DashboardPage /> },
          { path: '/transactions', element: <TransactionsPage /> },
          { path: '/transactions/new', element: <NewTransactionPage /> },
          { path: '/accounts', element: <AccountsPage /> },
          { path: '/accounts/:id', element: <AccountDetailPage /> },
          { path: '/categories', element: <CategoriesPage /> },
          { path: '/reports', element: <ReportsPage /> },
          { path: '/recurring', element: <RecurringPage /> },
          { path: '/settings', element: <SettingsPage /> },
        ],
      },
    ],
  },
  { path: '*', element: <NotFoundPage /> },
])
