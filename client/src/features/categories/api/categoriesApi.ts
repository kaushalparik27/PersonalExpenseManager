import { apiClient } from '@/lib/api/client'
import type { Category, CategoryKind, SubCategory } from '@/features/categories/types'

interface CreateCategoryPayload {
  name: string
  icon: string | null
}

interface UpdateCategoryPayload {
  name: string
  icon: string | null
  isArchived: boolean
}

interface CreateSubCategoryPayload {
  name: string
  icon: string | null
}

interface UpdateSubCategoryPayload {
  name: string
  categoryId: string
  icon: string | null
  isArchived: boolean
}

interface ReorderPayload {
  orderedIds: string[]
}

function basePathFor(kind: CategoryKind) {
  return kind === 'expense' ? '/api/expense-categories' : '/api/income-categories'
}

// Expense and income categories are separate resources on the backend but
// share an identical shape and operation set — one parameterized client
// avoids duplicating the same eight functions twice.
export function createCategoriesApi(kind: CategoryKind) {
  const base = basePathFor(kind)

  return {
    list: () => apiClient.get<Category[]>(base),

    createCategory: (payload: CreateCategoryPayload) => apiClient.post<Category>(base, payload),

    updateCategory: (id: string, payload: UpdateCategoryPayload) =>
      apiClient.put<Category>(`${base}/${id}`, payload),

    archiveCategory: (id: string) => apiClient.patch<void>(`${base}/${id}/archive`),

    reorderCategories: (orderedIds: string[]) =>
      apiClient.patch<void>(`${base}/reorder`, { orderedIds } satisfies ReorderPayload),

    createSubCategory: (categoryId: string, payload: CreateSubCategoryPayload) =>
      apiClient.post<SubCategory>(`${base}/${categoryId}/subcategories`, payload),

    updateSubCategory: (id: string, payload: UpdateSubCategoryPayload) =>
      apiClient.put<SubCategory>(`${base}/subcategories/${id}`, payload),

    archiveSubCategory: (id: string) => apiClient.patch<void>(`${base}/subcategories/${id}/archive`),

    reorderSubCategories: (categoryId: string, orderedIds: string[]) =>
      apiClient.patch<void>(`${base}/${categoryId}/subcategories/reorder`, { orderedIds } satisfies ReorderPayload),
  }
}
