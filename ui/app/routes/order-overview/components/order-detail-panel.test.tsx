import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import { OrderDetailPanel } from "@cashregister/routes/order-overview/components/order-detail-panel";
import type { OrderDto } from "@cashregister/model";
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

const order: OrderDto = {
  id: "order-123",
  number: "00000000000000000042",
  date: 1700000000,
  totalInCents: 1550,
  totalOverrideInCents: null,
  items: [
    { id: "item-1", article: "art-1", description: "Espresso", priceInCents: 300, quantity: 2 },
    { id: "item-2", article: "art-2", description: "Latte", priceInCents: 475, quantity: 2 },
  ],
};

function renderPanel(props?: {
  order?: OrderDto;
  closeTo?: string;
}) {
  return render(
    <MemoryRouter>
      <div className="flex h-[600px]">
        <div className="flex w-[24rem] flex-col">
          <OrderDetailPanel
            order={props?.order ?? order}
            closeTo={props?.closeTo ?? "/orders"}
          />
        </div>
      </div>
    </MemoryRouter>,
  );
}

describe("OrderDetailPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  it("renders persisted order metadata", () => {
    renderPanel();

    expect(screen.getByText(`Order ${order.number}`)).toBeDefined();
    expect(screen.getByText("Date")).toBeDefined();
    expect(screen.getByText("Order ID")).toBeDefined();
    expect(screen.getByText(order.id)).toBeDefined();
  });

  it("renders items and the total", () => {
    renderPanel();

    expect(screen.getByText("Espresso × 2")).toBeDefined();
    expect(screen.getByText("Latte × 2")).toBeDefined();
    expect(screen.getByText("Total")).toBeDefined();
    expect(screen.getByText("15.50")).toBeDefined();
  });

  it("renders the overridden total when present", () => {
    renderPanel({
      order: { ...order, totalOverrideInCents: 1200 },
    });

    expect(screen.getByText("Overridden Total")).toBeDefined();
    expect(screen.getByText("12.00")).toBeDefined();
  });

  it("renders the close link", () => {
    renderPanel({ closeTo: "/orders?until=cursor-1" });

    const closeLink = screen.getByRole("link", { name: "Close order details" });

    expect(closeLink.getAttribute("href")).toBe("/orders?until=cursor-1");
  });

  it("posts to the order print endpoint when reprint is clicked", async () => {
    const user = userEvent.setup();
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderPanel();

    await user.click(screen.getByRole("button", { name: "Reprint" }));

    expect(deps.apiClient.post).toHaveBeenCalledWith("/orders/order-123/print");

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Order 'order-123' printed", "info");
    });
  });

  it("disables the reprint button while the request is pending", async () => {
    const user = userEvent.setup();
    let resolveReprint!: (result: Result<void>) => void;

    vi.mocked(deps.apiClient.post).mockReturnValue(
      new Promise<Result<void>>((resolve) => {
        resolveReprint = resolve;
      }),
    );

    renderPanel();

    const button = screen.getByRole("button", { name: "Reprint" });

    await user.click(button);

    expect(screen.getByRole("button", { name: "Reprinting..." })).toHaveProperty("disabled", true);

    resolveReprint({ ok: true, value: undefined });

    await waitFor(() => {
      expect(screen.getByRole("button", { name: "Reprint" })).toHaveProperty("disabled", false);
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

    renderPanel();

    await user.click(screen.getByRole("button", { name: "Reprint" }));

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("printer offline");
    });
  });
});
