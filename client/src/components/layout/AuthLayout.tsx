import { Wallet2 } from 'lucide-react'
import { Outlet } from 'react-router-dom'

export function AuthLayout() {
  return (
    <div className="flex min-h-svh items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm">
        <div className="mb-6 flex items-center justify-center gap-2">
          <Wallet2 className="size-6 text-primary" />
          <span className="text-base font-semibold tracking-tight">Expense Manager</span>
        </div>
        <div className="rounded-lg border border-border bg-card p-6 shadow-sm">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
