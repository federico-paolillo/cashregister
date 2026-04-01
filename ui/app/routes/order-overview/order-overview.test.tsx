import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import OrderOverview from "@cashregister/routes/order-overview/order-overview";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import type { OrdersPageDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";

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
    },
  },
}));

const mockInitialData: OrdersPageDto = {
  items: [
    { id: "1", number: "ORD-001", totalInCents: 1000, date: 1700000000 },
    { id: "2", number: "ORD-002", totalInCents: 2000, date: 1700100000 },
  ],
  next: "cursor-1",
  hasNext: true,
};

const mockInitialResult: Result<OrdersPageDto> = {
  ok: true,
  value: mockInitialData,
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderOrderOverview(props: any = {}) {
  return render(
    <OrderOverview loaderData={mockInitialResult} {...props} />,
  );
}

describe("Order Overview Page", () => {
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

  it("renders initial orders from loader", () => {
    renderOrderOverview();

    expect(screen.getByText("ORD-001")).toBeDefined();
    expect(screen.getByText("ORD-002")).toBeDefined();
  });

  it("shows Load More button when page.next is set", () => {
    renderOrderOverview();

    expect(screen.getByRole("button", { name: "Load More" })).toBeDefined();
  });

  it("hides Load More button when page.next is null", () => {
    const data: Result<OrdersPageDto> = {
      ok: true,
      value: { ...mockInitialData, next: null, hasNext: false },
    };

    renderOrderOverview({ loaderData: data });

    expect(screen.queryByRole("button", { name: "Load More" })).toBeNull();
  });

  it("shows loading state when navigation is loading", () => {
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
    } as reactRouter.Navigation);

    renderOrderOverview();

    expect(screen.getByText("Loading...")).toBeDefined();
    expect(screen.getByRole("button", { name: "Loading..." })).toHaveProperty("disabled", true);
  });

  it("calls addError when loader returns an error", () => {
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    const errorResult: Result<OrdersPageDto> = {
      ok: false,
      error: { message: "Something went wrong", status: 500 },
    };

    renderOrderOverview({ loaderData: errorResult });

    expect(addError).toHaveBeenCalledWith("Something went wrong");
  });
});
