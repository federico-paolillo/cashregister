import { Form, useNavigation } from "react-router";
import { OrdersTable } from "@cashregister/routes/order-overview/components/orders-table";
import { OrderDetailPanel } from "@cashregister/routes/order-overview/components/order-detail-panel";
import { Spinner } from "@cashregister/components/spinner";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type { OrderDto, OrdersPageDto } from "@cashregister/model";
import { buildOrderOverviewCloseLink } from "./url";
import type { Route } from "./+types/order-overview";

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const url = new URL(request.url);
  const until = url.searchParams.get("until");
  const selectedOrderId = url.searchParams.get("orderId");

  const [ordersPage, selectedOrder] = await Promise.all([
    deps.apiClient.get<OrdersPageDto>("/orders", until ? { until } : undefined),
    selectedOrderId
      ? deps.apiClient.get<OrderDto>(`/orders/${selectedOrderId}`)
      : Promise.resolve(null),
  ]);

  return {
    ordersPage,
    selectedOrder,
    selectedOrderId,
    until,
  };
}

export default function OrderOverview({ loaderData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const data = loaderData;

  const page = data.ordersPage.ok ? data.ordersPage.value : null;
  const selectedOrder = data.selectedOrder?.ok ? data.selectedOrder.value : null;
  const isLoadingMore =
    navigation.state !== "idle" && navigation.formData?.get("until") === page?.next;

  useLoaderError(data.ordersPage);
  useLoaderError(data.selectedOrder);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Orders</h1>
      </header>
      <main className="flex flex-1 overflow-hidden">
        <div className="relative flex min-w-0 flex-1 flex-col">
          <div className="relative flex-1 overflow-auto p-4">
            <OrdersTable
              orders={page?.items ?? []}
              selectedOrderId={data.selectedOrderId}
              until={data.until}
            />
            {navigation.state !== "idle" && <Spinner />}
          </div>
          <footer className="flex justify-center p-4 border-t">
            {page?.next && (
              <Form method="get">
                <input type="hidden" name="until" value={page.next} />
                {data.selectedOrderId && (
                  <input type="hidden" name="orderId" value={data.selectedOrderId} />
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
        {selectedOrder && (
          <aside className="flex w-[24rem] min-w-[24rem] flex-col border-l bg-white">
            <OrderDetailPanel
              order={selectedOrder}
              closeTo={buildOrderOverviewCloseLink(data.until)}
            />
          </aside>
        )}
      </main>
    </>
  );
}
