import { useEffect, useRef } from "react";
import { useFetcher } from "react-router";
import type { Route } from "./+types/articles";
import { deps } from "../deps";
import { type ArticleFormData, ArticleForm } from "../components/article-form";
import { Modal } from "../components/modal";
import { useModal } from "../components/use-modal";

interface ArticlesPageDto {
  next: string | null;
  hasNext: boolean;
  items: ArticleListItemDto[];
}

interface ArticleListItemDto {
  id: string;
  description: string;
  price: number;
}

export async function clientLoader() {
  const result = await deps.apiClient.get<ArticlesPageDto>("/articles");
  if (!result.ok) {
    throw new Response(result.error.message, {
      status: result.error.status || 500,
    });
  }
  return result.value;
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const payload = await request.json();

  if (payload.intent === "create") {
    return await deps.apiClient.post("/articles", {
      description: payload.description,
      priceInCents: payload.priceInCents,
    });
  }

  return { ok: false, error: { status: 400, message: "Unknown intent" } };
}

export default function Articles() {
  const {
    isOpen: isCreateOpen,
    open: openCreate,
    close: closeCreate,
  } = useModal();

  const fetcher = useFetcher();
  const pendingCreate = useRef(false);

  useEffect(() => {
    if (!pendingCreate.current) return;
    if (fetcher.state !== "idle") return;

    pendingCreate.current = false;

    const data = fetcher.data as { ok: boolean } | undefined;
    if (data?.ok) {
      closeCreate();
    }
  }, [fetcher.state, fetcher.data, closeCreate]);

  function handleCreate(data: ArticleFormData) {
    pendingCreate.current = true;
    fetcher.submit(
      {
        intent: "create",
        description: data.description,
        priceInCents: data.priceInCents,
      },
      { method: "POST", encType: "application/json" },
    );
  }

  const pending = fetcher.state !== "idle";

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4">
        <h1>Articles</h1>
      </header>
      <div className="flex justify-end p-4">
        <button type="button" onClick={openCreate}>
          New Article
        </button>
      </div>
      <div className="flex-1 overflow-auto p-4">
        {/* Table placeholder — cursor-paginated table will go here */}
      </div>
      <Modal open={isCreateOpen} onClose={closeCreate}>
        <ArticleForm onSave={handleCreate} pending={pending} />
      </Modal>
    </div>
  );
}
