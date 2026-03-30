import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import { OrderRow } from "@cashregister/routes/order-overview/components/order-row";
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

afterEach(() => {
  cleanup();
});

const order: OrderListItemDto = {
  id: "1",
  number: "ORD-001",
  total: 3.5,
  date: 1700000000,
};

function renderRow(props?: { striped?: boolean }) {
  return render(
    <table>
      <tbody>
        <OrderRow
          order={order}
          striped={props?.striped ?? false}
        />
      </tbody>
    </table>,
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

  it("navigates to the order detail page on click", () => {
    const mockNavigate = vi.fn();
    vi.mocked(reactRouter.useNavigate).mockReturnValue(mockNavigate);

    renderRow();
    const row = screen.getByText("ORD-001").closest("tr")!;
    fireEvent.click(row);

    expect(mockNavigate).toHaveBeenCalledWith("/order/1");
  });
});
