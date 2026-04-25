import { useFetcher } from "react-router";
import { useModalId } from "@cashregister/components/use-modal";
import { useEffect, useRef } from "react";
import { MoneyInput } from "@cashregister/components/money-input";
import type { Result } from "@cashregister/result";

export interface ArticleFormData {
  description: string;
  priceInCents: number;
}

interface ArticleFormProps {
  articleId?: string;
  initialData?: ArticleFormData;
  intent: string;
  onSubmit?: () => void;
  onError?: (message: string) => void;
}

export function ArticleForm({ articleId, initialData, intent, onSubmit, onError }: ArticleFormProps) {
  const modalId = useModalId();
  const fetcher = useFetcher<Result<unknown>>();

  const pending = fetcher.state !== "idle";
  const idling = fetcher.state === "idle";

  const prevFetcherState = useRef(fetcher.state);

  useEffect(() => {
    if (prevFetcherState.current !== "idle" && idling) {
      if (fetcher.data && !fetcher.data.ok) {
        onError?.(fetcher.data.error.message);
      } else {
        onSubmit?.();
      }
    }

    prevFetcherState.current = fetcher.state;
  }, [fetcher.state, fetcher.data, onSubmit, onError, idling]);

  return (
    <fetcher.Form method="post" action="/articles" className="flex flex-col gap-6 p-6 min-w-80">
      <input type="hidden" name="intent" value={intent} />
      {articleId && <input type="hidden" name="articleId" value={articleId} />}
      <div className="flex flex-col gap-1">
        <label htmlFor="description" className="text-sm font-medium text-gray-700">
          Description
        </label>
        <input
          id="description"
          name="description"
          type="text"
          defaultValue={initialData?.description ?? ""}
          required
          className="input-field"
        />
      </div>
      <MoneyInput
        id="priceInCents"
        name="priceInCents"
        label="Price"
        defaultCents={initialData?.priceInCents ?? 0}
        required
      />
      <div className="flex justify-end gap-2">
        <button
          type="button"
          disabled={pending}
          command="close"
          commandfor={modalId}
          className="btn-secondary"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={pending}
          className="btn-primary"
        >
          Save
        </button>
      </div>
    </fetcher.Form>
  );
}
