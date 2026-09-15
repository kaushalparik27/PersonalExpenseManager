import {
  endOfMonth,
  endOfQuarter,
  endOfWeek,
  endOfYear,
  format,
  startOfMonth,
  startOfQuarter,
  startOfWeek,
  startOfYear,
  subDays,
  subMonths,
  subYears,
} from 'date-fns'

export type DateRangePreset =
  | 'today'
  | 'yesterday'
  | 'thisWeek'
  | 'lastWeek'
  | 'thisMonth'
  | 'lastMonth'
  | 'thisQuarter'
  | 'thisYear'
  | 'lastYear'
  | 'custom'

export interface DateRange {
  fromDate: string
  toDate: string
}

export const DATE_RANGE_PRESET_OPTIONS: { value: DateRangePreset; label: string }[] = [
  { value: 'today', label: 'Today' },
  { value: 'yesterday', label: 'Yesterday' },
  { value: 'thisWeek', label: 'This Week' },
  { value: 'lastWeek', label: 'Last Week' },
  { value: 'thisMonth', label: 'This Month' },
  { value: 'lastMonth', label: 'Last Month' },
  { value: 'thisQuarter', label: 'This Quarter' },
  { value: 'thisYear', label: 'This Year' },
  { value: 'lastYear', label: 'Last Year' },
  { value: 'custom', label: 'Custom Range' },
]

function fmt(date: Date): string {
  return format(date, 'yyyy-MM-dd')
}

export function computeDateRange(preset: DateRangePreset, custom?: DateRange): DateRange {
  const today = new Date()

  switch (preset) {
    case 'today':
      return { fromDate: fmt(today), toDate: fmt(today) }
    case 'yesterday': {
      const y = subDays(today, 1)
      return { fromDate: fmt(y), toDate: fmt(y) }
    }
    case 'thisWeek':
      return { fromDate: fmt(startOfWeek(today)), toDate: fmt(endOfWeek(today)) }
    case 'lastWeek': {
      const lastWeek = subDays(today, 7)
      return { fromDate: fmt(startOfWeek(lastWeek)), toDate: fmt(endOfWeek(lastWeek)) }
    }
    case 'thisMonth':
      return { fromDate: fmt(startOfMonth(today)), toDate: fmt(endOfMonth(today)) }
    case 'lastMonth': {
      const lastMonth = subMonths(today, 1)
      return { fromDate: fmt(startOfMonth(lastMonth)), toDate: fmt(endOfMonth(lastMonth)) }
    }
    case 'thisQuarter':
      return { fromDate: fmt(startOfQuarter(today)), toDate: fmt(endOfQuarter(today)) }
    case 'thisYear':
      return { fromDate: fmt(startOfYear(today)), toDate: fmt(endOfYear(today)) }
    case 'lastYear': {
      const lastYear = subYears(today, 1)
      return { fromDate: fmt(startOfYear(lastYear)), toDate: fmt(endOfYear(lastYear)) }
    }
    case 'custom':
      return custom ?? { fromDate: fmt(startOfMonth(today)), toDate: fmt(endOfMonth(today)) }
  }
}

export function formatDateRangeLabel(range: DateRange): string {
  if (range.fromDate === range.toDate) {
    return format(new Date(range.fromDate), 'd MMM yyyy')
  }
  return `${format(new Date(range.fromDate), 'd MMM yyyy')} – ${format(new Date(range.toDate), 'd MMM yyyy')}`
}
