import { MoneyInput } from "@cashregister/components/money-input";
import { formatPrice } from "@cashregister/money";
import type { CartEntry } from "../order";

interface OrderSummaryProps {
  cartEntries: CartEntry[];
  hasOverride: boolean;
  displayTotalCents: number;
  totalOverride: string;
  onTotalOverrideChange: (value: string) => void;
  onDecrease: (articleId: string) => void;
  onRemove: (articleId: string) => void;
}

export function OrderSummary({
  cartEntries,
  hasOverride,
  displayTotalCents,
  totalOverride,
  onTotalOverrideChange,
  onDecrease,
  onRemove,
}: OrderSummaryProps) {
  return (
    <div className="flex-1 overflow-auto p-4">
      <h2 className="mb-3 font-semibold">Order Summary</h2>
      {cartEntries.length === 0 ? (
        <p className="text-sm italic text-gray-500">No items yet.</p>
      ) : (
        <>
          {cartEntries.map((entry) => (
            <div
              key={entry.article.id}
              className="flex items-center justify-between border-b py-1 text-sm"
            >
              <span>
                {entry.article.description} × {entry.quantity}
              </span>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  aria-label={`Decrease ${entry.article.description}`}
                  onClick={() => onDecrease(entry.article.id)}
                  className="cursor-pointer text-gray-400 hover:text-blue-600"
                >
                  −
                </button>
                <span>
                  {formatPrice(entry.article.priceInCents * entry.quantity)}
                </span>
                <button
                  type="button"
                  aria-label={`Remove ${entry.article.description}`}
                  onClick={() => onRemove(entry.article.id)}
                  className="cursor-pointer text-gray-400 hover:text-red-600"
                >
                  ✕
                </button>
              </div>
            </div>
          ))}
          <div className="mt-3 flex justify-between pt-3 font-semibold">
            <span>Total</span>
            <span className={hasOverride ? "italic" : ""}>
              {formatPrice(displayTotalCents)}
            </span>
          </div>
          <MoneyInput
            id="totalOverrideInput"
            name="totalOverrideInCents"
            label="Custom total"
            placeholder="Override total..."
            value={totalOverride}
            onValueChange={onTotalOverrideChange}
            className="mt-2 flex flex-col gap-1"
          />
        </>
      )}
    </div>
  );
}
