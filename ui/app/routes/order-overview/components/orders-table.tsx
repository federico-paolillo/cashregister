import type { OrderListItemDto } from "@cashregister/model";
import { OrderRow } from "@cashregister/routes/order-overview/components/order-row";

interface OrdersTableProps {
  orders: OrderListItemDto[];
  selectedOrderId: string | null;
  until: string | null;
}

export function OrdersTable({ orders, selectedOrderId, until }: OrdersTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Number</th>
          <th className="p-2 font-semibold text-right">Total</th>
          <th className="p-2 font-semibold text-right">Date</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order, index) => (
          <OrderRow
            key={order.id}
            order={order}
            striped={index % 2 === 1}
            selected={selectedOrderId === order.id}
            until={until}
          />
        ))}
        {orders.length === 0 && (
          <tr>
            <td colSpan={3} className="p-4 text-center text-gray-500 text-sm italic">
              No orders found.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
