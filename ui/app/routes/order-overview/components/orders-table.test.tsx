import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { OrdersTable } from "@cashregister/routes/order-overview/components/orders-table";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import type { OrderListItemDto } from "@cashregister/model";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      post: vi.fn(),
    },
  },
}));

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

const orders: OrderListItemDto[] = [
  { id: "1", number: "ORD-001", totalInCents: 350, date: 1700000000 },
  { id: "2", number: "ORD-002", totalInCents: 450, date: 1700100000 },
];

describe("OrdersTable", () => {
  beforeEach(() => {
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  it("renders all orders", () => {
    render(<OrdersTable orders={orders} />);

    expect(screen.getByText("ORD-001")).toBeDefined();
    expect(screen.getByText("ORD-002")).toBeDefined();
  });

  it("shows the empty message when there are no orders", () => {
    render(<OrdersTable orders={[]} />);

    expect(screen.getByText("No orders found.")).toBeDefined();
  });

  it("renders the actions column", () => {
    render(<OrdersTable orders={orders} />);

    const actionsHeader = screen.getByRole("columnheader", { name: "Actions" });
    expect(actionsHeader).toBeDefined();
    expect(actionsHeader.className).toContain("text-center");
    expect(screen.getByRole("button", { name: "Reprint ORD-001" })).toBeDefined();
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
