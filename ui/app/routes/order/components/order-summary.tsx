import { formatPrice } from "@cashregister/money";
import type { ArticleListItemDto } from "@cashregister/model";

interface CartEntry {
  article: ArticleListItemDto;
  quantity: number;
}

interface OrderSummaryProps {
  cartEntries: CartEntry[];
  hasOverride: boolean;
  displayTotal: number;
  totalOverride: string;
  onTotalOverrideChange: (value: string) => void;
  onDecrease: (articleId: string) => void;
  onRemove: (articleId: string) => void;
}

export function OrderSummary({
  cartEntries,
  hasOverride,
  displayTotal,
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
                  {formatPrice(entry.article.price * entry.quantity)}
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
              {formatPrice(displayTotal)}
            </span>
          </div>
          <div className="mt-2">
            <label
              htmlFor="totalOverrideInput"
              className="block text-xs text-gray-500 mb-1"
            >
              Custom total
            </label>
            <input
              id="totalOverrideInput"
              type="number"
              step="0.01"
              min="0"
              placeholder="Override total..."
              value={totalOverride}
              onChange={(e) => onTotalOverrideChange(e.target.value)}
              className="w-full rounded border border-gray-300 px-2 py-1 text-sm"
            />
          </div>
        </>
      )}
    </div>
  );
}
