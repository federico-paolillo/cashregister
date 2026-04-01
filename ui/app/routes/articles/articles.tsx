import { useState } from "react";
import { Form, Link, useNavigation } from "react-router";
import { ArticleForm } from "@cashregister/routes/articles/components/article-form";
import { ArticlesTable } from "@cashregister/routes/articles/components/articles-table";
import { Spinner } from "@cashregister/components/spinner";
import { Modal } from "@cashregister/components/modal";
import { useModal } from "@cashregister/components/use-modal";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type {
  ArticleListItemDto,
  ArticlesPageDto,
  ChangeArticleRequestDto,
  RegisterArticleRequestDto,
} from "@cashregister/model";
import type { Route } from "./+types/articles";
import { failure } from "@cashregister/result";

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const url = new URL(request.url);

  const until = url.searchParams.get("until");

  const result = await deps.apiClient.get<ArticlesPageDto>(
    "/articles",
    until ? { until } : undefined,
  );

  return result;
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

  if (intent === "edit") {
    const articleId = String(formData.get("articleId"));

    const body: ChangeArticleRequestDto = {
      description: String(formData.get("description")),
      priceInCents: Number(formData.get("priceInCents")),
    };

    return await deps.apiClient.post(`/articles/${articleId}`, body);
  }

  return failure({ message: "unknown intent", status: 400 });
}

export default function Articles({ loaderData }: Route.ComponentProps) {
  const navigation = useNavigation();

  const { addError } = useErrorMessages();

  const isLoadingMore = navigation.state === "loading";
  const page = loaderData.ok ? loaderData.value : null;

  const {
    isOpen: isCreateOpen,
    open: openCreate,
    close: closeCreate,
  } = useModal();

  const {
    isOpen: isEditOpen,
    open: openEdit,
    close: closeEdit,
  } = useModal();

  const [createKey, setCreateKey] = useState(0);
  const [editKey, setEditKey] = useState(0);
  const [editingArticle, setEditingArticle] = useState<ArticleListItemDto | null>(null);

  function openCreateModal() {
    setCreateKey((k) => k + 1);
    openCreate();
  }

  function openEditModal(article: ArticleListItemDto) {
    setEditingArticle(article);
    setEditKey((k) => k + 1);
    openEdit();
  }

  useLoaderError(loaderData);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Articles</h1>
      </header>
      <nav aria-label="Article actions" className="flex justify-end p-4 gap-2">
        <button
          type="button"
          onClick={openCreateModal}
          className="btn-primary"
        >
          New Article
        </button>
        <Link
          to="/articles/bulk"
          className="btn-primary inline-block"
        >
          New Articles
        </Link>
      </nav>
      <main className="relative flex-1 overflow-auto p-4">
        <ArticlesTable articles={page?.items ?? []} onEdit={openEditModal} />
        {isLoadingMore && <Spinner />}
      </main>
      <footer className="flex justify-center p-4 border-t">
        {page?.next && (
          <Form method="get">
            <input type="hidden" name="until" value={page.next} />
            <button
              type="submit"
              disabled={isLoadingMore}
              className="btn-outline"
            >
              {isLoadingMore ? "Loading..." : "Load More"}
            </button>
          </Form>
        )}
      </footer>
      <Modal open={isCreateOpen} onClose={closeCreate}>
        <ArticleForm key={createKey} intent="create" onSubmit={() => closeCreate()} onError={(msg) => { closeCreate(); addError(msg); }} />
      </Modal>
      <Modal open={isEditOpen} onClose={closeEdit}>
        {editingArticle && (
          <ArticleForm
            key={editKey}
            intent="edit"
            articleId={editingArticle.id}
            initialData={{
              description: editingArticle.description,
              priceInCents: editingArticle.priceInCents,
            }}
            onSubmit={() => {
              closeEdit();
            }}
            onError={(msg) => {
              closeEdit();
              addError(msg);
            }}
          />
        )}
      </Modal>
    </>
  );
}
