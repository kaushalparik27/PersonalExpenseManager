// Remembers which summary periods the user has chosen to display in the
// dashboard's Summary section (1-6 of the 9 available). Per-viewer
// convenience only — never anything sensitive — so localStorage is fine.
import type { SummaryPeriod } from '@/features/dashboard/types'

const STORAGE_KEY = 'pem:dashboard-summary-periods'
const MIN_PERIODS = 1
const MAX_PERIODS = 6

export const SUMMARY_PERIOD_OPTIONS: { value: SummaryPeriod; label: string }[] = [
  { value: 'Today', label: 'Today' },
  { value: 'Yesterday', label: 'Yesterday' },
  { value: 'Last7Days', label: 'Last 7 Days' },
  { value: 'ThisMonth', label: 'This Month' },
  { value: 'Last31Days', label: 'Last 31 Days' },
  { value: 'LastMonth', label: 'Last Month' },
  { value: 'ThisYear', label: 'This Year' },
  { value: 'LastYear', label: 'Last Year' },
  { value: 'Total', label: 'Total' },
]

const VALID_PERIODS = new Set(SUMMARY_PERIOD_OPTIONS.map((o) => o.value))
const DEFAULT_PERIODS: SummaryPeriod[] = ['Today', 'ThisMonth', 'ThisYear']

export { MIN_PERIODS, MAX_PERIODS }

function isSummaryPeriod(value: unknown): value is SummaryPeriod {
  return typeof value === 'string' && VALID_PERIODS.has(value as SummaryPeriod)
}

export function getSummaryPeriods(): SummaryPeriod[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return DEFAULT_PERIODS

    const parsed: unknown = JSON.parse(raw)
    if (!Array.isArray(parsed)) return DEFAULT_PERIODS

    const valid = parsed.filter(isSummaryPeriod)
    if (valid.length < MIN_PERIODS || valid.length > MAX_PERIODS) return DEFAULT_PERIODS

    return valid
  } catch {
    return DEFAULT_PERIODS
  }
}

export function saveSummaryPeriods(periods: SummaryPeriod[]) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(periods))
  } catch {
    // Ignore — private browsing / storage disabled, etc.
  }
}
