import { Spinner } from "@cashregister/components/spinner";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type {
  ArticleDto,
  ArticlesPageDto,
  ChangeArticleRequestDto,
} from "@cashregister/model";
import { failure } from "@cashregister/result";
import { ArticleDetailPanel } from "@cashregister/routes/articles/components/article-detail-panel";
import { ArticlesTable } from "@cashregister/routes/articles/components/articles-table";
import { Form, Link, useNavigation } from "react-router";
import type { Route } from "./+types/articles";
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
  const articleId = formData.get("articleId");

  if (!articleId) {
    return failure({ message: "missing article id", status: 400 });
  }

  const body: ChangeArticleRequestDto = {
    description: String(formData.get("description")),
    priceInCents: Number(formData.get("priceInCents")),
    printDetailReceipt: formData.has("printDetailReceipt"),
  };

  return await deps.apiClient.post(`/articles/${articleId}`, body);
}

export default function Articles({ loaderData }: Route.ComponentProps) {
  const navigation = useNavigation();

  const data = loaderData;
  const page = data.articlesPage.ok ? data.articlesPage.value : null;
  const selectedArticle = data.selectedArticle?.ok ? data.selectedArticle.value : null;
  const isLoadingMore =
    navigation.state !== "idle" && navigation.formData?.get("until") === page?.next;

  useLoaderError(data.articlesPage);
  useLoaderError(data.selectedArticle);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Articles</h1>
      </header>
      <nav aria-label="Article actions" className="flex justify-end p-4 gap-2">
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
    </>
  );
}
