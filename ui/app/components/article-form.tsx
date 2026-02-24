import { useFetcher } from "react-router";
import { useModalId } from "./use-modal";
import { useEffect, useRef } from "react";
import type { Result } from "../result";

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
  }, [fetcher.state, fetcher.data, onSubmit, onError]);

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
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>
      <div className="flex flex-col gap-1">
        <label htmlFor="priceInCents" className="text-sm font-medium text-gray-700">
          Price (cents)
        </label>
        <input
          id="priceInCents"
          name="priceInCents"
          type="number"
          defaultValue={initialData?.priceInCents ?? 0}
          required
          min={0}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>
      <div className="flex justify-end gap-2">
        <button
          type="button"
          disabled={pending}
          command="close"
          commandfor={modalId}
          className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={pending}
          className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Save
        </button>
      </div>
    </fetcher.Form>
  );
}
