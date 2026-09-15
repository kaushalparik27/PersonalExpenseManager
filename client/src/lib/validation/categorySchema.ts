import { z } from 'zod'

export const categoryNameSchema = z.object({
  name: z.string().min(1, 'Name is required.').max(100),
  icon: z.string().nullable(),
})

export type CategoryNameFormValues = z.infer<typeof categoryNameSchema>
