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
  totalOverrideInCents: null,
  date: 1700000000,
};

function renderRow(props?: {
  order?: OrderListItemDto;
  striped?: boolean;
  selected?: boolean;
  until?: string | null;
}) {
  return render(
    <MemoryRouter>
      <table>
        <tbody>
          <OrderRow
            order={props?.order ?? order}
            striped={props?.striped ?? false}
            selected={props?.selected ?? false}
            until={props?.until ?? null}
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

  it("renders an empty overridden total marker without an override", () => {
    renderRow();

    expect(screen.getByText("-")).toBeDefined();
  });

  it("renders the formatted overridden total", () => {
    renderRow({ order: { ...order, totalOverrideInCents: 275 } });

    expect(screen.getByText("2.75")).toBeDefined();
  });

  it("renders the formatted date", () => {
    renderRow();

    expect(screen.getByText(new Date(1700000000 * 1000).toLocaleString())).toBeDefined();
  });

  it("links to the selected order on the orders route", () => {
    renderRow();

    const links = screen.getAllByRole("link");

    expect(links.length).toBeGreaterThan(0);

    for (const link of links) {
      expect(link.getAttribute("href")).toBe("/orders?orderId=1");
    }
  });

  it("preserves the pagination cursor in selection links", () => {
    renderRow({ until: "cursor-1" });

    const links = screen.getAllByRole("link");

    for (const link of links) {
      expect(link.getAttribute("href")).toBe("/orders?until=cursor-1&orderId=1");
    }
  });

  it("applies striped background when striped is true", () => {
    renderRow({ striped: true });

    const row = screen.getByText("ORD-001").closest("tr");

    expect(row?.className).toContain("bg-gray-50");
  });

  it("applies selected background when selected is true", () => {
    renderRow({ striped: true, selected: true });

    const row = screen.getByText("ORD-001").closest("tr");

    expect(row?.className).toContain("bg-blue-100");
    expect(row?.className).not.toContain("bg-gray-50");
  });
});
