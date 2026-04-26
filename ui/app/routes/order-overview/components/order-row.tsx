import { Link } from "react-router";
import type { OrderListItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";
import { buildOrderOverviewSelectionLink } from "../url";

interface OrderRowProps {
  order: OrderListItemDto;
  striped: boolean;
  selected: boolean;
  until: string | null;
}

export function OrderRow({ order, striped, selected, until }: OrderRowProps) {
  const to = buildOrderOverviewSelectionLink(order.id, until);
  const backgroundClass = selected ? "bg-blue-100" : striped ? "bg-gray-50" : "";

  return (
    <tr className={`border-b hover:bg-blue-50 ${backgroundClass}`}>
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
