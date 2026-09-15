import { Eye, EyeOff, MoreVertical, Pencil, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { accountTypeMeta } from '@/features/accounts/accountTypeMeta'
import { AccountFormDialog } from '@/features/accounts/components/AccountFormDialog'
import { useArchiveAccount, useDeleteAccount, useUnhideAccount } from '@/features/accounts/hooks/useAccountMutations'
import type { Account } from '@/features/accounts/types'
import { ApiRequestError } from '@/lib/api/client'
import { formatCurrency } from '@/lib/utils/currency'
import { cn } from 'cn'

interface AccountCardProps {
  account: Account
}

export function AccountCard({ account }: AccountCardProps) {
  const [editOpen, setEditOpen] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const archiveAccount = useArchiveAccount()
  const unhideAccount = useUnhideAccount()
  const deleteAccount = useDeleteAccount()

  const { label, icon: Icon } = accountTypeMeta[account.accountType]

  async function handleHide() {
    try {
      await archiveAccount.mutateAsync(account.id)
      toast.success(`${account.name} hidden. It's excluded from balances, reports, and charts.`)
    } catch {
      toast.error('Unable to hide this account.')
    }
  }

  async function handleUnhide() {
    try {
      await unhideAccount.mutateAsync(account.id)
      toast.success(`${account.name} is visible again.`)
    } catch {
      toast.error('Unable to unhide this account.')
    }
  }

  async function handleDelete() {
    try {
      await deleteAccount.mutateAsync(account.id)
      toast.success(`${account.name} deleted.`)
    } catch (error) {
      toast.error(
        error instanceof ApiRequestError
          ? error.message
          : 'Unable to delete this account.',
      )
    } finally {
      setDeleteOpen(false)
    }
  }

  return (
    <>
      <Card className={cn(!account.isActive && 'opacity-60')}>
        <CardContent className="flex items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary/10">
            <Icon className="size-5 text-primary" />
          </div>

          <div className="min-w-0 flex-1">
            <div className="flex items-center gap-2">
              <p className="truncate text-sm font-semibold text-foreground">{account.name}</p>
              {!account.isActive && (
                <Badge variant="secondary" className="text-xs">
                  Hidden
                </Badge>
              )}
            </div>
            <p className="text-xs text-muted-foreground">{label}</p>
            <p
              className={cn(
                'mt-2 text-lg font-semibold tabular-nums',
                account.currentBalance < 0 ? 'text-destructive' : 'text-foreground',
              )}
            >
              {formatCurrency(account.currentBalance, account.currency)}
            </p>
          </div>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="size-8 shrink-0" aria-label="Account actions">
                <MoreVertical className="size-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onSelect={() => setEditOpen(true)}>
                <Pencil className="size-4" />
                Edit
              </DropdownMenuItem>
              {account.isActive ? (
                <DropdownMenuItem onSelect={handleHide}>
                  <EyeOff className="size-4" />
                  Hide
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onSelect={handleUnhide}>
                  <Eye className="size-4" />
                  Unhide
                </DropdownMenuItem>
              )}
              <DropdownMenuItem variant="destructive" onSelect={() => setDeleteOpen(true)}>
                <Trash2 className="size-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </CardContent>
      </Card>

      <AccountFormDialog open={editOpen} onOpenChange={setEditOpen} account={account} />

      <AlertDialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete {account.name}?</AlertDialogTitle>
            <AlertDialogDescription>
              This can't be undone. Accounts with existing transactions can't be deleted — hide them
              instead.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-white hover:bg-destructive/90">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
