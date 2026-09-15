import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Button } from '@/components/ui/button'
import { Combobox } from '@/components/common/Combobox'
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { IconPicker } from '@/components/common/IconPicker'
import { Input } from '@/components/ui/input'
import { categoryNameSchema, type CategoryNameFormValues } from '@/lib/validation/categorySchema'

interface CategoryNameDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  initialName?: string
  initialIcon?: string | null
  showIcon?: boolean
  initialCategoryId?: string
  showCategorySelect?: boolean
  categoryOptions?: { value: string; label: string }[]
  onSubmit: (values: CategoryNameFormValues) => Promise<void>
}

export function CategoryNameDialog({
  open,
  onOpenChange,
  title,
  initialName,
  initialIcon,
  showIcon,
  initialCategoryId,
  showCategorySelect,
  categoryOptions,
  onSubmit,
}: CategoryNameDialogProps) {
  const form = useForm<CategoryNameFormValues>({
    resolver: zodResolver(categoryNameSchema),
    defaultValues: { name: initialName ?? '', icon: initialIcon ?? null, categoryId: initialCategoryId ?? '' },
  })

  useEffect(() => {
    if (open) {
      form.reset({ name: initialName ?? '', icon: initialIcon ?? null, categoryId: initialCategoryId ?? '' })
    }
  }, [open, initialName, initialIcon, initialCategoryId, form])

  async function handleSubmit(values: CategoryNameFormValues) {
    await onSubmit(values)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            {showIcon ? (
              <div className="flex items-end gap-3">
                <FormField
                  control={form.control}
                  name="icon"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Icon</FormLabel>
                      <IconPicker value={field.value} onChange={field.onChange} />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem className="flex-1">
                      <FormLabel>Name</FormLabel>
                      <FormControl>
                        <Input autoFocus {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            ) : (
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Name</FormLabel>
                    <FormControl>
                      <Input autoFocus {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            {showCategorySelect && (
              <FormField
                control={form.control}
                name="categoryId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Category</FormLabel>
                    <Combobox
                      options={categoryOptions ?? []}
                      value={field.value}
                      onChange={field.onChange}
                      placeholder="Select category"
                      searchPlaceholder="Search categories…"
                    />
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            <DialogFooter>
              <Button type="submit" disabled={form.formState.isSubmitting}>
                {form.formState.isSubmitting && <Loader2 className="size-4 animate-spin" />}
                Save
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
