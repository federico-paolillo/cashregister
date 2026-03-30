import { useEffect } from "react";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import { formatPrice } from "@cashregister/money";
import type { OrderDto } from "@cashregister/model";
import { OrderItemsList } from "./components/order-items-list";
import type { Route } from "./+types/order-view";

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  return deps.apiClient.get<OrderDto>(`/orders/${params.orderId}`);
}

export default function OrderView({ loaderData }: Route.ComponentProps) {
  const { addError } = useErrorMessages();

  const order = loaderData.ok ? loaderData.value : null;

  useEffect(() => {
    if (!loaderData.ok) {
      addError(loaderData.error.message);
    }
  }, [loaderData, addError]);

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">
          {order ? `Order ${order.number}` : "Order"}
        </h1>
      </header>
      <div className="relative flex-1 overflow-auto p-4">
        {order && (
          <>
            <OrderItemsList items={order.items} />
            <div className="mt-3 flex justify-between pt-3 font-semibold">
              <span>Total</span>
              <span>{formatPrice(order.total)}</span>
            </div>
            {order.totalOverride !== null && (
              <div className="mt-1 flex justify-between text-sm">
                <span>Overridden Total</span>
                <span className="italic">{formatPrice(order.totalOverride)}</span>
              </div>
            )}
          </>
        )}
      </div>
      <div className="flex justify-center p-4 border-t">
        {order && (
          <p className="text-sm text-gray-500">
            {order.id} — {new Date(order.date * 1000).toLocaleString()}
          </p>
        )}
      </div>
    </div>
  );
}
