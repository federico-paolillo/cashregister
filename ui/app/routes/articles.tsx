import { useState } from "react";
import { ArticleForm } from "../components/article-form";
import { Modal } from "../components/modal";
import { useModal } from "../components/use-modal";
import { deps } from "../deps";
import type {
  RegisterArticleRequestDto
} from "../model";
import type { Route } from "./+types/articles";

export async function clientLoader() {
  // TODO: Load the first page of Articles
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();

  const intent = formData.get("intent");

  // TODO: Do something with errors

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

  const [createKey, setCreateKey] = useState(0);

  function openCreateModal() {
    setCreateKey((k) => k + 1);
    openCreate();
  }

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
        <ArticleForm key={createKey} intent="create" onSubmit={closeCreate} />
      </Modal>
    </div>
  );
}
