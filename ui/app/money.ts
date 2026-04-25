export function formatPrice(cents: number): string {
  if (!Number.isSafeInteger(cents) || cents < 0) {
    throw new RangeError("cents must be a non-negative safe integer");
  }

  const serializedCents = String(cents).padStart(3, "0");
  const integerPart = serializedCents.slice(0, -2);
  const decimalPart = serializedCents.slice(-2);
  const normalizedIntegerPart = integerPart.replace(/^0+(?=\d)/, "");

  return `${normalizedIntegerPart}.${decimalPart}`;
}

export function decimalToCents(input: string): number | null {
  const normalizedInput = input.trim();

  if (!/^\d+(?:\.\d{1,2})?$/.test(normalizedInput)) {
    return null;
  }

  const [integerPart, decimalPart = ""] = normalizedInput.split(".");
  const cents = Number(`${integerPart}${decimalPart.padEnd(2, "0")}`);

  return Number.isSafeInteger(cents) ? cents : null;
}
