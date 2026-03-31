import { formatPrice } from "@cashregister/money";
import type { OrderItemDto } from "@cashregister/model";

interface OrderItemsListProps {
  items: OrderItemDto[];
}

export function OrderItemsList({ items }: OrderItemsListProps) {
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
