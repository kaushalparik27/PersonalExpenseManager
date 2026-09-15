import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createCategoriesApi } from '@/features/categories/api/categoriesApi'
import { categoriesQueryKey } from '@/features/categories/hooks/useCategories'
import type { CategoryKind } from '@/features/categories/types'

export function useCategoryMutations(kind: CategoryKind) {
  const api = createCategoriesApi(kind)
  const queryClient = useQueryClient()
  const invalidate = () => queryClient.invalidateQueries({ queryKey: categoriesQueryKey(kind) })

  const createCategory = useMutation({
    mutationFn: api.createCategory,
    onSuccess: invalidate,
  })

  const updateCategory = useMutation({
    mutationFn: ({ id, ...payload }: { id: string; name: string; icon: string | null; isArchived: boolean }) =>
      api.updateCategory(id, payload),
    onSuccess: invalidate,
  })

  const archiveCategory = useMutation({
    mutationFn: api.archiveCategory,
    onSuccess: invalidate,
  })

  const reorderCategories = useMutation({
    mutationFn: api.reorderCategories,
    onSuccess: invalidate,
  })

  const createSubCategory = useMutation({
    mutationFn: ({ categoryId, name, icon }: { categoryId: string; name: string; icon: string | null }) =>
      api.createSubCategory(categoryId, { name, icon }),
    onSuccess: invalidate,
  })

  const updateSubCategory = useMutation({
    mutationFn: ({
      id,
      ...payload
    }: {
      id: string
      name: string
      categoryId: string
      icon: string | null
      isArchived: boolean
    }) => api.updateSubCategory(id, payload),
    onSuccess: invalidate,
  })

  const archiveSubCategory = useMutation({
    mutationFn: api.archiveSubCategory,
    onSuccess: invalidate,
  })

  const reorderSubCategories = useMutation({
    mutationFn: ({ categoryId, orderedIds }: { categoryId: string; orderedIds: string[] }) =>
      api.reorderSubCategories(categoryId, orderedIds),
    onSuccess: invalidate,
  })

  return {
    createCategory,
    updateCategory,
    archiveCategory,
    reorderCategories,
    createSubCategory,
    updateSubCategory,
    archiveSubCategory,
    reorderSubCategories,
  }
}
