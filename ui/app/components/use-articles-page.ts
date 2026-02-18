import { useEffect, useRef, useState } from "react";
import { useFetcher, useNavigate } from "react-router";
import type { ArticlesPageDto, ArticleListItemDto } from "../model";
import type { Result } from "../result";
import { useErrorMessages } from "./use-error-messages";

export function useArticlesPages(initialPageResult: Result<ArticlesPageDto>) {
  const fetcher = useFetcher<Result<ArticlesPageDto>>();
  const navigate = useNavigate();

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

      // Update the route URL with the latest cursor so that React Router's
      // revalidation (triggered by a subsequent action) calls the clientLoader
      // with ?until=<cursor>, allowing it to reconstruct the full accumulated view.
      if (newPage.hasNext && newPage.next) {
        navigate(`/articles?until=${encodeURIComponent(newPage.next)}`, {
          replace: true,
          preventScrollReset: true,
        });
      }
    }
  }, [fetcher.data, fetcher.state, addError, navigate]);

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
