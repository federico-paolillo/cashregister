import type { OrderStatisticsDto } from "@cashregister/model";
import { formatPrice } from "@cashregister/money";

interface OrderStatisticsTableProps {
  orders: OrderStatisticsDto;
  totals: OrderStatisticsDto;
}

export function OrderStatisticsTable({
  orders,
  totals,
}: OrderStatisticsTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Scope</th>
          <th className="p-2 font-semibold text-right">Orders</th>
          <th className="p-2 font-semibold text-right">Nominal Volume</th>
          <th className="p-2 font-semibold text-right">Real Volume</th>
          <th className="p-2 font-semibold text-right">Delta</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td className="p-2">All orders</td>
          <td className="p-2 text-right">{orders.orderCount}</td>
          <td className="p-2 text-right">
            {formatPrice(orders.nominalVolumeInCents)}
          </td>
          <td className="p-2 text-right">
            {formatPrice(orders.realVolumeInCents)}
          </td>
          <td className="p-2 text-right">{formatSignedPrice(orders.deltaInCents)}</td>
        </tr>
      </tbody>
      <tfoot>
        <tr className="border-t bg-gray-100 font-semibold">
          <th scope="row" className="p-2 text-left">Total</th>
          <td className="p-2 text-right">{totals.orderCount}</td>
          <td className="p-2 text-right">
            {formatPrice(totals.nominalVolumeInCents)}
          </td>
          <td className="p-2 text-right">
            {formatPrice(totals.realVolumeInCents)}
          </td>
          <td className="p-2 text-right">
            {formatSignedPrice(totals.deltaInCents)}
          </td>
        </tr>
      </tfoot>
    </table>
  );
}

function formatSignedPrice(cents: number): string {
  if (cents < 0) {
    return `-${formatPrice(Math.abs(cents))}`;
  }

  return formatPrice(cents);
}
