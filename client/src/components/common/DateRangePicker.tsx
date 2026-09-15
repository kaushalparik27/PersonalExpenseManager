import { CalendarRange } from 'lucide-react'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  computeDateRange,
  DATE_RANGE_PRESET_OPTIONS,
  formatDateRangeLabel,
  type DateRange,
  type DateRangePreset,
} from '@/lib/dateRangePresets'

interface DateRangePickerProps {
  preset: DateRangePreset
  range: DateRange
  onChange: (preset: DateRangePreset, range: DateRange) => void
}

export function DateRangePicker({ preset, range, onChange }: DateRangePickerProps) {
  function handlePresetChange(value: string) {
    const nextPreset = value as DateRangePreset
    onChange(nextPreset, computeDateRange(nextPreset, range))
  }

  function handleCustomChange(patch: Partial<DateRange>) {
    onChange('custom', { ...range, ...patch })
  }

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Select value={preset} onValueChange={handlePresetChange}>
        <SelectTrigger className="w-44">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {DATE_RANGE_PRESET_OPTIONS.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {preset === 'custom' && (
        <div className="flex items-center gap-2">
          <Input
            type="date"
            value={range.fromDate}
            onChange={(e) => handleCustomChange({ fromDate: e.target.value })}
            className="w-40"
          />
          <span className="text-sm text-muted-foreground">to</span>
          <Input
            type="date"
            value={range.toDate}
            onChange={(e) => handleCustomChange({ toDate: e.target.value })}
            className="w-40"
          />
        </div>
      )}

      <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
        <CalendarRange className="size-4" />
        {formatDateRangeLabel(range)}
      </div>
    </div>
  )
}
