interface OrderMultiplierProps {
  value: string;
  onDigit: (digit: string) => void;
  onClear: () => void;
}

const multiplierDigits = ["1", "2", "3", "4", "5", "6", "7", "8", "9"];

export function OrderMultiplier({
  value,
  onDigit,
  onClear,
}: OrderMultiplierProps) {
  return (
    <section aria-label="Multiplier" className="w-56 shrink-0 border-l pl-4">
      <h2 className="mb-2 text-sm font-semibold">Multiplier</h2>
      <output className="mb-3 flex h-14 items-center justify-end rounded border border-gray-300 px-3 text-lg font-semibold">
        {value === "" ? "No multiplier" : `${value}x`}
      </output>
      <div className="grid grid-cols-3 gap-2">
        {multiplierDigits.map((digit) => (
          <button
            key={digit}
            type="button"
            onClick={() => onDigit(digit)}
            className="h-14 rounded border border-gray-300 text-lg font-semibold hover:bg-gray-50 active:bg-blue-50"
          >
            {digit}
          </button>
        ))}
        <button
          type="button"
          onClick={() => onDigit("0")}
          className="col-span-2 h-14 rounded border border-gray-300 text-lg font-semibold hover:bg-gray-50 active:bg-blue-50"
        >
          0
        </button>
        <button
          type="button"
          aria-label="Clear multiplier"
          onClick={onClear}
          className="h-14 rounded border border-gray-300 text-lg font-semibold hover:bg-gray-50 active:bg-blue-50"
        >
          C
        </button>
      </div>
    </section>
  );
}
