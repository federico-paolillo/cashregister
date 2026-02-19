import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import Order, { clientAction } from "./order";
import * as reactRouter from "react-router";
import * as errorMessages from "../components/use-error-messages";
import { deps } from "../deps";
import type { Route } from "./+types/order";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    Form: ({
      children,
      ...props
    }: React.FormHTMLAttributes<HTMLFormElement>) => (
      <form {...props}>{children}</form>
    ),
  };
});

vi.mock("../deps", () => ({
  deps: {
    apiClient: {
      post: vi.fn(),
    },
  },
}));

vi.mock("../components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

const articles = [
  { id: "art1", description: "Espresso", price: 3.0 },
  { id: "art2", description: "Latte", price: 4.5 },
];

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderOrder(props: any = {}) {
  return render(
    <Order loaderData={articles} actionData={undefined} {...props} />,
  );
}

describe("Order", () => {
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

  it("renders all articles", () => {
    renderOrder();
    expect(screen.getByText("Espresso")).toBeDefined();
    expect(screen.getByText("Latte")).toBeDefined();
  });

  it("shows empty cart message initially", () => {
    renderOrder();
    expect(screen.getByText("No items yet.")).toBeDefined();
  });

  it("adds article to cart when tapped", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText(/Espresso × 1/)).toBeDefined();
  });

  it("increases quantity when same article is tapped multiple times", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText(/Espresso × 2/)).toBeDefined();
  });

  it("shows multiple articles in cart", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: /^Latte/i }));
    expect(screen.getByText(/Espresso × 1/)).toBeDefined();
    expect(screen.getByText(/Latte × 1/)).toBeDefined();
  });

  it("shows Total label when cart has items", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText("Total")).toBeDefined();
  });

  it("submit button is disabled when cart is empty", () => {
    renderOrder();
    expect(
      screen.getByRole("button", { name: "Submit Order" }),
    ).toHaveProperty("disabled", true);
  });

  it("submit button is enabled when cart has items", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(
      screen.getByRole("button", { name: "Submit Order" }),
    ).toHaveProperty("disabled", false);
  });

  it("submit button is disabled while pending", () => {
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "submitting",
    } as reactRouter.Navigation);
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(
      screen.getByRole("button", { name: "Submitting..." }),
    ).toHaveProperty("disabled", true);
  });

  it("decreases quantity by one when decrease is clicked", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText(/Espresso × 2/)).toBeDefined();

    fireEvent.click(screen.getByRole("button", { name: "Decrease Espresso" }));

    expect(screen.getByText(/Espresso × 1/)).toBeDefined();
  });

  it("removes article from cart when quantity reaches zero via decrease", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));

    fireEvent.click(screen.getByRole("button", { name: "Decrease Espresso" }));

    expect(screen.queryByText(/Espresso ×/)).toBeNull();
    expect(screen.getByText("No items yet.")).toBeDefined();
  });

  it("removes article from cart when remove is clicked", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText(/Espresso × 1/)).toBeDefined();

    fireEvent.click(screen.getByRole("button", { name: "Remove Espresso" }));

    expect(screen.queryByText(/Espresso × 1/)).toBeNull();
    expect(screen.getByText("No items yet.")).toBeDefined();
  });

  it("resets cart when action succeeds", () => {
    const { rerender } = renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByText(/Espresso × 1/)).toBeDefined();

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const rerenderProps: any = { loaderData: articles, actionData: { ok: true } };
    rerender(<Order {...rerenderProps} />);

    expect(screen.queryByText(/Espresso × 1/)).toBeNull();
    expect(screen.getByText("No items yet.")).toBeDefined();
  });
});

function buildOrderRequest(
  items: Array<{ articleId: string; quantity: string }>,
): Route.ClientActionArgs {
  const formData = new FormData();
  for (const item of items) {
    formData.append("articleId", item.articleId);
    formData.append("quantity", item.quantity);
  }
  return {
    request: new Request("http://localhost/order", {
      method: "POST",
      body: formData,
    }),
    params: {},
  } as Route.ClientActionArgs;
}

describe("clientAction", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("calls POST /orders with article ids and quantities", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: true,
      value: undefined,
    });

    await clientAction(
      buildOrderRequest([
        { articleId: "art1", quantity: "2" },
        { articleId: "art2", quantity: "1" },
      ]),
    );

    expect(deps.apiClient.post).toHaveBeenCalledWith("/orders", {
      items: [
        { article: "art1", quantity: 2 },
        { article: "art2", quantity: 1 },
      ],
    });
  });

  it("returns ok:true on success", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: true,
      value: undefined,
    });

    const result = await clientAction(
      buildOrderRequest([{ articleId: "art1", quantity: "1" }]),
    );

    expect(result).toEqual({ ok: true });
  });

  it("returns ok:false with message on failure", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: false,
      error: { status: 400, message: "Bad request" },
    });

    const result = await clientAction(
      buildOrderRequest([{ articleId: "art1", quantity: "1" }]),
    );

    expect(result).toEqual({ ok: false, message: "Bad request" });
  });
});
