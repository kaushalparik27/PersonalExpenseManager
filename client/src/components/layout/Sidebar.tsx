import {
  ArrowLeftRight,
  ChartColumn,
  LayoutDashboard,
  LogOut,
  Repeat,
  Settings,
  Tags,
  Wallet,
  Wallet2,
} from 'lucide-react'
import { NavLink, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { cn } from 'cn'
import { useAuth } from '@/app/providers/AuthProvider'
import { Button } from '@/components/ui/button'

interface NavItem {
  to: string
  label: string
  icon: typeof LayoutDashboard
  end?: boolean
}

const navItems: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/transactions', label: 'Transactions', icon: ArrowLeftRight },
  { to: '/accounts', label: 'Accounts', icon: Wallet },
  { to: '/categories', label: 'Categories', icon: Tags },
  { to: '/reports', label: 'Reports', icon: ChartColumn },
  { to: '/recurring', label: 'Recurring', icon: Repeat },
  { to: '/settings', label: 'Settings', icon: Settings },
]

export function Sidebar() {
  const { currentUser, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    try {
      await logout()
      navigate('/login', { replace: true })
    } catch {
      toast.error('Unable to sign out. Please try again.')
    }
  }

  return (
    <aside className="hidden w-60 shrink-0 border-r border-border bg-sidebar text-sidebar-foreground md:flex md:flex-col">
      <div className="flex h-14 items-center gap-2 border-b border-sidebar-border px-5">
        <Wallet2 className="size-5 text-sidebar-primary" />
        <span className="text-sm font-semibold tracking-tight">Expense Manager</span>
      </div>
      <nav className="flex flex-1 flex-col gap-1 p-3">
        {navItems.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-sidebar-foreground/70 hover:bg-sidebar-accent/60 hover:text-sidebar-accent-foreground',
              )
            }
          >
            <Icon className="size-4" />
            {label}
          </NavLink>
        ))}
      </nav>
      <div className="flex items-center gap-2.5 border-t border-sidebar-border p-3">
        <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-sidebar-accent text-xs font-semibold text-sidebar-accent-foreground">
          {currentUser?.firstName?.[0]}
          {currentUser?.lastName?.[0]}
        </div>
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium">
            {currentUser?.firstName} {currentUser?.lastName}
          </p>
          <p className="truncate text-xs text-sidebar-foreground/60">{currentUser?.email}</p>
        </div>
        <Button variant="ghost" size="icon" className="size-8 shrink-0" onClick={handleLogout} aria-label="Log out">
          <LogOut className="size-4" />
        </Button>
      </div>
    </aside>
  )
}
