import { ArrowLeftRight, ChartColumn, LayoutDashboard, Wallet } from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { cn } from 'cn'

interface NavItem {
  to: string
  label: string
  icon: typeof LayoutDashboard
  end?: boolean
}

const navItems: NavItem[] = [
  { to: '/', label: 'Home', icon: LayoutDashboard, end: true },
  { to: '/transactions', label: 'Transactions', icon: ArrowLeftRight },
  { to: '/accounts', label: 'Accounts', icon: Wallet },
  { to: '/reports', label: 'Reports', icon: ChartColumn },
]

export function BottomNav() {
  return (
    <nav className="fixed inset-x-0 bottom-0 z-40 flex h-16 items-center justify-around border-t border-border bg-card/95 backdrop-blur md:hidden">
      {navItems.map(({ to, label, icon: Icon, end }) => (
        <NavLink
          key={to}
          to={to}
          end={end}
          className={({ isActive }) =>
            cn(
              'flex flex-col items-center gap-1 px-3 py-1.5 text-xs font-medium',
              isActive ? 'text-primary' : 'text-muted-foreground',
            )
          }
        >
          <Icon className="size-5" />
          {label}
        </NavLink>
      ))}
    </nav>
  )
}
