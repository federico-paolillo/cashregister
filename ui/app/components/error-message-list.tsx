import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { ErrorMessageItem } from "@cashregister/components/error-message-item";

export function ErrorMessageList() {
  const { errors, dismissError } = useErrorMessages();

  if (errors.length === 0) {
    return null;
  }

  return (
    <div
      role="log"
      aria-live="polite"
      aria-label="Notifications"
      className="fixed bottom-4 left-4 z-50 flex flex-col gap-2"
    >
      {errors.map((error) => (
        <ErrorMessageItem
          key={error.id}
          error={error}
          onDismiss={() => dismissError(error.id)}
        />
      ))}
    </div>
  );
}
