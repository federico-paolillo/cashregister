import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import { OrderRow } from "@cashregister/routes/order-overview/components/order-row";
import type { OrderListItemDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";

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
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

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

  it("renders the reprint action", () => {
    renderRow();

    expect(screen.getByRole("button", { name: "Reprint ORD-001" })).toBeDefined();
  });

  it("posts to the order print endpoint when reprint is clicked", async () => {
    const user = userEvent.setup();
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });
    renderRow();

    await user.click(screen.getByRole("button", { name: "Reprint ORD-001" }));

    expect(deps.apiClient.post).toHaveBeenCalledWith("/orders/1/print");
    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Order '1' printed", "info");
    });
  });

  it("disables the reprint action while the print request is pending", async () => {
    const user = userEvent.setup();
    let resolveReprint!: (result: Result<void>) => void;
    vi.mocked(deps.apiClient.post).mockReturnValue(
      new Promise<Result<void>>((resolve) => {
        resolveReprint = resolve;
      }),
    );
    renderRow();

    const button = screen.getByRole("button", { name: "Reprint ORD-001" });
    await user.click(button);

    expect(button).toHaveProperty("disabled", true);

    resolveReprint({ ok: true, value: undefined });

    await waitFor(() => {
      expect(button).toHaveProperty("disabled", false);
    });
  });

  it("shows an error when reprint fails", async () => {
    const user = userEvent.setup();
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: false,
      error: { message: "printer offline", status: 500 },
    });
    renderRow();

    await user.click(screen.getByRole("button", { name: "Reprint ORD-001" }));

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("printer offline");
    });
  });

  it("centers the reprint action cell", () => {
    renderRow();

    const cell = screen.getByRole("button", { name: "Reprint ORD-001" }).closest("td");

    expect(cell?.className).toContain("text-center");
  });
});
