import { useEffect } from "react";
import { useFetcher } from "react-router";
import { useModalId } from "./use-modal";

export interface ArticleFormData {
  description: string;
  priceInCents: number;
}

interface ArticleFormProps {
  initialData?: ArticleFormData;
  intent: string;
}

export function ArticleForm({ initialData, intent }: ArticleFormProps) {
  const modalId = useModalId();
  const fetcher = useFetcher();
  const pending = fetcher.state !== "idle";

  useEffect(() => {
    if (fetcher.state !== "idle") return;

    const data = fetcher.data as { ok: boolean } | undefined;
    if (data?.ok) {
      (document.getElementById(modalId) as HTMLDialogElement | null)?.close();
    }
  }, [fetcher.state, fetcher.data, modalId]);

  return (
    <fetcher.Form method="POST" action="/articles">
      <input type="hidden" name="intent" value={intent} />
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
