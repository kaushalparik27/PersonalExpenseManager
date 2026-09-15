import { Plus, Wallet } from 'lucide-react'
import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { AccountCard } from '@/features/accounts/components/AccountCard'
import { AccountFormDialog } from '@/features/accounts/components/AccountFormDialog'
import { useAccounts } from '@/features/accounts/hooks/useAccounts'

export function AccountsPage() {
  const { data: accounts, isLoading, isError } = useAccounts()
  const [createOpen, setCreateOpen] = useState(false)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-foreground">Accounts</h1>
          <p className="text-sm text-muted-foreground">Bank accounts, cash, credit cards, and more.</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="size-4" />
          Add Account
        </Button>
      </div>

      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-xl" />
          ))}
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">Unable to load accounts. Please try again.</p>
      )}

      {accounts && accounts.length === 0 && (
        <PlaceholderPage
          icon={Wallet}
          title="No accounts yet"
          description="Create your first account to start tracking balances and transactions."
        />
      )}

      {accounts && accounts.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {accounts.map((account) => (
            <AccountCard key={account.id} account={account} />
          ))}
        </div>
      )}

      <AccountFormDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
