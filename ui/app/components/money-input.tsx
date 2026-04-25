import { useId, useState } from "react";
import { decimalToCents, formatPrice } from "@cashregister/money";

export interface MoneyInputProps {
  name: string;
  label: string;
  id?: string;
  defaultCents?: number;
  required?: boolean;
  placeholder?: string;
  value?: string;
  onValueChange?: (value: string) => void;
  className?: string;
}

export function MoneyInput({
  name,
  label,
  id,
  defaultCents,
  required = false,
  placeholder,
  value,
  onValueChange,
  className = "flex flex-col gap-1",
}: MoneyInputProps) {
  const generatedId = useId();
  const inputId = id ?? `${generatedId}-money`;
  const initialValue = defaultCents !== undefined
    ? formatPrice(defaultCents)
    : required
      ? "0.00"
      : "";
  const [internalValue, setInternalValue] = useState(initialValue);
  const currentValue = value ?? internalValue;
  const cents = decimalToCents(currentValue);
  const hasValue = currentValue.trim() !== "";
  const isInvalid = hasValue && cents === null;
  const shouldSubmit = cents !== null && (required || hasValue);

  function updateValue(nextValue: string) {
    if (value === undefined) {
      setInternalValue(nextValue);
    }

    onValueChange?.(nextValue);
  }

  function normalizeValue() {
    if (cents !== null && hasValue) {
      updateValue(formatPrice(cents));
    }
  }

  return (
    <div className={className}>
      <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
        {label}
      </label>
      <input
        id={inputId}
        type="text"
        inputMode="decimal"
        pattern={String.raw`\d+(\.\d{1,2})?`}
        required={required}
        placeholder={placeholder}
        value={currentValue}
        onChange={(e) => updateValue(e.target.value)}
        onBlur={normalizeValue}
        aria-invalid={isInvalid || undefined}
        className="input-field"
      />
      {shouldSubmit && (
        <input
          type="hidden"
          name={name}
          value={String(cents)}
        />
      )}
    </div>
  );
}
