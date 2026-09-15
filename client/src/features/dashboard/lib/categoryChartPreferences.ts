// Remembers the user's chosen chart style for the Expense by Category chart
// so it doesn't reset to the default every time the dashboard loads.
// Per-viewer convenience only — never anything sensitive — so localStorage is fine.
const STORAGE_KEY = 'pem:expense-by-category-chart-style'

export type CategoryChartStyle = 'donut' | 'pie' | 'bar'

const DEFAULT_STYLE: CategoryChartStyle = 'donut'

function isCategoryChartStyle(value: unknown): value is CategoryChartStyle {
  return value === 'donut' || value === 'pie' || value === 'bar'
}

export function getCategoryChartStyle(): CategoryChartStyle {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return isCategoryChartStyle(raw) ? raw : DEFAULT_STYLE
  } catch {
    return DEFAULT_STYLE
  }
}

export function saveCategoryChartStyle(style: CategoryChartStyle) {
  try {
    localStorage.setItem(STORAGE_KEY, style)
  } catch {
    // Ignore — private browsing / storage disabled, etc.
  }
}
