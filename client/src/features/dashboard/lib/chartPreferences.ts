// Remembers the user's chosen chart style/series for the Income vs Expense
// chart so it doesn't reset to the default every time the dashboard loads.
// Per-viewer convenience only — never anything sensitive — so localStorage is fine.
const STORAGE_KEY = 'pem:income-expense-chart-preferences'

export type ChartStyle = 'bar' | 'line' | 'area'
export type ChartSeries = 'both' | 'income' | 'expense'

interface ChartPreferences {
  style: ChartStyle
  series: ChartSeries
}

const DEFAULT_PREFERENCES: ChartPreferences = { style: 'bar', series: 'both' }

function isChartStyle(value: unknown): value is ChartStyle {
  return value === 'bar' || value === 'line' || value === 'area'
}

function isChartSeries(value: unknown): value is ChartSeries {
  return value === 'both' || value === 'income' || value === 'expense'
}

export function getChartPreferences(): ChartPreferences {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return DEFAULT_PREFERENCES

    const parsed = JSON.parse(raw) as Partial<ChartPreferences>
    return {
      style: isChartStyle(parsed.style) ? parsed.style : DEFAULT_PREFERENCES.style,
      series: isChartSeries(parsed.series) ? parsed.series : DEFAULT_PREFERENCES.series,
    }
  } catch {
    return DEFAULT_PREFERENCES
  }
}

export function saveChartPreferences(preferences: ChartPreferences) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(preferences))
  } catch {
    // Ignore — private browsing / storage disabled, etc.
  }
}
