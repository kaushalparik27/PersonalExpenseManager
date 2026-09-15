// Remembers the last account/category used per transaction type, so the quick
// entry form can default to them (spec: "Remember recently used account/category").
// Per-viewer convenience only — never anything sensitive — so localStorage is fine.
const STORAGE_KEY = 'pem:recent-transaction-selections'

interface RecentSelections {
  accountId?: string
  expenseCategoryId?: string
  incomeCategoryId?: string
}

function read(): RecentSelections {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as RecentSelections) : {}
  } catch {
    return {}
  }
}

function write(values: RecentSelections) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(values))
  } catch {
    // Ignore — private browsing / storage disabled, etc.
  }
}

export function getRecentSelections(): RecentSelections {
  return read()
}

export function rememberSelections(update: Partial<RecentSelections>) {
  write({ ...read(), ...update })
}
