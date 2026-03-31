export function formatPrice(cents: number): string {
  return (cents / 100).toFixed(2);
}

export function decimalToCents(decimal: number): number {
  return Math.round(decimal * 100);
}
