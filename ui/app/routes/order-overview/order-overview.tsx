import { Form, useNavigation } from "react-router";
import { OrdersTable } from "@cashregister/routes/order-overview/components/orders-table";
import { Spinner } from "@cashregister/components/spinner";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type { OrdersPageDto } from "@cashregister/model";
import type { Route } from "./+types/order-overview";

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const url = new URL(request.url);

  const until = url.searchParams.get("until");

  const result = await deps.apiClient.get<OrdersPageDto>(
    "/orders",
    until ? { until } : undefined,
  );

  return result;
}

export default function OrderOverview({ loaderData }: Route.ComponentProps) {
  const navigation = useNavigation();

  const isLoadingMore = navigation.state === "loading";
  const page = loaderData.ok ? loaderData.value : null;

  useLoaderError(loaderData);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Orders</h1>
      </header>
      <main className="relative flex-1 overflow-auto p-4">
        <OrdersTable orders={page?.items ?? []} />
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
    </>
  );
}
