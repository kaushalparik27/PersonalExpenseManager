import { MoreVertical, Pencil, Archive, Trash2 } from 'lucide-react'
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
import { useArchiveAccount, useDeleteAccount } from '@/features/accounts/hooks/useAccountMutations'
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
  const deleteAccount = useDeleteAccount()

  const { label, icon: Icon } = accountTypeMeta[account.accountType]

  async function handleArchive() {
    try {
      await archiveAccount.mutateAsync(account.id)
      toast.success(`${account.name} archived.`)
    } catch {
      toast.error('Unable to archive this account.')
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
                  Archived
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
              {account.isActive && (
                <DropdownMenuItem onSelect={handleArchive}>
                  <Archive className="size-4" />
                  Archive
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
              This can't be undone. Accounts with existing transactions can't be deleted — archive them
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
