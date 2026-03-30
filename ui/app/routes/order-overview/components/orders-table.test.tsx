import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { OrdersTable } from "@cashregister/routes/order-overview/components/orders-table";
import * as reactRouter from "react-router";
import type { OrderListItemDto } from "@cashregister/model";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigate: vi.fn(),
  };
});

beforeEach(() => {
  vi.mocked(reactRouter.useNavigate).mockReturnValue(vi.fn());
});

afterEach(cleanup);

const orders: OrderListItemDto[] = [
  { id: "1", number: "ORD-001", total: 3.5, date: 1700000000 },
  { id: "2", number: "ORD-002", total: 4.5, date: 1700100000 },
];

describe("OrdersTable", () => {
  it("renders all orders", () => {
    render(<OrdersTable orders={orders} />);

    expect(screen.getByText("ORD-001")).toBeDefined();
    expect(screen.getByText("ORD-002")).toBeDefined();
  });

  it("shows the empty message when there are no orders", () => {
    render(<OrdersTable orders={[]} />);

    expect(screen.getByText("No orders found.")).toBeDefined();
  });

  it("does not show the empty message when there are orders", () => {
    render(<OrdersTable orders={orders} />);

    expect(screen.queryByText("No orders found.")).toBeNull();
  });

  it("renders alternating striped rows", () => {
    render(<OrdersTable orders={orders} />);

    const rows = screen.getAllByRole("row").slice(1); // skip header
    expect(rows[0].className).not.toContain("bg-gray-50");
    expect(rows[1].className).toContain("bg-gray-50");
  });
});
