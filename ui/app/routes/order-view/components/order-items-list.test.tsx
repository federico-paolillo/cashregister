import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { OrderItemsList } from "@cashregister/routes/order-view/components/order-items-list";
import type { OrderItemDto } from "@cashregister/model";

afterEach(() => {
  cleanup();
});

const items: OrderItemDto[] = [
  { id: "item-1", article: "art-1", description: "Espresso", priceInCents: 300, quantity: 2 },
  { id: "item-2", article: "art-2", description: "Latte", priceInCents: 475, quantity: 1 },
];

describe("OrderItemsList", () => {
  it("renders each item description with quantity", () => {
    render(<OrderItemsList items={items} />);

    expect(screen.getByText("Espresso × 2")).toBeDefined();
    expect(screen.getByText("Latte × 1")).toBeDefined();
  });

  it("renders formatted line prices", () => {
    render(<OrderItemsList items={items} />);

    expect(screen.getByText("6.00")).toBeDefined();
    expect(screen.getByText("4.75")).toBeDefined();
  });

  it("renders empty state when items array is empty", () => {
    render(<OrderItemsList items={[]} />);

    expect(screen.getByText("No items.")).toBeDefined();
  });
});
