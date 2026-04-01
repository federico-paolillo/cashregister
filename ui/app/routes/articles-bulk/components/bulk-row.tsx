import { useId } from "react";

interface BulkRowProps {
  onRemove: () => void;
  canRemove: boolean;
}

export function BulkRow({ onRemove, canRemove }: BulkRowProps) {
  const id = useId();
  const descriptionId = `${id}-description`;
  const priceId = `${id}-price`;

  return (
    <div className="flex gap-4 items-end rounded border border-gray-200 p-4">
      <div className="flex flex-col gap-1 flex-1">
        <label htmlFor={descriptionId} className="text-sm font-medium text-gray-700">Description</label>
        <input
          id={descriptionId}
          autoFocus
          name="description"
          type="text"
          required
          className="input-field"
        />
      </div>
      <div className="flex flex-col gap-1 w-36">
        <label htmlFor={priceId} className="text-sm font-medium text-gray-700">
          Price (cents)
        </label>
        <input
          id={priceId}
          name="priceInCents"
          type="number"
          min={0}
          required
          defaultValue={0}
          className="input-field"
        />
      </div>
      {canRemove && (
        <button
          type="button"
          onClick={onRemove}
          className="rounded border border-red-300 px-3 py-2 text-sm text-red-600 hover:bg-red-50"
        >
          Remove
        </button>
      )}
    </div>
  );
}
