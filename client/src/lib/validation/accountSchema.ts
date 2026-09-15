import { z } from 'zod'

export const accountSchema = z.object({
  name: z.string().min(1, 'Name is required.').max(100),
  accountType: z.enum(['Cash', 'Bank', 'CreditCard', 'Wallet', 'Investment', 'Other']),
  description: z.string().max(500),
  openingBalance: z.number({ message: 'Enter a valid amount.' }),
  currency: z
    .string()
    .min(1, 'Currency is required.')
    .regex(/^[A-Z]{3}$/, 'Use a 3-letter ISO code, e.g. INR.'),
  icon: z.string().max(50),
  displayOrder: z.number().int().min(0),
})

export type AccountFormSchema = z.infer<typeof accountSchema>
