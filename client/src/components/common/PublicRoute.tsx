import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/app/providers/AuthProvider'

// Keeps an already-authenticated user off the login/register/forgot-password
// screens instead of showing them the form again.
export function PublicRoute() {
  const { isAuthenticated, isLoading } = useAuth()

  if (!isLoading && isAuthenticated) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
