import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import OrderView from "@cashregister/routes/order-view/order-view";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import type { OrderDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
  };
});

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      get: vi.fn(),
    },
  },
}));

const mockOrder: OrderDto = {
  id: "order-123",
  number: "00000000000000000042",
  date: 1700000000,
  total: 15.5,
  totalOverride: null,
  items: [
    { id: "item-1", article: "art-1", description: "Espresso", price: 3.0, quantity: 2 },
    { id: "item-2", article: "art-2", description: "Latte", price: 4.75, quantity: 2 },
  ],
};

const mockResult: Result<OrderDto> = { ok: true, value: mockOrder };

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderOrderView(props: any = {}) {
  return render(<OrderView loaderData={mockResult} {...props} />);
}

describe("Order View Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
    } as reactRouter.Navigation);
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  afterEach(() => {
    cleanup();
  });

  it("renders the order number in the heading", () => {
    renderOrderView();

    expect(screen.getByText(`Order ${mockOrder.number}`)).toBeDefined();
  });

  it("renders all item descriptions with quantities", () => {
    renderOrderView();

    expect(screen.getByText("Espresso × 2")).toBeDefined();
    expect(screen.getByText("Latte × 2")).toBeDefined();
  });

  it("renders formatted item prices", () => {
    renderOrderView();

    expect(screen.getByText("6.00")).toBeDefined();
    expect(screen.getByText("9.50")).toBeDefined();
  });

  it("renders the total", () => {
    renderOrderView();

    expect(screen.getByText("Total")).toBeDefined();
    expect(screen.getByText("15.50")).toBeDefined();
  });

  it("does not show overridden total when totalOverride is null", () => {
    renderOrderView();

    expect(screen.queryByText("Overridden Total")).toBeNull();
  });

  it("shows overridden total when totalOverride is not null", () => {
    const result: Result<OrderDto> = {
      ok: true,
      value: { ...mockOrder, totalOverride: 12.0 },
    };

    renderOrderView({ loaderData: result });

    expect(screen.getByText("Overridden Total")).toBeDefined();
    expect(screen.getByText("12.00")).toBeDefined();
  });

  it("renders order ID in the footer", () => {
    renderOrderView();

    expect(screen.getByText(/order-123/)).toBeDefined();
  });

  it("renders formatted date in the footer", () => {
    renderOrderView();

    const expectedDate = new Date(1700000000 * 1000).toLocaleString();
    expect(screen.getByText(new RegExp(expectedDate.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")))).toBeDefined();
  });

  it("calls addError when loader returns an error", () => {
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    const errorResult: Result<OrderDto> = {
      ok: false,
      error: { message: "Order not found", status: 404 },
    };

    renderOrderView({ loaderData: errorResult });

    expect(addError).toHaveBeenCalledWith("Order not found");
  });
});
