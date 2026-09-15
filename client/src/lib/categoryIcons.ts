import {
  Activity,
  Apple,
  Baby,
  Bandage,
  Banknote,
  Beer,
  Bike,
  BookOpen,
  Briefcase,
  Building2,
  Bus,
  Cake,
  Camera,
  Car,
  ChefHat,
  Clapperboard,
  Coffee,
  Coins,
  Compass,
  CreditCard,
  Dog,
  Droplet,
  Dumbbell,
  Flame,
  Fuel,
  Gamepad2,
  Gem,
  Gift,
  Glasses,
  Globe,
  GraduationCap,
  Hammer,
  HandCoins,
  Headphones,
  HeartPulse,
  Home,
  IceCreamCone,
  IndianRupee,
  Landmark,
  Laptop,
  Lightbulb,
  Luggage,
  MapPin,
  Mic,
  Music,
  PaintRoller,
  PartyPopper,
  PawPrint,
  Pencil,
  Percent,
  PiggyBank,
  Pill,
  Pizza,
  Plane,
  Popcorn,
  Receipt,
  Sandwich,
  Scissors,
  Shield,
  Shirt,
  ShoppingBag,
  ShoppingCart,
  Ship,
  Smartphone,
  Sparkles,
  Stethoscope,
  Store,
  Syringe,
  Tag,
  Ticket,
  Train,
  TrendingUp,
  Trash2,
  Truck,
  Tv,
  Users,
  Utensils,
  UtensilsCrossed,
  Wallet,
  Wifi,
  Wine,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

interface CategoryIconEntry {
  name: string
  icon: LucideIcon
  keywords: string[]
}

// Category icons are stored as this registry's string keys (Category.Icon in the
// DB), not raw component references — a curated, keyword-searchable set rather
// than exposing all of lucide-react (which would blow up the bundle since a
// dynamic name lookup across the full namespace defeats tree-shaking), so old
// data never points at a name that got removed from the library.
export const CATEGORY_ICON_ENTRIES: CategoryIconEntry[] = [
  // Food & drink
  { name: 'Utensils', icon: Utensils, keywords: ['food', 'restaurant', 'dining', 'eat'] },
  { name: 'UtensilsCrossed', icon: UtensilsCrossed, keywords: ['food', 'restaurant', 'dining'] },
  { name: 'ChefHat', icon: ChefHat, keywords: ['food', 'cooking', 'chef', 'kitchen'] },
  { name: 'Coffee', icon: Coffee, keywords: ['cafe', 'drink', 'tea'] },
  { name: 'Beer', icon: Beer, keywords: ['drink', 'bar', 'alcohol'] },
  { name: 'Wine', icon: Wine, keywords: ['drink', 'bar', 'alcohol'] },
  { name: 'Pizza', icon: Pizza, keywords: ['food', 'takeout'] },
  { name: 'Sandwich', icon: Sandwich, keywords: ['food', 'lunch'] },
  { name: 'IceCreamCone', icon: IceCreamCone, keywords: ['food', 'dessert', 'sweet'] },
  { name: 'Apple', icon: Apple, keywords: ['food', 'fruit', 'grocery', 'health'] },
  { name: 'ShoppingCart', icon: ShoppingCart, keywords: ['grocery', 'shopping', 'store'] },
  { name: 'ShoppingBag', icon: ShoppingBag, keywords: ['shopping', 'retail'] },
  { name: 'Store', icon: Store, keywords: ['shop', 'retail', 'market'] },

  // Transport
  { name: 'Car', icon: Car, keywords: ['transport', 'drive', 'vehicle'] },
  { name: 'Bus', icon: Bus, keywords: ['transport', 'public transport'] },
  { name: 'Train', icon: Train, keywords: ['transport', 'rail', 'commute'] },
  { name: 'Bike', icon: Bike, keywords: ['transport', 'cycle'] },
  { name: 'Fuel', icon: Fuel, keywords: ['gas', 'petrol', 'diesel', 'transport'] },
  { name: 'Truck', icon: Truck, keywords: ['transport', 'delivery', 'moving'] },
  { name: 'Ship', icon: Ship, keywords: ['transport', 'ferry', 'cruise'] },
  { name: 'Plane', icon: Plane, keywords: ['travel', 'flight', 'airport'] },
  { name: 'Luggage', icon: Luggage, keywords: ['travel', 'trip', 'vacation'] },
  { name: 'MapPin', icon: MapPin, keywords: ['travel', 'location', 'trip'] },
  { name: 'Compass', icon: Compass, keywords: ['travel', 'explore', 'adventure'] },
  { name: 'Globe', icon: Globe, keywords: ['travel', 'world', 'international'] },

  // Home & utilities
  { name: 'Home', icon: Home, keywords: ['house', 'rent', 'mortgage'] },
  { name: 'Building2', icon: Building2, keywords: ['office', 'apartment', 'work'] },
  { name: 'Lightbulb', icon: Lightbulb, keywords: ['electricity', 'power', 'utilities'] },
  { name: 'Flame', icon: Flame, keywords: ['gas', 'heating', 'utilities'] },
  { name: 'Droplet', icon: Droplet, keywords: ['water', 'utilities'] },
  { name: 'Wifi', icon: Wifi, keywords: ['internet', 'broadband', 'utilities'] },
  { name: 'Smartphone', icon: Smartphone, keywords: ['phone', 'mobile', 'recharge'] },
  { name: 'Tv', icon: Tv, keywords: ['cable', 'streaming', 'subscription'] },
  { name: 'Laptop', icon: Laptop, keywords: ['computer', 'tech', 'electronics'] },
  { name: 'Wrench', icon: Wrench, keywords: ['repair', 'maintenance', 'tools'] },
  { name: 'Hammer', icon: Hammer, keywords: ['repair', 'renovation', 'diy'] },
  { name: 'PaintRoller', icon: PaintRoller, keywords: ['renovation', 'painting', 'diy'] },
  { name: 'Trash2', icon: Trash2, keywords: ['waste', 'garbage', 'utilities'] },

  // Health & fitness
  { name: 'HeartPulse', icon: HeartPulse, keywords: ['health', 'medical', 'doctor'] },
  { name: 'Stethoscope', icon: Stethoscope, keywords: ['health', 'medical', 'doctor'] },
  { name: 'Pill', icon: Pill, keywords: ['medicine', 'pharmacy', 'health'] },
  { name: 'Syringe', icon: Syringe, keywords: ['medical', 'vaccine', 'health'] },
  { name: 'Bandage', icon: Bandage, keywords: ['medical', 'first aid', 'health'] },
  { name: 'Dumbbell', icon: Dumbbell, keywords: ['gym', 'fitness', 'workout'] },
  { name: 'Activity', icon: Activity, keywords: ['fitness', 'health', 'exercise'] },
  { name: 'Glasses', icon: Glasses, keywords: ['eyewear', 'health', 'optical'] },

  // Entertainment & leisure
  { name: 'Clapperboard', icon: Clapperboard, keywords: ['movie', 'cinema', 'entertainment', 'film'] },
  { name: 'Music', icon: Music, keywords: ['entertainment', 'concert', 'streaming'] },
  { name: 'Headphones', icon: Headphones, keywords: ['music', 'audio', 'entertainment'] },
  { name: 'Mic', icon: Mic, keywords: ['karaoke', 'entertainment'] },
  { name: 'Gamepad2', icon: Gamepad2, keywords: ['gaming', 'entertainment'] },
  { name: 'Popcorn', icon: Popcorn, keywords: ['movie', 'cinema', 'snacks'] },
  { name: 'Ticket', icon: Ticket, keywords: ['event', 'show', 'entertainment'] },
  { name: 'Camera', icon: Camera, keywords: ['photography', 'hobby'] },
  { name: 'PartyPopper', icon: PartyPopper, keywords: ['party', 'celebration', 'event'] },
  { name: 'Cake', icon: Cake, keywords: ['birthday', 'celebration', 'party'] },

  // Shopping & personal
  { name: 'Shirt', icon: Shirt, keywords: ['clothing', 'apparel', 'fashion'] },
  { name: 'Scissors', icon: Scissors, keywords: ['salon', 'haircut', 'grooming'] },
  { name: 'Gem', icon: Gem, keywords: ['jewelry', 'luxury'] },
  { name: 'Sparkles', icon: Sparkles, keywords: ['beauty', 'cosmetics', 'other'] },
  { name: 'Gift', icon: Gift, keywords: ['present', 'donation'] },

  // Family & pets
  { name: 'Baby', icon: Baby, keywords: ['childcare', 'kids', 'family'] },
  { name: 'Users', icon: Users, keywords: ['family', 'friends', 'social'] },
  { name: 'PawPrint', icon: PawPrint, keywords: ['pet', 'dog', 'cat', 'animal'] },
  { name: 'Dog', icon: Dog, keywords: ['pet', 'animal'] },

  // Education
  { name: 'GraduationCap', icon: GraduationCap, keywords: ['education', 'school', 'college', 'tuition'] },
  { name: 'BookOpen', icon: BookOpen, keywords: ['books', 'education', 'reading'] },
  { name: 'Pencil', icon: Pencil, keywords: ['stationery', 'supplies', 'education'] },

  // Finance & work
  { name: 'Shield', icon: Shield, keywords: ['insurance', 'protection'] },
  { name: 'TrendingUp', icon: TrendingUp, keywords: ['investment', 'stocks', 'growth'] },
  { name: 'PiggyBank', icon: PiggyBank, keywords: ['savings', 'investment'] },
  { name: 'Landmark', icon: Landmark, keywords: ['bank', 'salary', 'government'] },
  { name: 'HandCoins', icon: HandCoins, keywords: ['freelance', 'income', 'payment'] },
  { name: 'Percent', icon: Percent, keywords: ['interest', 'dividend', 'tax'] },
  { name: 'Receipt', icon: Receipt, keywords: ['bill', 'invoice', 'tax'] },
  { name: 'Wallet', icon: Wallet, keywords: ['money', 'cash', 'other'] },
  { name: 'CreditCard', icon: CreditCard, keywords: ['card', 'payment', 'fees'] },
  { name: 'Banknote', icon: Banknote, keywords: ['cash', 'money', 'salary'] },
  { name: 'Coins', icon: Coins, keywords: ['money', 'change', 'savings'] },
  { name: 'IndianRupee', icon: IndianRupee, keywords: ['money', 'salary', 'income'] },
  { name: 'Briefcase', icon: Briefcase, keywords: ['work', 'business', 'salary'] },

  // Misc
  { name: 'Tag', icon: Tag, keywords: ['other', 'misc', 'general'] },
]

export const CATEGORY_ICONS: Record<string, LucideIcon> = Object.fromEntries(
  CATEGORY_ICON_ENTRIES.map((entry) => [entry.name, entry.icon]),
)

export function resolveCategoryIcon(icon: string | null | undefined): LucideIcon | null {
  if (!icon) return null
  return CATEGORY_ICONS[icon] ?? null
}
