import { useErrorMessages, type ErrorMessage } from "./use-error-messages";

export function ErrorMessageList() {
  const { errors, dismissError } = useErrorMessages();

  if (errors.length === 0) {
    return null;
  }

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {errors.map((error) => (
        <ErrorMessageItem
          key={error.id}
          error={error}
          onDismiss={dismissError}
        />
      ))}
    </div>
  );
}

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
