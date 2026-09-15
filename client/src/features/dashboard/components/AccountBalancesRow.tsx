import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { accountTypeMeta } from '@/features/accounts/accountTypeMeta'
import type { Account } from '@/features/accounts/types'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

interface AccountBalancesRowProps {
  accounts: Account[]
}

export function AccountBalancesRow({ accounts }: AccountBalancesRowProps) {
  const active = accounts.filter((a) => a.isActive)

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <CardTitle className="text-base">Account Balances</CardTitle>
        <Link to="/accounts" className="text-xs font-medium text-primary hover:underline">
          Manage accounts
        </Link>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {active.map((account) => {
            const { icon: Icon, label } = accountTypeMeta[account.accountType]
            return (
              <Link
                key={account.id}
                to={`/accounts/${account.id}`}
                className="flex items-center gap-2.5 rounded-lg border border-border p-3 transition-colors hover:bg-accent/50"
              >
                <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary/10">
                  <Icon className="size-4 text-primary" />
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-xs font-medium text-foreground">{account.name}</p>
                  <p
                    className={cn(
                      'truncate text-sm font-semibold tabular-nums',
                      account.currentBalance < 0 ? 'text-destructive' : 'text-foreground',
                    )}
                  >
                    {formatCurrency(account.currentBalance, account.currency)}
                  </p>
                </div>
                <span className="sr-only">{label}</span>
              </Link>
            )
          })}
        </div>
      </CardContent>
    </Card>
  )
}
