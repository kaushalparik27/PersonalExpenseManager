import { Plus } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from '@/components/common/ThemeToggle'

export function Topbar() {
  const navigate = useNavigate()

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center justify-end gap-2 border-b border-border bg-background/95 px-4 backdrop-blur md:px-6">
      <ThemeToggle />
      <Button size="sm" onClick={() => navigate('/transactions/new')}>
        <Plus className="size-4" />
        Add Transaction
      </Button>
    </header>
  )
}
