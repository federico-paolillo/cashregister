import { formatPrice } from "@cashregister/money";

export function formatSignedPrice(cents: number): string {
  if (cents < 0) {
    return `-${formatPrice(Math.abs(cents))}`;
  }

  return formatPrice(cents);
}
