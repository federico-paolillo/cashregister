import React from "react";
import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { OrdersTable } from "@cashregister/routes/order-overview/components/orders-table";
import * as reactRouter from "react-router";
import type { OrderListItemDto } from "@cashregister/model";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

afterEach(() => {
  cleanup();
});

const orders: OrderListItemDto[] = [
  { id: "1", number: "ORD-001", totalInCents: 350, totalOverrideInCents: null, date: 1700000000 },
  { id: "2", number: "ORD-002", totalInCents: 450, totalOverrideInCents: 400, date: 1700100000 },
];

describe("OrdersTable", () => {
  it("renders all orders", () => {
    render(<OrdersTable orders={orders} selectedOrderId={null} until={null} />);

    expect(screen.getByText("ORD-001")).toBeDefined();
    expect(screen.getByText("ORD-002")).toBeDefined();
  });

  it("shows the empty message when there are no orders", () => {
    render(<OrdersTable orders={[]} selectedOrderId={null} until={null} />);

    expect(screen.getByText("No orders found.")).toBeDefined();
  });

  it("does not render the actions column", () => {
    render(<OrdersTable orders={orders} selectedOrderId={null} until={null} />);

    expect(screen.queryByRole("columnheader", { name: "Actions" })).toBeNull();
  });

  it("renders total and overridden total columns", () => {
    render(<OrdersTable orders={orders} selectedOrderId={null} until={null} />);

    expect(screen.getByRole("columnheader", { name: "Total" })).toBeDefined();
    expect(screen.getByRole("columnheader", { name: "Overridden Total" })).toBeDefined();
  });

  it("renders alternating striped rows", () => {
    render(<OrdersTable orders={orders} selectedOrderId={null} until={null} />);

    const rows = screen.getAllByRole("row").slice(1);

    expect(rows[0].className).not.toContain("bg-gray-50");
    expect(rows[1].className).toContain("bg-gray-50");
  });

  it("marks the selected row", () => {
    render(<OrdersTable orders={orders} selectedOrderId="2" until={null} />);

    const selectedRow = screen.getByText("ORD-002").closest("tr");

    expect(selectedRow?.className).toContain("bg-blue-100");
  });
});
