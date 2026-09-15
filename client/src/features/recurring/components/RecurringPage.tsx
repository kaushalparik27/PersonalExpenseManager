import { Repeat } from 'lucide-react'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'

export function RecurringPage() {
  return (
    <PlaceholderPage
      icon={Repeat}
      title="Recurring Transactions"
      description="Salary, rent, subscriptions, and other recurring transactions will be managed here."
    />
  )
}
