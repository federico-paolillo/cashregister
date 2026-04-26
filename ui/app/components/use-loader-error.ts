import { useEffect } from "react";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import type { Result } from "@cashregister/result";

export function useLoaderError(loaderData: Result<unknown> | null | undefined): void {
  const { addError } = useErrorMessages();

  useEffect(() => {
    if (loaderData && !loaderData.ok) {
      addError(loaderData.error.message);
    }
  }, [loaderData, addError]);
}
