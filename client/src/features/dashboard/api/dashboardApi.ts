import { apiClient } from '@/lib/api/client'
import type { BalanceTrendGranularity, BalanceTrendPoint, Dashboard, PeriodSummary } from '@/features/dashboard/types'

export const dashboardApi = {
  get: () => apiClient.get<Dashboard>('/api/reports/dashboard'),
  balanceTrend: (granularity: BalanceTrendGranularity) =>
    apiClient.get<BalanceTrendPoint[]>(`/api/reports/balance-trend?granularity=${granularity}`),
  summary: () => apiClient.get<PeriodSummary[]>('/api/reports/summary'),
}
