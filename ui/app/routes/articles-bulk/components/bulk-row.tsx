interface BulkRowProps {
  onRemove: () => void;
  canRemove: boolean;
}

export function BulkRow({ onRemove, canRemove }: BulkRowProps) {
  return (
    <div className="flex gap-4 items-end rounded border border-gray-200 p-4">
      <div className="flex flex-col gap-1 flex-1">
        <label className="text-sm font-medium text-gray-700">Description</label>
        <input
          autoFocus
          name="description"
          type="text"
          required
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>
      <div className="flex flex-col gap-1 w-36">
        <label className="text-sm font-medium text-gray-700">
          Price (cents)
        </label>
        <input
          name="priceInCents"
          type="number"
          min={0}
          required
          defaultValue={0}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
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
