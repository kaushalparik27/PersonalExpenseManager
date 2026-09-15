import { Banknote, CreditCard, Landmark, TrendingUp, Wallet, WalletCards, type LucideIcon } from 'lucide-react'
import type { AccountType } from '@/features/accounts/types'

interface AccountTypeMeta {
  label: string
  icon: LucideIcon
}

export const accountTypeMeta: Record<AccountType, AccountTypeMeta> = {
  Cash: { label: 'Cash', icon: Banknote },
  Bank: { label: 'Bank', icon: Landmark },
  CreditCard: { label: 'Credit Card', icon: CreditCard },
  Wallet: { label: 'Wallet', icon: WalletCards },
  Investment: { label: 'Investment', icon: TrendingUp },
  Other: { label: 'Other', icon: Wallet },
}

export const accountTypeOptions: { value: AccountType; label: string }[] = (
  Object.keys(accountTypeMeta) as AccountType[]
).map((value) => ({ value, label: accountTypeMeta[value].label }))
