import { Link } from "react-router";
import type { OrderListItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface OrderRowProps {
  order: OrderListItemDto;
  striped: boolean;
  to: string;
}

export function OrderRow({ order, striped, to }: OrderRowProps) {
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
    </tr>
  );
}
