import { Tag, X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { CATEGORY_ICON_NAMES, CATEGORY_ICONS } from '@/lib/categoryIcons'
import { cn } from 'cn'

interface IconPickerProps {
  value: string | null
  onChange: (icon: string | null) => void
}

export function IconPicker({ value, onChange }: IconPickerProps) {
  const SelectedIcon = value ? CATEGORY_ICONS[value] : undefined

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          type="button"
          variant="outline"
          size="icon"
          className="size-10 shrink-0"
          aria-label={value ? `Icon: ${value}` : 'Choose an icon'}
        >
          {SelectedIcon ? <SelectedIcon className="size-5" /> : <Tag className="size-5 text-muted-foreground" />}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-64" align="start">
        <div className="mb-2 flex items-center justify-between">
          <p className="text-xs font-medium text-muted-foreground">Choose an icon</p>
          {value && (
            <button
              type="button"
              onClick={() => onChange(null)}
              className="flex items-center gap-0.5 text-xs text-muted-foreground hover:text-foreground"
            >
              <X className="size-3" />
              Clear
            </button>
          )}
        </div>
        <div className="grid grid-cols-6 gap-1">
          {CATEGORY_ICON_NAMES.map((name) => {
            const Icon = CATEGORY_ICONS[name]
            const isSelected = value === name
            return (
              <button
                key={name}
                type="button"
                onClick={() => onChange(name)}
                aria-label={name}
                aria-pressed={isSelected}
                className={cn(
                  'flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground',
                  isSelected && 'bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground',
                )}
              >
                <Icon className="size-4" />
              </button>
            )
          })}
        </div>
      </PopoverContent>
    </Popover>
  )
}
