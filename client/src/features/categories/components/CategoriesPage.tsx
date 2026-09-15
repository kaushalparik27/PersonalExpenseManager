import { Plus, Tags } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { PlaceholderPage } from '@/components/common/PlaceholderPage'
import { CategoryNameDialog } from '@/features/categories/components/CategoryNameDialog'
import { CategoryTree } from '@/features/categories/components/CategoryTree'
import { useCategories } from '@/features/categories/hooks/useCategories'
import { useCategoryMutations } from '@/features/categories/hooks/useCategoryMutations'
import type { CategoryKind } from '@/features/categories/types'
import { ApiRequestError } from '@/lib/api/client'

function CategoryKindPanel({ kind }: { kind: CategoryKind }) {
  const { data: categories, isLoading, isError } = useCategories(kind)
  const { createCategory } = useCategoryMutations(kind)
  const [addOpen, setAddOpen] = useState(false)

  async function handleCreate(values: { name: string }) {
    try {
      await createCategory.mutateAsync({ name: values.name, icon: null })
      toast.success('Category created.')
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to create category.')
      throw error
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button size="sm" onClick={() => setAddOpen(true)}>
          <Plus className="size-4" />
          Add Category
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-11 rounded-lg" />
          ))}
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Unable to load categories. Please try again.</p>}

      {categories && categories.length === 0 && (
        <PlaceholderPage
          icon={Tags}
          title="No categories yet"
          description="Add a category to start organizing your transactions."
        />
      )}

      {categories && categories.length > 0 && <CategoryTree kind={kind} categories={categories} />}

      <CategoryNameDialog
        open={addOpen}
        onOpenChange={setAddOpen}
        title="Add Category"
        onSubmit={handleCreate}
      />
    </div>
  )
}

export function CategoriesPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-lg font-semibold text-foreground">Categories</h1>
        <p className="text-sm text-muted-foreground">Organize income and expenses with categories and subcategories.</p>
      </div>

      <Tabs defaultValue="expense">
        <TabsList>
          <TabsTrigger value="expense">Expense</TabsTrigger>
          <TabsTrigger value="income">Income</TabsTrigger>
        </TabsList>
        <TabsContent value="expense" className="mt-4">
          <CategoryKindPanel kind="expense" />
        </TabsContent>
        <TabsContent value="income" className="mt-4">
          <CategoryKindPanel kind="income" />
        </TabsContent>
      </Tabs>
    </div>
  )
}
