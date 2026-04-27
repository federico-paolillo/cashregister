import { useState } from "react";
import { Link } from "react-router";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { OrderDto, OrderItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface OrderDetailPanelProps {
  order: OrderDto;
  closeTo: string;
}

function OrderItems({ items }: { items: OrderItemDto[] }) {
  if (items.length === 0) {
    return <p className="text-sm italic text-gray-500">No items.</p>;
  }

  return (
    <>
      {items.map((item) => (
        <div
          key={item.id}
          className="flex items-center justify-between border-b py-1 text-sm"
        >
          <span>
            {item.description} × {item.quantity}
          </span>
          <span>{formatPrice(item.priceInCents * item.quantity)}</span>
        </div>
      ))}
    </>
  );
}

export function OrderDetailPanel({ order, closeTo }: OrderDetailPanelProps) {
  const { addError } = useErrorMessages();
  const [reprinting, setReprinting] = useState(false);

  async function reprintOrder() {
    setReprinting(true);

    try {
      const result = await deps.apiClient.post<void>(`/orders/${order.id}/print`);

      if (!result.ok) {
        addError(result.error.message);
      } else {
        addError(`Order '${order.id}' printed`, "info");
      }
    } finally {
      setReprinting(false);
    }
  }

  return (
    <>
      <header className="flex items-start justify-between gap-4 border-b p-4">
        <div className="min-w-0">
          <h2 className="text-xl font-semibold">{`Order ${order.number}`}</h2>
          <div className="mt-3 space-y-2 text-sm text-gray-600">
            <div className="flex items-start justify-between gap-4">
              <span className="font-medium text-gray-700">Date</span>
              <span className="text-right">{new Date(order.date * 1000).toLocaleString()}</span>
            </div>
            <div className="flex items-start justify-between gap-4">
              <span className="font-medium text-gray-700">Order ID</span>
              <span className="break-all text-right">{order.id}</span>
            </div>
          </div>
        </div>
        <Link
          to={closeTo}
          aria-label="Close order details"
          className="text-2xl leading-none text-gray-400 hover:text-gray-700"
        >
          ✕
        </Link>
      </header>
      <main className="flex-1 overflow-auto p-4">
        <h3 className="mb-3 font-semibold">Items</h3>
        <OrderItems items={order.items} />
        <div className="mt-4 flex justify-between border-t pt-4 font-semibold">
          <span>Total</span>
          <span>{formatPrice(order.totalInCents)}</span>
        </div>
        {order.totalOverrideInCents !== null && (
          <div className="mt-1 flex justify-between text-sm">
            <span>Overridden Total</span>
            <span className="italic">{formatPrice(order.totalOverrideInCents)}</span>
          </div>
        )}
      </main>
      <footer className="border-t p-4">
        <button
          type="button"
          onClick={reprintOrder}
          disabled={reprinting}
          className="w-full btn-primary"
        >
          {reprinting ? "Reprinting..." : "Reprint"}
        </button>
      </footer>
    </>
  );
}
