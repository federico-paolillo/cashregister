import { useFetcher } from "react-router";
import { useModalId } from "./use-modal";
import { useEffect, useRef } from "react";

export interface ArticleFormData {
  description: string;
  priceInCents: number;
}

interface ArticleFormProps {
  articleId?: string;
  initialData?: ArticleFormData;
  intent: string;
  onSubmit?: () => void;
}

export function ArticleForm({ articleId, initialData, intent, onSubmit }: ArticleFormProps) {
  const modalId = useModalId();
  const fetcher = useFetcher();

  const pending = fetcher.state !== "idle";
  const idling = fetcher.state === "idle";

  const prevFetcherState = useRef(fetcher.state);

  useEffect(() => {
    if (prevFetcherState.current !== "idle" && idling) {
      onSubmit?.();
    }

    prevFetcherState.current = fetcher.state;
  }, [fetcher.state, onSubmit]);

  return (
    <fetcher.Form method="post" action="/articles">
      <input type="hidden" name="intent" value={intent} />
      {articleId && <input type="hidden" name="articleId" value={articleId} />}
      <div>
        <label htmlFor="description">Description</label>
        <input
          id="description"
          name="description"
          type="text"
          defaultValue={initialData?.description ?? ""}
          required
        />
      </div>
      <div>
        <label htmlFor="priceInCents">Price (cents)</label>
        <input
          id="priceInCents"
          name="priceInCents"
          type="number"
          defaultValue={initialData?.priceInCents ?? 0}
          required
          min={0}
        />
      </div>
      <div>
        <button
          type="button"
          disabled={pending}
          command="close"
          commandfor={modalId}
        >
          Cancel
        </button>
        <button type="submit" disabled={pending}>
          Save
        </button>
      </div>
    </fetcher.Form>
  );
}
