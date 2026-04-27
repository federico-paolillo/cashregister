export function buildArticlesCloseLink(until: string | null): string {
  const searchParams = new URLSearchParams();

  if (until) {
    searchParams.set("until", until);
  }

  const search = searchParams.toString();

  return search ? `/articles?${search}` : "/articles";
}

export function buildArticlesSelectionLink(
  articleId: string,
  until: string | null,
): string {
  const searchParams = new URLSearchParams();

  if (until) {
    searchParams.set("until", until);
  }

  searchParams.set("articleId", articleId);

  return `/articles?${searchParams.toString()}`;
}
