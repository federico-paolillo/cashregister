import { type FormEvent, useState } from "react";

export interface ArticleFormData {
  description: string;
  priceInCents: number;
}

interface ArticleFormProps {
  initialData?: ArticleFormData;
  onSave: (data: ArticleFormData) => void;
  onCancel: () => void;
  pending?: boolean;
}

export function ArticleForm({
  initialData,
  onSave,
  onCancel,
  pending,
}: ArticleFormProps) {
  const [description, setDescription] = useState(
    initialData?.description ?? "",
  );
  const [priceInCents, setPriceInCents] = useState(
    initialData?.priceInCents ?? 0,
  );

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    onSave({ description, priceInCents });
  }

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label htmlFor="description">Description</label>
        <input
          id="description"
          type="text"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
        />
      </div>
      <div>
        <label htmlFor="priceInCents">Price (cents)</label>
        <input
          id="priceInCents"
          type="number"
          value={priceInCents}
          onChange={(e) => setPriceInCents(Number(e.target.value))}
          required
          min={0}
        />
      </div>
      <div>
        <button type="button" onClick={onCancel} disabled={pending}>
          Cancel
        </button>
        <button type="submit" disabled={pending}>
          Save
        </button>
      </div>
    </form>
  );
}
