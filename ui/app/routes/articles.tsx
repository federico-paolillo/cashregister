import { useEffect, useState } from "react";
import { useFetcher } from "react-router";
import type { Route } from "./+types/articles";
import { deps } from "../deps";
import type {
  ArticlesPageDto,
  RegisterArticleRequestDto,
} from "../model";
import { ArticleForm } from "../components/article-form";
import { Modal } from "../components/modal";
import { useModal } from "../components/use-modal";

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

export default function Articles() {
  const {
    isOpen: isCreateOpen,
    open: openCreate,
    close: closeCreate,
  } = useModal();

  const [fetcherKey, setFetcherKey] = useState(0);
  const fetcher = useFetcher({ key: `create-article-${fetcherKey}` });

  function openCreateModal() {
    setFetcherKey((k) => k + 1);
    openCreate();
  }

  useEffect(() => {
    if (fetcher.state !== "idle") return;

    const data = fetcher.data as { ok: boolean } | undefined;
    if (data?.ok) {
      closeCreate();
    }
  }, [fetcher.state, fetcher.data, closeCreate]);

  const pending = fetcher.state !== "idle";

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4">
        <h1>Articles</h1>
      </header>
      <div className="flex justify-end p-4">
        <button type="button" onClick={openCreateModal}>
          New Article
        </button>
      </div>
      <div className="flex-1 overflow-auto p-4">
        {/* Table placeholder — cursor-paginated table will go here */}
      </div>
      <Modal open={isCreateOpen} onClose={closeCreate}>
        <fetcher.Form method="POST">
          <input type="hidden" name="intent" value="create" />
          <ArticleForm pending={pending} />
        </fetcher.Form>
      </Modal>
    </div>
  );
}
