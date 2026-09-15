export type AccountType = 'Cash' | 'Bank' | 'CreditCard' | 'Wallet' | 'Investment' | 'Other'

export interface Account {
  id: string
  name: string
  accountType: AccountType
  description: string | null
  openingBalance: number
  currentBalance: number
  currency: string
  isActive: boolean
  displayOrder: number
  icon: string | null
  createdAt: string
  updatedAt: string
}

export interface AccountFormValues {
  name: string
  accountType: AccountType
  description: string
  openingBalance: number
  currency: string
  icon: string
  displayOrder: number
}

export interface UpdateAccountPayload extends AccountFormValues {
  isActive: boolean
}
