import { useNavigate } from "react-router";
import type { OrderListItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface OrderRowProps {
  order: OrderListItemDto;
  striped: boolean;
}

export function OrderRow({ order, striped }: OrderRowProps) {
  const navigate = useNavigate();

  return (
    <tr
      className={`border-b hover:bg-blue-50 cursor-pointer ${striped ? "bg-gray-50" : ""}`}
      onClick={() => navigate(`/order/${order.id}`)}
    >
      <td className="p-2">{order.number}</td>
      <td className="p-2 text-right">
        {formatPrice(order.totalInCents)}
      </td>
      <td className="p-2 text-right">
        {new Date(order.date * 1000).toLocaleString()}
      </td>
    </tr>
  );
}
