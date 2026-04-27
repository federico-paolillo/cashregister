import { useState } from "react";
import { Form, Link, useNavigation } from "react-router";
import { ArticleForm } from "@cashregister/routes/articles/components/article-form";
import { ArticleDetailPanel } from "@cashregister/routes/articles/components/article-detail-panel";
import { ArticlesTable } from "@cashregister/routes/articles/components/articles-table";
import { Spinner } from "@cashregister/components/spinner";
import { Modal } from "@cashregister/components/modal";
import { useModal } from "@cashregister/components/use-modal";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type {
  ArticleDto,
  ArticlesPageDto,
  ChangeArticleRequestDto,
  RegisterArticleRequestDto,
} from "@cashregister/model";
import type { Route } from "./+types/articles";
import { failure } from "@cashregister/result";
import { buildArticlesCloseLink } from "./url";

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const url = new URL(request.url);
  const until = url.searchParams.get("until");
  const selectedArticleId = url.searchParams.get("articleId");

  const [articlesPage, selectedArticle] = await Promise.all([
    deps.apiClient.get<ArticlesPageDto>(
      "/articles",
      until ? { until } : undefined,
    ),
    selectedArticleId
      ? deps.apiClient.get<ArticleDto>(`/articles/${selectedArticleId}`)
      : Promise.resolve(null),
  ]);

  return {
    articlesPage,
    selectedArticle,
    selectedArticleId,
    until,
  };
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

  const data = loaderData;
  const page = data.articlesPage.ok ? data.articlesPage.value : null;
  const selectedArticle = data.selectedArticle?.ok ? data.selectedArticle.value : null;
  const isLoadingMore =
    navigation.state !== "idle" && navigation.formData?.get("until") === page?.next;

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

  useLoaderError(data.articlesPage);
  useLoaderError(data.selectedArticle);

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
      <main className="flex flex-1 overflow-hidden">
        <div className="relative flex min-w-0 flex-1 flex-col">
          <div className="relative flex-1 overflow-auto p-4">
            <ArticlesTable
              articles={page?.items ?? []}
              selectedArticleId={data.selectedArticleId}
              until={data.until}
            />
            {navigation.state !== "idle" && <Spinner />}
          </div>
          <footer className="flex justify-center p-4 border-t">
            {page?.next && (
              <Form method="get">
                <input type="hidden" name="until" value={page.next} />
                {data.selectedArticleId && (
                  <input type="hidden" name="articleId" value={data.selectedArticleId} />
                )}
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
        </div>
        {selectedArticle && (
          <aside className="flex w-[24rem] min-w-[24rem] flex-col border-l bg-white">
            <ArticleDetailPanel
              article={selectedArticle}
              closeTo={buildArticlesCloseLink(data.until)}
            />
          </aside>
        )}
      </main>
      <Modal open={isCreateOpen} onClose={closeCreate}>
        <ArticleForm key={createKey} intent="create" onSubmit={() => closeCreate()} onError={(msg) => { closeCreate(); addError(msg); }} />
      </Modal>
    </>
  );
}
