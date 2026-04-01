import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { OrderRow } from "@cashregister/routes/order-overview/components/order-row";
import type { OrderListItemDto } from "@cashregister/model";

afterEach(() => {
  cleanup();
});

const order: OrderListItemDto = {
  id: "1",
  number: "ORD-001",
  totalInCents: 350,
  date: 1700000000,
};

function renderRow(props?: { striped?: boolean }) {
  return render(
    <MemoryRouter>
      <table>
        <tbody>
          <OrderRow
            order={order}
            striped={props?.striped ?? false}
            to="/order/1"
          />
        </tbody>
      </table>
    </MemoryRouter>,
  );
}

describe("OrderRow", () => {
  it("renders the order number", () => {
    renderRow();
    expect(screen.getByText("ORD-001")).toBeDefined();
  });

  it("renders the formatted total", () => {
    renderRow();
    expect(screen.getByText("3.50")).toBeDefined();
  });

  it("renders the formatted date", () => {
    renderRow();
    expect(screen.getByText(new Date(1700000000 * 1000).toLocaleString())).toBeDefined();
  });

  it("applies striped background when striped is true", () => {
    renderRow({ striped: true });
    const row = screen.getByText("ORD-001").closest("tr");
    expect(row?.className).toContain("bg-gray-50");
  });

  it("does not apply striped background when striped is false", () => {
    renderRow({ striped: false });
    const row = screen.getByText("ORD-001").closest("tr");
    expect(row?.className).not.toContain("bg-gray-50");
  });

  it("renders links to the order detail page", () => {
    renderRow();
    const links = screen.getAllByRole("link");
    expect(links.length).toBeGreaterThan(0);
    for (const link of links) {
      expect(link.getAttribute("href")).toBe("/order/1");
    }
  });
});
