import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import OrderOverview, { clientLoader } from "@cashregister/routes/order-overview/order-overview";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { OrderDto, OrdersPageDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";
import type { Route } from "./+types/order-overview";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) =>
      <form {...props}>{children}</form>,
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
      get: vi.fn(),
      post: vi.fn(),
    },
  },
}));

const pageData: OrdersPageDto = {
  items: [
    { id: "1", number: "ORD-001", totalInCents: 1000, totalOverrideInCents: null, date: 1700000000 },
    { id: "2", number: "ORD-002", totalInCents: 2000, totalOverrideInCents: null, date: 1700100000 },
  ],
  next: "cursor-1",
  hasNext: true,
};

const pageResult: Result<OrdersPageDto> = {
  ok: true,
  value: pageData,
};

const order: OrderDto = {
  id: "1",
  number: "ORD-001",
  date: 1700000000,
  totalInCents: 1000,
  totalOverrideInCents: null,
  items: [
    { id: "item-1", article: "art-1", description: "Espresso", priceInCents: 500, quantity: 2 },
  ],
};

const orderResult: Result<OrderDto> = {
  ok: true,
  value: order,
};

function createLoaderData(overrides?: Partial<{
  ordersPage: Result<OrdersPageDto>;
  selectedOrder: Result<OrderDto> | null;
  selectedOrderId: string | null;
  until: string | null;
}>) {
  return {
    ordersPage: pageResult,
    selectedOrder: null,
    selectedOrderId: null,
    until: null,
    ...overrides,
  };
}

function renderOrderOverview(props: Partial<Route.ComponentProps> = {}) {
  return render(
    <OrderOverview loaderData={createLoaderData()} {...props} />,
  );
}

function buildLoaderArgs(url: string): Route.ClientLoaderArgs {
  return {
    request: new Request(url),
    params: {},
  } as Route.ClientLoaderArgs;
}

describe("Order Overview Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
      formData: undefined,
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

  it("renders initial orders from loader", () => {
    renderOrderOverview();

    expect(screen.getByText("ORD-001")).toBeDefined();
    expect(screen.getByText("ORD-002")).toBeDefined();
  });

  it("does not render the detail panel when no order is selected", () => {
    renderOrderOverview();

    expect(screen.queryByRole("button", { name: "Reprint" })).toBeNull();
  });

  it("renders the detail panel when a selected order is present", () => {
    renderOrderOverview({
      loaderData: createLoaderData({
        selectedOrder: orderResult,
        selectedOrderId: order.id,
      }),
    });

    expect(screen.getByText(`Order ${order.number}`)).toBeDefined();
    expect(screen.getByText("Order ID")).toBeDefined();
    expect(screen.getByRole("button", { name: "Reprint" })).toBeDefined();
  });

  it("shows Load More button when page.next is set", () => {
    renderOrderOverview();

    expect(screen.getByRole("button", { name: "Load More" })).toBeDefined();
  });

  it("hides Load More button when page.next is null", () => {
    renderOrderOverview({
      loaderData: createLoaderData({
        ordersPage: {
          ok: true,
          value: { ...pageData, next: null, hasNext: false },
        },
      }),
    });

    expect(screen.queryByRole("button", { name: "Load More" })).toBeNull();
  });

  it("preserves the selected order when submitting load more", () => {
    renderOrderOverview({
      loaderData: createLoaderData({
        selectedOrder: orderResult,
        selectedOrderId: order.id,
      }),
    });

    const preservedSelection = screen.getByDisplayValue(order.id);

    expect(preservedSelection.getAttribute("name")).toBe("orderId");
    expect(preservedSelection.getAttribute("type")).toBe("hidden");
  });

  it("preserves URL selection state when selected order loading fails", () => {
    renderOrderOverview({
      loaderData: createLoaderData({
        selectedOrderId: "1",
        selectedOrder: {
          ok: false,
          error: { message: "Order not found", status: 404 },
        },
      }),
    });

    const selectedRow = screen.getByText("ORD-001").closest("tr");
    const preservedSelection = screen.getByDisplayValue("1");

    expect(selectedRow?.className).toContain("bg-blue-100");
    expect(preservedSelection.getAttribute("name")).toBe("orderId");
    expect(preservedSelection.getAttribute("type")).toBe("hidden");
    expect(screen.queryByRole("button", { name: "Reprint" })).toBeNull();
  });

  it("shows loading state only for the load more submission", () => {
    const formData = new FormData();
    formData.set("until", "cursor-1");

    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
      formData,
    } as reactRouter.Navigation);

    renderOrderOverview();

    expect(screen.getByRole("button", { name: "Loading..." })).toHaveProperty("disabled", true);
  });

  it("keeps the load more button label when loading a selection change", () => {
    const formData = new FormData();
    formData.set("orderId", "1");

    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
      formData,
    } as reactRouter.Navigation);

    renderOrderOverview();

    expect(screen.getByRole("button", { name: "Load More" })).toHaveProperty("disabled", false);
  });

  it("calls addError when the orders page loader returns an error", async () => {
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderOrderOverview({
      loaderData: createLoaderData({
        ordersPage: {
          ok: false,
          error: { message: "Something went wrong", status: 500 },
        },
      }),
    });

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Something went wrong");
    });
  });

  it("calls addError when the selected order loader returns an error", async () => {
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderOrderOverview({
      loaderData: createLoaderData({
        selectedOrderId: "missing-order",
        selectedOrder: {
          ok: false,
          error: { message: "Order not found", status: 404 },
        },
      }),
    });

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Order not found");
    });
  });
});

describe("clientLoader", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches only the orders page when no order is selected", async () => {
    vi.mocked(deps.apiClient.get).mockResolvedValue(pageResult);

    const result = await clientLoader(buildLoaderArgs("http://localhost/orders?until=cursor-1"));

    expect(result).toEqual({
      ordersPage: pageResult,
      selectedOrder: null,
      selectedOrderId: null,
      until: "cursor-1",
    });
    expect(deps.apiClient.get).toHaveBeenCalledTimes(1);
    expect(deps.apiClient.get).toHaveBeenCalledWith("/orders", { until: "cursor-1" });
  });

  it("fetches the orders page and the selected order when orderId is present", async () => {
    vi.mocked(deps.apiClient.get)
      .mockResolvedValueOnce(pageResult)
      .mockResolvedValueOnce(orderResult);

    const result = await clientLoader(buildLoaderArgs("http://localhost/orders?until=cursor-1&orderId=1"));

    expect(result).toEqual({
      ordersPage: pageResult,
      selectedOrder: orderResult,
      selectedOrderId: "1",
      until: "cursor-1",
    });
    expect(deps.apiClient.get).toHaveBeenNthCalledWith(1, "/orders", { until: "cursor-1" });
    expect(deps.apiClient.get).toHaveBeenNthCalledWith(2, "/orders/1");
  });
});
