import { LayoutDashboard } from 'lucide-react'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'

export function DashboardPage() {
  return (
    <PlaceholderPage
      icon={LayoutDashboard}
      title="Dashboard"
      description="Your financial overview will appear here once accounts and transactions exist."
    />
  )
}
