import { ArrowLeft, Pencil } from 'lucide-react'
import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { NotFoundPage } from '@/components/common/NotFoundPage'
import { accountTypeMeta } from '@/features/accounts/accountTypeMeta'
import { AccountFormDialog } from '@/features/accounts/components/AccountFormDialog'
import { useAccount } from '@/features/accounts/hooks/useAccount'
import { formatCurrency } from '@/lib/utils/currency'

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: account, isLoading } = useAccount(id)
  const [editOpen, setEditOpen] = useState(false)

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-32 rounded-xl" />
      </div>
    )
  }

  if (!account) {
    return <NotFoundPage />
  }

  const { label, icon: Icon } = accountTypeMeta[account.accountType]

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to="/accounts">
          <ArrowLeft className="size-4" />
          Accounts
        </Link>
      </Button>

      <Card>
        <CardContent className="flex items-start gap-4">
          <div className="flex size-12 shrink-0 items-center justify-center rounded-full bg-primary/10">
            <Icon className="size-6 text-primary" />
          </div>
          <div className="min-w-0 flex-1">
            <h1 className="text-lg font-semibold text-foreground">{account.name}</h1>
            <p className="text-sm text-muted-foreground">{label}</p>
            {account.description && <p className="mt-1 text-sm text-muted-foreground">{account.description}</p>}
          </div>
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="size-4" />
            Edit
          </Button>
        </CardContent>
      </Card>

      <div className="grid grid-cols-2 gap-4">
        <Card>
          <CardContent>
            <p className="text-xs text-muted-foreground">Current Balance</p>
            <p className="mt-1 text-2xl font-semibold tabular-nums text-foreground">
              {formatCurrency(account.currentBalance, account.currency)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent>
            <p className="text-xs text-muted-foreground">Opening Balance</p>
            <p className="mt-1 text-2xl font-semibold tabular-nums text-foreground">
              {formatCurrency(account.openingBalance, account.currency)}
            </p>
          </CardContent>
        </Card>
      </div>

      <p className="text-center text-sm text-muted-foreground">
        Transactions for this account will appear here once the transactions feature is built.
      </p>

      <AccountFormDialog open={editOpen} onOpenChange={setEditOpen} account={account} />
    </div>
  )
}
