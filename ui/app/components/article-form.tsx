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
  onSubmit?: (data: ArticleFormData) => void;
}

export function ArticleForm({ articleId, initialData, intent, onSubmit }: ArticleFormProps) {
  const modalId = useModalId();
  const fetcher = useFetcher();
  const submittedRef = useRef<ArticleFormData | null>(null);

  const pending = fetcher.state !== "idle";

  useEffect(() => {
    if (fetcher.state === "submitting" && fetcher.formData) {
      submittedRef.current = {
        description: String(fetcher.formData.get("description")),
        priceInCents: Number(fetcher.formData.get("priceInCents")),
      };
    }

    if (fetcher.state === "idle" && fetcher.data && submittedRef.current) {
      onSubmit?.(submittedRef.current);
      submittedRef.current = null;
    }
  }, [fetcher.state, fetcher.data, fetcher.formData, onSubmit]);

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
