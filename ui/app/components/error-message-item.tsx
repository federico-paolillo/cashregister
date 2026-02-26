import type { ErrorMessage } from "@cashregister/components/use-error-messages";

interface ErrorMessageItemProps {
  error: ErrorMessage;
  onDismiss: (id: number) => void;
}

export function ErrorMessageItem({ error, onDismiss }: ErrorMessageItemProps) {
  return (
    <div role="alert" className="flex items-center gap-2 rounded border border-red-300 bg-red-50 p-3 text-red-800">
      <span className="flex-1">{error.message}</span>
      <button
        type="button"
        aria-label="Dismiss"
        onClick={() => onDismiss(error.id)}
        className="ml-2 cursor-pointer font-bold"
      >
        ✕
      </button>
    </div>
  );
}
