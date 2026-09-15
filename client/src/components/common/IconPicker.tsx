import { Tag, X } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { CATEGORY_ICON_ENTRIES, CATEGORY_ICONS } from '@/lib/categoryIcons'
import { cn } from 'cn'

interface IconPickerProps {
  value: string | null
  onChange: (icon: string | null) => void
}

export function IconPicker({ value, onChange }: IconPickerProps) {
  const [search, setSearch] = useState('')
  const SelectedIcon = value ? CATEGORY_ICONS[value] : undefined

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase()
    if (!query) return CATEGORY_ICON_ENTRIES

    return CATEGORY_ICON_ENTRIES.filter(
      (entry) =>
        entry.name.toLowerCase().includes(query) || entry.keywords.some((keyword) => keyword.includes(query)),
    )
  }, [search])

  return (
    <Popover onOpenChange={(open) => !open && setSearch('')}>
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
      <PopoverContent className="w-72" align="start">
        <div className="mb-2 flex items-center justify-between gap-2">
          <Input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search icons…"
            className="h-8"
            autoFocus
          />
          {value && (
            <button
              type="button"
              onClick={() => onChange(null)}
              className="flex shrink-0 items-center gap-0.5 text-xs text-muted-foreground hover:text-foreground"
            >
              <X className="size-3" />
              Clear
            </button>
          )}
        </div>
        {filtered.length === 0 ? (
          <p className="py-4 text-center text-sm text-muted-foreground">No icons match "{search}".</p>
        ) : (
          <div className="grid max-h-56 grid-cols-6 gap-1 overflow-y-auto">
            {filtered.map(({ name, icon: Icon }) => {
              const isSelected = value === name
              return (
                <button
                  key={name}
                  type="button"
                  onClick={() => onChange(name)}
                  aria-label={name}
                  aria-pressed={isSelected}
                  title={name}
                  className={cn(
                    'flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground',
                    isSelected &&
                      'bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground',
                  )}
                >
                  <Icon className="size-4" />
                </button>
              )
            })}
          </div>
        )}
      </PopoverContent>
    </Popover>
  )
}
