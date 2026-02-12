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
    }
  }, [fetcher.data, fetcher.state, addError]);

  function loadMore() {
    if (!nextCursor || fetcher.state !== "idle") return;
    const url = `/articles?after=${encodeURIComponent(nextCursor)}`;
    fetcher.load(url);
  }

  return {
    articles,
    isLoadingMore: fetcher.state !== "idle",
    hasNext,
    loadMore,
  };
}
