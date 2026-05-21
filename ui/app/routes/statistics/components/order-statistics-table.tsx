import type { OrderStatisticsItemDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";
import { formatSignedPrice } from "@cashregister/routes/statistics/format";

interface OrderStatisticsTableProps {
  orders: OrderStatisticsItemDto[];
}

export function OrderStatisticsTable({ orders }: OrderStatisticsTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Order</th>
          <th className="p-2 font-semibold">Date</th>
          <th className="p-2 font-semibold text-right">Produced</th>
          <th className="p-2 font-semibold text-right">Expected Volume</th>
          <th className="p-2 font-semibold text-right">Real Volume</th>
          <th className="p-2 font-semibold text-right">Delta</th>
          <th className="p-2 font-semibold">Override</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order, index) => (
          <tr
            key={order.orderId}
            className={index % 2 === 1 ? "bg-gray-50" : undefined}
          >
            <td className="p-2 font-mono text-xs">{order.orderNumber}</td>
            <td className="p-2">
              {new Date(order.date * 1000).toLocaleString()}
            </td>
            <td className="p-2 text-right">{order.producedArticles}</td>
            <td className="p-2 text-right">
              {formatPrice(order.expectedVolumeInCents)}
            </td>
            <td className="p-2 text-right">
              {formatPrice(order.realVolumeInCents)}
            </td>
            <td className="p-2 text-right">
              {formatSignedPrice(order.deltaInCents)}
            </td>
            <td className="p-2">{order.hasOverride ? "Yes" : "No"}</td>
          </tr>
        ))}
        {orders.length === 0 && (
          <tr>
            <td
              colSpan={7}
              className="p-4 text-center text-gray-500 text-sm italic"
            >
              No order statistics found.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
