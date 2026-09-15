// Centralized currency formatting so no component hand-rolls its own Intl call.
// Locale is fixed to en-IN (matches the spec's ₹1,25,000-style grouping) since
// INR is the only currency in real use today; revisit if genuine multi-currency
// display (locale-per-currency) becomes a requirement.
export function formatCurrency(amount: number, currencyCode: string = 'INR'): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: currencyCode,
    maximumFractionDigits: 2,
  }).format(amount)
}
