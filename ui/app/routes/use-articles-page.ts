import { useEffect, useRef, useState } from "react";
import { useFetcher } from "react-router";
import type { ArticlesPageDto, ArticleListItemDto } from "../model";

export function useArticlesPages(initialData: ArticlesPageDto) {
  const fetcher = useFetcher<ArticlesPageDto>();
  const [articles, setArticles] = useState<ArticleListItemDto[]>(initialData.items);
  const [nextCursor, setNextCursor] = useState<string | null>(initialData.next);
  const [hasNext, setHasNext] = useState<boolean>(initialData.hasNext);
  const lastProcessedData = useRef<ArticlesPageDto | null>(null);

  // Synchronize state when initialData changes (e.g., after a revalidation of the loader)
  useEffect(() => {
    setArticles(initialData.items);
    setNextCursor(initialData.next);
    setHasNext(initialData.hasNext);
    lastProcessedData.current = null;
  }, [initialData]);

  useEffect(() => {
    if (fetcher.data && fetcher.state === "idle" && fetcher.data !== lastProcessedData.current) {
      lastProcessedData.current = fetcher.data;
      setArticles((prev) => [...prev, ...fetcher.data!.items]);
      setNextCursor(fetcher.data!.next);
      setHasNext(fetcher.data!.hasNext);
    }
  }, [fetcher.data, fetcher.state]);

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
