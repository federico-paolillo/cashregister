import { useEffect, useRef, useState } from "react";
import { useFetcher } from "react-router";
import type { ArticlesPageDto, ArticleListItemDto } from "../model";
import type { Result } from "../result";
import { useErrorMessages } from "./use-error-messages";

export function useArticlesPages(initialPageResult: Result<ArticlesPageDto>) {
  const fetcher = useFetcher<Result<ArticlesPageDto>>();

  const [articles, setArticles] = useState<ArticleListItemDto[]>([]);
  const [nextCursor, setNextCursor] = useState<string | null>(null);
  const [hasNext, setHasNext] = useState<boolean>(false);

  const { addError } = useErrorMessages();

  const lastProcessedData = useRef<ArticlesPageDto | null>(null);

  // Synchronize state when initialData changes (e.g., after a revalidation of the loader)

  useEffect(() => {
    if (!initialPageResult.ok) {
      addError(initialPageResult.error.message)
      lastProcessedData.current = null;
      return;
    }

    const initialPage = initialPageResult.value;

    setArticles(initialPage.items);
    setNextCursor(initialPage.next);
    setHasNext(initialPage.hasNext);

    lastProcessedData.current = null;

  }, [initialPageResult, addError]);

  useEffect(() => {
    if (!fetcher.data) {
      return;
    }

    const fetcherResult = fetcher.data;

    if (!fetcherResult.ok) {
      addError(fetcherResult.error.message)
      return;
    }

    if (fetcher.state === "idle" && lastProcessedData.current !== fetcherResult.value) {

      const newPage = fetcherResult.value;

      lastProcessedData.current = newPage;

      setArticles((prev) => [...prev, ...newPage.items]);
      setNextCursor(newPage.next);
      setHasNext(newPage.hasNext);

      // Keep the URL in sync with the last-seen cursor.
      // When React Router revalidates after a mutation it will call clientLoader
      // with this URL, which maps to GET /articles?until=<cursor> and returns
      // the full accumulated list instead of just page 1.
      if (newPage.next) {
        const nextUrl = new URL(window.location.href);
        nextUrl.searchParams.set("until", newPage.next);
        nextUrl.searchParams.delete("after");
        window.history.replaceState({}, "", nextUrl.toString());
      }
    }
  }, [fetcher.data, fetcher.state, addError]);

  function loadMore() {
    if (!nextCursor || fetcher.state !== "idle") return;
    const url = `/articles?after=${encodeURIComponent(nextCursor)}`;
    fetcher.load(url);
  }

  function updateArticle(id: string, description: string, price: number) {
    setArticles((prev) =>
      prev.map((a) => (a.id === id ? { ...a, description, price } : a)),
    );
  }

  return {
    articles,
    isLoadingMore: fetcher.state !== "idle",
    hasNext,
    loadMore,
    updateArticle,
  };
}
