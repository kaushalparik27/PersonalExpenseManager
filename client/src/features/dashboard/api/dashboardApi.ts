import { apiClient } from '@/lib/api/client'
import type { Dashboard } from '@/features/dashboard/types'

export const dashboardApi = {
  get: () => apiClient.get<Dashboard>('/api/reports/dashboard'),
}
