export type CategoryKind = 'expense' | 'income'

export interface SubCategory {
  id: string
  name: string
  displayOrder: number
  isArchived: boolean
}

export interface Category {
  id: string
  name: string
  icon: string | null
  displayOrder: number
  isArchived: boolean
  subCategories: SubCategory[]
}
