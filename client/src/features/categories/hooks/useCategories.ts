import { useQuery } from '@tanstack/react-query'
import { createCategoriesApi } from '@/features/categories/api/categoriesApi'
import type { CategoryKind } from '@/features/categories/types'

export function categoriesQueryKey(kind: CategoryKind) {
  return ['categories', kind] as const
}

export function useCategories(kind: CategoryKind) {
  return useQuery({
    queryKey: categoriesQueryKey(kind),
    queryFn: createCategoriesApi(kind).list,
  })
}
