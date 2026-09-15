import {
  Archive,
  ChevronDown,
  ChevronRight,
  MoreVertical,
  Pencil,
  Plus,
  ArrowUp,
  ArrowDown,
} from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { CategoryNameDialog } from '@/features/categories/components/CategoryNameDialog'
import { useCategoryMutations } from '@/features/categories/hooks/useCategoryMutations'
import type { Category, CategoryKind, SubCategory } from '@/features/categories/types'
import { ApiRequestError } from '@/lib/api/client'
import { resolveCategoryIcon } from '@/lib/categoryIcons'
import { cn } from 'cn'

interface CategoryTreeProps {
  kind: CategoryKind
  categories: Category[]
}

type DialogState =
  | { type: 'edit-category'; category: Category }
  | { type: 'add-subcategory'; category: Category }
  | { type: 'edit-subcategory'; category: Category; subCategory: SubCategory }
  | null

export function CategoryTree({ kind, categories }: CategoryTreeProps) {
  const navigate = useNavigate()
  const mutations = useCategoryMutations(kind)
  const [expanded, setExpanded] = useState<Record<string, boolean>>({})
  const [dialog, setDialog] = useState<DialogState>(null)

  const sorted = [...categories].sort((a, b) => a.displayOrder - b.displayOrder)

  function handleAddTransaction(category: Category, subCategory?: SubCategory) {
    const params = new URLSearchParams({ type: kind, categoryId: category.id })
    if (subCategory) {
      params.set('subCategoryId', subCategory.id)
    }
    navigate(`/transactions/new?${params.toString()}`)
  }

  function isExpanded(categoryId: string) {
    return expanded[categoryId] ?? true
  }

  function toggleExpanded(categoryId: string) {
    setExpanded((prev) => ({ ...prev, [categoryId]: !isExpanded(categoryId) }))
  }

  async function handleArchiveCategory(category: Category) {
    try {
      await mutations.archiveCategory.mutateAsync(category.id)
      toast.success(`${category.name} archived.`)
    } catch {
      toast.error('Unable to archive this category.')
    }
  }

  async function handleArchiveSubCategory(subCategory: SubCategory) {
    try {
      await mutations.archiveSubCategory.mutateAsync(subCategory.id)
      toast.success(`${subCategory.name} archived.`)
    } catch {
      toast.error('Unable to archive this subcategory.')
    }
  }

  async function moveCategory(category: Category, direction: -1 | 1) {
    const index = sorted.findIndex((c) => c.id === category.id)
    const swapWith = sorted[index + direction]
    if (!swapWith) return

    const reordered = [...sorted]
    ;[reordered[index], reordered[index + direction]] = [reordered[index + direction], reordered[index]]

    try {
      await mutations.reorderCategories.mutateAsync(reordered.map((c) => c.id))
    } catch {
      toast.error('Unable to reorder categories.')
    }
  }

  async function moveSubCategory(category: Category, subCategory: SubCategory, direction: -1 | 1) {
    const subs = [...category.subCategories].sort((a, b) => a.displayOrder - b.displayOrder)
    const index = subs.findIndex((s) => s.id === subCategory.id)
    const swapWith = subs[index + direction]
    if (!swapWith) return

    const reordered = [...subs]
    ;[reordered[index], reordered[index + direction]] = [reordered[index + direction], reordered[index]]

    try {
      await mutations.reorderSubCategories.mutateAsync({
        categoryId: category.id,
        orderedIds: reordered.map((s) => s.id),
      })
    } catch {
      toast.error('Unable to reorder subcategories.')
    }
  }

  async function handleDialogSubmit(values: { name: string; icon: string | null; categoryId: string }) {
    if (!dialog) return

    try {
      if (dialog.type === 'edit-category') {
        await mutations.updateCategory.mutateAsync({
          id: dialog.category.id,
          name: values.name,
          icon: values.icon,
          isArchived: dialog.category.isArchived,
        })
        toast.success('Category updated.')
      } else if (dialog.type === 'add-subcategory') {
        await mutations.createSubCategory.mutateAsync({
          categoryId: dialog.category.id,
          name: values.name,
          icon: values.icon,
        })
        toast.success('Subcategory added.')
      } else if (dialog.type === 'edit-subcategory') {
        await mutations.updateSubCategory.mutateAsync({
          id: dialog.subCategory.id,
          name: values.name,
          categoryId: values.categoryId,
          icon: values.icon,
          isArchived: dialog.subCategory.isArchived,
        })
        const moved = values.categoryId !== dialog.category.id
        toast.success(moved ? 'Subcategory moved.' : 'Subcategory updated.')
      }
    } catch (error) {
      toast.error(error instanceof ApiRequestError ? error.message : 'Unable to save.')
      throw error
    }
  }

  return (
    <>
      <div className="divide-y divide-border rounded-lg border border-border">
        {sorted.map((category, categoryIndex) => (
          <div key={category.id}>
            <div className="flex items-center gap-1 px-3 py-2.5">
              <button
                type="button"
                onClick={() => toggleExpanded(category.id)}
                className="flex size-6 shrink-0 items-center justify-center rounded text-muted-foreground hover:bg-accent"
                aria-label={isExpanded(category.id) ? 'Collapse' : 'Expand'}
              >
                {isExpanded(category.id) ? (
                  <ChevronDown className="size-4" />
                ) : (
                  <ChevronRight className="size-4" />
                )}
              </button>

              {(() => {
                const CategoryIcon = resolveCategoryIcon(category.icon)
                return CategoryIcon ? (
                  <CategoryIcon className="size-4 shrink-0 text-muted-foreground" />
                ) : null
              })()}

              <span className={cn('flex-1 text-sm font-medium', category.isArchived && 'text-muted-foreground')}>
                {category.name}
              </span>

              {category.isArchived && (
                <Badge variant="secondary" className="text-xs">
                  Archived
                </Badge>
              )}

              <span className="text-xs text-muted-foreground">{category.subCategories.length}</span>

              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" className="size-7 shrink-0" aria-label="Category actions">
                    <MoreVertical className="size-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  {!category.isArchived && (
                    <DropdownMenuItem onSelect={() => handleAddTransaction(category)}>
                      <Plus className="size-4" />
                      Add Transaction
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem onSelect={() => setDialog({ type: 'edit-category', category })}>
                    <Pencil className="size-4" />
                    Edit
                  </DropdownMenuItem>
                  <DropdownMenuItem onSelect={() => setDialog({ type: 'add-subcategory', category })}>
                    <Plus className="size-4" />
                    Add Subcategory
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem disabled={categoryIndex === 0} onSelect={() => moveCategory(category, -1)}>
                    <ArrowUp className="size-4" />
                    Move Up
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    disabled={categoryIndex === sorted.length - 1}
                    onSelect={() => moveCategory(category, 1)}
                  >
                    <ArrowDown className="size-4" />
                    Move Down
                  </DropdownMenuItem>
                  {!category.isArchived && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem variant="destructive" onSelect={() => handleArchiveCategory(category)}>
                        <Archive className="size-4" />
                        Archive
                      </DropdownMenuItem>
                    </>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>
            </div>

            {isExpanded(category.id) && category.subCategories.length > 0 && (
              <div className="divide-y divide-border/60 bg-muted/30">
                {[...category.subCategories]
                  .sort((a, b) => a.displayOrder - b.displayOrder)
                  .map((subCategory, subIndex, subArray) => (
                    <div key={subCategory.id} className="flex items-center gap-1 py-2 pr-3 pl-11">
                      {(() => {
                        const SubCategoryIcon = resolveCategoryIcon(subCategory.icon)
                        return SubCategoryIcon ? (
                          <SubCategoryIcon className="size-3.5 shrink-0 text-muted-foreground" />
                        ) : null
                      })()}

                      <span
                        className={cn(
                          'flex-1 text-sm',
                          subCategory.isArchived ? 'text-muted-foreground' : 'text-foreground',
                        )}
                      >
                        {subCategory.name}
                      </span>

                      {subCategory.isArchived && (
                        <Badge variant="secondary" className="text-xs">
                          Archived
                        </Badge>
                      )}

                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7 shrink-0"
                            aria-label="Subcategory actions"
                          >
                            <MoreVertical className="size-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          {!category.isArchived && !subCategory.isArchived && (
                            <DropdownMenuItem onSelect={() => handleAddTransaction(category, subCategory)}>
                              <Plus className="size-4" />
                              Add Transaction
                            </DropdownMenuItem>
                          )}
                          <DropdownMenuItem
                            onSelect={() => setDialog({ type: 'edit-subcategory', category, subCategory })}
                          >
                            <Pencil className="size-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            disabled={subIndex === 0}
                            onSelect={() => moveSubCategory(category, subCategory, -1)}
                          >
                            <ArrowUp className="size-4" />
                            Move Up
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            disabled={subIndex === subArray.length - 1}
                            onSelect={() => moveSubCategory(category, subCategory, 1)}
                          >
                            <ArrowDown className="size-4" />
                            Move Down
                          </DropdownMenuItem>
                          {!subCategory.isArchived && (
                            <>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                variant="destructive"
                                onSelect={() => handleArchiveSubCategory(subCategory)}
                              >
                                <Archive className="size-4" />
                                Archive
                              </DropdownMenuItem>
                            </>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  ))}
              </div>
            )}
          </div>
        ))}
      </div>

      <CategoryNameDialog
        open={dialog !== null}
        onOpenChange={(open) => !open && setDialog(null)}
        title={
          dialog?.type === 'edit-category'
            ? 'Edit Category'
            : dialog?.type === 'add-subcategory'
              ? 'Add Subcategory'
              : 'Edit Subcategory'
        }
        initialName={
          dialog?.type === 'edit-category'
            ? dialog.category.name
            : dialog?.type === 'edit-subcategory'
              ? dialog.subCategory.name
              : ''
        }
        initialIcon={
          dialog?.type === 'edit-category'
            ? dialog.category.icon
            : dialog?.type === 'edit-subcategory'
              ? dialog.subCategory.icon
              : null
        }
        showIcon
        initialCategoryId={dialog?.type === 'edit-subcategory' ? dialog.category.id : undefined}
        showCategorySelect={dialog?.type === 'edit-subcategory'}
        categoryOptions={sorted.filter((c) => !c.isArchived).map((c) => ({ value: c.id, label: c.name }))}
        onSubmit={handleDialogSubmit}
      />
    </>
  )
}
