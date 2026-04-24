import { useState } from "react";
import { Link } from "react-router";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { OrderListItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface OrderRowProps {
  order: OrderListItemDto;
  striped: boolean;
  to: string;
}

export function OrderRow({ order, striped, to }: OrderRowProps) {
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
    <tr className={`border-b hover:bg-blue-50 ${striped ? "bg-gray-50" : ""}`}>
      <td className="p-2">
        <Link to={to} className="block">{order.number}</Link>
      </td>
      <td className="p-2 text-right">
        <Link to={to} className="block">{formatPrice(order.totalInCents)}</Link>
      </td>
      <td className="p-2 text-right">
        <Link to={to} className="block">{new Date(order.date * 1000).toLocaleString()}</Link>
      </td>
      <td className="p-2 text-center">
        <button
          type="button"
          aria-label={`Reprint ${order.number}`}
          className="inline-flex h-8 w-8 cursor-pointer items-center justify-center rounded bg-blue-600 hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          disabled={reprinting}
          onClick={reprintOrder}
        >
          <img src="/icons/printer.svg" alt="" aria-hidden="true" className="h-4 w-4" />
        </button>
      </td>
    </tr>
  );
}
