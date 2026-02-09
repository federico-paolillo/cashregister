import { useState } from "react";
import { ArticleForm } from "../components/article-form";
import { ArticlesTable } from "../components/articles-table";
import { Spinner } from "../components/spinner";
import { Modal } from "../components/modal";
import { useModal } from "../components/use-modal";
import { deps } from "../deps";
import { useArticlesPages } from "./use-articles-page";
import type {
  ArticlesPageDto,
  RegisterArticleRequestDto,
} from "../model";
import type { Route } from "./+types/articles";

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const url = new URL(request.url);
  const after = url.searchParams.get("after");
  const result = await deps.apiClient.get<ArticlesPageDto>(
    "/articles",
    after ? { after } : undefined,
  );
  if (!result.ok) {
    throw new Response(result.error.message, {
      status: result.error.status || 500,
    });
  }
  return result.value;
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();

  const intent = formData.get("intent");

  if (intent === "create") {
    const body: RegisterArticleRequestDto = {
      description: String(formData.get("description")),
      priceInCents: Number(formData.get("priceInCents")),
    };

    return await deps.apiClient.post("/articles", body);
  }

  return { ok: false, error: { status: 400, message: "Unknown intent" } };
}

/**
 * We use Route.ComponentProps to receive loaderData directly as a prop.
 * This approach is preferred over useLoaderData() inside the custom hook because:
 * 1. It leverages React Router v7 Framework mode's end-to-end type safety.
 * 2. It keeps the custom hook decoupled from the router context, making it easier to test.
 * 3. It makes the data flow explicit: the route component receives data and passes it to its logic.
 */
export default function Articles({ loaderData }: Route.ComponentProps) {
  const { articles, isLoadingMore, hasNext, loadMore } = useArticlesPages(loaderData);

  const {
    isOpen: isCreateOpen,
    open: openCreate,
    close: closeCreate,
  } = useModal();

  const [createKey, setCreateKey] = useState(0);

  function openCreateModal() {
    setCreateKey((k) => k + 1);
    openCreate();
  }

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Articles</h1>
      </header>
      <div className="flex justify-end p-4">
        <button
          type="button"
          onClick={openCreateModal}
          className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
        >
          New Article
        </button>
      </div>
      <div className="relative flex-1 overflow-auto p-4">
        <ArticlesTable articles={articles} />
        {isLoadingMore && <Spinner />}
      </div>
      <div className="flex justify-center p-4 border-t">
        <button
          type="button"
          onClick={loadMore}
          disabled={!hasNext || isLoadingMore}
          className="rounded border border-gray-300 px-6 py-2 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoadingMore ? "Loading..." : "Load More"}
        </button>
      </div>
      <Modal open={isCreateOpen} onClose={closeCreate}>
        <ArticleForm key={createKey} intent="create" onSubmit={closeCreate} />
      </Modal>
    </div>
  );
}
