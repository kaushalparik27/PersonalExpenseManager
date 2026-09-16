import { useQuery } from '@tanstack/react-query'
import { dashboardApi } from '@/features/dashboard/api/dashboardApi'
import type { BalanceTrendGranularity } from '@/features/dashboard/types'

export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: dashboardApi.get,
  })
}

export function useBalanceTrend(granularity: BalanceTrendGranularity) {
  return useQuery({
    queryKey: ['dashboard', 'balance-trend', granularity],
    queryFn: () => dashboardApi.balanceTrend(granularity),
  })
}

export function useSummary() {
  return useQuery({
    queryKey: ['dashboard', 'summary'],
    queryFn: dashboardApi.summary,
  })
}
