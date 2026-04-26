export function buildOrderOverviewCloseLink(until: string | null): string {
  const searchParams = new URLSearchParams();

  if (until) {
    searchParams.set("until", until);
  }

  const search = searchParams.toString();

  return search ? `/orders?${search}` : "/orders";
}

export function buildOrderOverviewSelectionLink(
  orderId: string,
  until: string | null,
): string {
  const searchParams = new URLSearchParams();

  if (until) {
    searchParams.set("until", until);
  }

  searchParams.set("orderId", orderId);

  return `/orders?${searchParams.toString()}`;
}
