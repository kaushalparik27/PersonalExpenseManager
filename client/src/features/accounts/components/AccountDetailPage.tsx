import { Wallet } from 'lucide-react'
import { useParams } from 'react-router-dom'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <PlaceholderPage
      icon={Wallet}
      title="Account Detail"
      description={`Balance history and transactions for account ${id} will appear here.`}
    />
  )
}
