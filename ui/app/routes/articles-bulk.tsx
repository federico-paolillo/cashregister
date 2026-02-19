import { useNavigation, Form, Link, redirect } from "react-router";
import { useState, useRef } from "react";
import { deps } from "../deps";
import type { RegisterArticleRequestDto } from "../model";
import type { Route } from "./+types/articles-bulk";

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();

  const descriptions = formData.getAll("description") as string[];
  const prices = formData.getAll("priceInCents") as string[];

  const results = await Promise.all(
    descriptions.map((description, i) => {
      const body: RegisterArticleRequestDto = {
        description,
        priceInCents: Number(prices[i]),
      };

      return deps.apiClient.post("/articles", body);
    }),
  );

  const failedCount = results.filter((r) => !r.ok).length;

  if (failedCount > 0) {
    return {
      message: `${failedCount} of ${results.length} article(s) failed to save.`,
    };
  }

  return redirect("/articles");
}

interface RowEntry {
  id: number;
}

interface BulkRowProps {
  onRemove: () => void;
  canRemove: boolean;
}

function BulkRow({ onRemove, canRemove }: BulkRowProps) {
  return (
    <div className="flex gap-4 items-end rounded border border-gray-200 p-4">
      <div className="flex flex-col gap-1 flex-1">
        <label className="text-sm font-medium text-gray-700">Description</label>
        <input
          autoFocus
          name="description"
          type="text"
          required
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>
      <div className="flex flex-col gap-1 w-36">
        <label className="text-sm font-medium text-gray-700">
          Price (cents)
        </label>
        <input
          name="priceInCents"
          type="number"
          min={0}
          required
          defaultValue={0}
          className="rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        />
      </div>
      {canRemove && (
        <button
          type="button"
          onClick={onRemove}
          className="rounded border border-red-300 px-3 py-2 text-sm text-red-600 hover:bg-red-50"
        >
          Remove
        </button>
      )}
    </div>
  );
}

export default function ArticlesBulk({ actionData }: Route.ComponentProps) {
  const navigation = useNavigation();

  const [rows, setRows] = useState<RowEntry[]>([{ id: 1 }]);
  const nextId = useRef(2);

  const isPending = navigation.state !== "idle";

  function addRow() {
    setRows((r) => [...r, { id: nextId.current++ }]);
  }

  function removeRow(id: number) {
    setRows((r) => r.filter((row) => row.id !== id));
  }

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Bulk Add Articles</h1>
      </header>
      <Form
        method="post"
        className="flex flex-col gap-4 p-4 flex-1 overflow-auto"
        onKeyDown={(e) => {
          if (e.key === "Enter" && e.target instanceof HTMLInputElement) {
            e.preventDefault();
            addRow();
          }
        }}
      >
        <div className="flex flex-col gap-3">
          {rows.map((row) => (
            <BulkRow
              key={row.id}
              onRemove={() => removeRow(row.id)}
              canRemove={rows.length > 1}
            />
          ))}
        </div>
        <div>
          <button
            type="button"
            onClick={addRow}
            disabled={isPending}
            className="rounded border border-dashed border-gray-400 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            + Add Article
          </button>
        </div>
        {actionData?.message && (
          <p className="text-sm text-red-600">{actionData.message}</p>
        )}
        <div className="flex justify-end gap-2 border-t pt-4">
          <Link
            to="/articles"
            className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={isPending}
            className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isPending ? "Saving..." : "Save"}
          </button>
        </div>
      </Form>
    </div>
  );
}
