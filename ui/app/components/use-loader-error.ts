import { useEffect } from "react";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import type { Result } from "@cashregister/result";

export function useLoaderError(loaderData: Result<unknown>): void {
  const { addError } = useErrorMessages();
  useEffect(() => {
    if (!loaderData.ok) addError(loaderData.error.message);
  }, [loaderData, addError]);
}
