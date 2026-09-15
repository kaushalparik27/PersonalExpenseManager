import {
  Baby,
  Bus,
  Car,
  Coffee,
  Dumbbell,
  Film,
  Fuel,
  Gift,
  GraduationCap,
  HandCoins,
  HeartPulse,
  Home,
  Landmark,
  Music,
  PawPrint,
  Percent,
  Phone,
  PiggyBank,
  Plane,
  Receipt,
  Shield,
  Shirt,
  ShoppingBag,
  ShoppingCart,
  Sparkles,
  Tag,
  TrendingUp,
  Utensils,
  Wifi,
  Zap,
  type LucideIcon,
} from 'lucide-react'

// Category icons are stored as this registry's string keys (Category.Icon in the
// DB), not raw component references — small, curated set rather than exposing all
// of lucide-react, so the picker stays scannable and old data never points at a
// name that got removed from the library.
export const CATEGORY_ICONS: Record<string, LucideIcon> = {
  Utensils,
  Coffee,
  ShoppingCart,
  ShoppingBag,
  Car,
  Bus,
  Fuel,
  Plane,
  Home,
  Zap,
  Wifi,
  Phone,
  HeartPulse,
  Dumbbell,
  Film,
  Music,
  GraduationCap,
  Shield,
  TrendingUp,
  PiggyBank,
  Landmark,
  HandCoins,
  Percent,
  Receipt,
  Gift,
  Baby,
  PawPrint,
  Shirt,
  Sparkles,
  Tag,
}

export const CATEGORY_ICON_NAMES = Object.keys(CATEGORY_ICONS)

export function resolveCategoryIcon(icon: string | null | undefined): LucideIcon | null {
  if (!icon) return null
  return CATEGORY_ICONS[icon] ?? null
}
