import { MoneyInput } from "@cashregister/components/money-input";
import type { Result } from "@cashregister/result";
import { useEffect, useId, useRef } from "react";
import { useFetcher } from "react-router";

export interface ArticleFormData {
  description: string;
  priceInCents: number;
  printDetailReceipt: boolean;
}

interface ArticleFormProps {
  articleId?: string;
  initialData?: ArticleFormData;
  onSubmit?: () => void;
  onError?: (message: string) => void;
}

export function ArticleForm({
  articleId,
  initialData,
  onSubmit,
  onError,
}: ArticleFormProps) {
  const fetcher = useFetcher<Result<unknown>>();
  const descriptionId = useId();
  const priceId = useId();
  const printDetailReceiptId = useId();

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
      {articleId && <input type="hidden" name="articleId" value={articleId} />}
      <div className="flex flex-col gap-1">
        <label htmlFor={descriptionId} className="text-sm font-medium text-gray-700">
          Description
        </label>
        <input
          id={descriptionId}
          name="description"
          type="text"
          defaultValue={initialData?.description ?? ""}
          required
          className="input-field"
        />
      </div>
      <MoneyInput
        id={priceId}
        name="priceInCents"
        label="Price"
        defaultCents={initialData?.priceInCents ?? 0}
        required
      />
      <label
        htmlFor={printDetailReceiptId}
        className="flex items-center gap-2 text-sm font-medium text-gray-700"
      >
        <input
          id={printDetailReceiptId}
          name="printDetailReceipt"
          type="checkbox"
          defaultChecked={initialData?.printDetailReceipt ?? true}
        />
        Detail receipt
      </label>
      <div className="flex justify-end gap-2">
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
