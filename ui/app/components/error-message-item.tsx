import type { ErrorMessage, ErrorMessageSeverity } from "@cashregister/components/use-error-messages";

interface ErrorMessageItemProps {
  error: ErrorMessage;
  onDismiss: () => void;
}

const severityStyles = new Map<ErrorMessageSeverity, string>(
  [
    ["error", "border-red-300 bg-red-50 text-red-800"],
    ["info", "border-green-300 bg-green-50 text-green-800"]
  ]
)

export function ErrorMessageItem({ error, onDismiss }: ErrorMessageItemProps) {
  const colorClasses = severityStyles.get(error.severity);
  return (
    <div className={`flex items-center gap-2 rounded border min-w-64 p-3 ${colorClasses}`}>
      <span className="flex-1">{error.message}</span>
      <button
        type="button"
        aria-label="Dismiss"
        onClick={onDismiss}
        className="ml-2 cursor-pointer font-bold"
      >
        ✕
      </button>
    </div>
  );
}
