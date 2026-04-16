import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import Order, { clientAction, cartReducer } from "@cashregister/routes/order/order";
import type { CartEntry } from "@cashregister/routes/order/order";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
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

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      post: vi.fn(),
    },
  },
}));

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

const articles = [
  { id: "art1", description: "Espresso", priceInCents: 300 },
  { id: "art2", description: "Latte", priceInCents: 450 },
];

const loaderResult = { ok: true as const, value: { next: null, hasNext: false, items: articles } };

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderOrder(props: any = {}) {
  return render(
    <Order loaderData={loaderResult} actionData={undefined} {...props} />,
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
    const rerenderProps: any = { loaderData: loaderResult, actionData: { ok: true, value: { id: "ord-1", location: "/orders/ord-1" } } };
    rerender(<Order {...rerenderProps} />);

    expect(screen.queryByText(/Espresso × 1/)).toBeNull();
    expect(screen.getByText("No items yet.")).toBeDefined();
  });

  it("renders total override input when cart has items", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    expect(screen.getByLabelText("Custom total")).toBeDefined();
  });

  it("shows italic total when override is entered", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    const input = screen.getByLabelText("Custom total");
    fireEvent.change(input, { target: { value: "1.50" } });

    const totalSpans = screen.getAllByText("1.50");
    const italicSpan = totalSpans.find((el) =>
      el.classList.contains("italic"),
    );
    expect(italicSpan).toBeDefined();
  });

  it("reverts to computed total when override is cleared", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    const input = screen.getByLabelText("Custom total");
    fireEvent.change(input, { target: { value: "1.50" } });
    fireEvent.change(input, { target: { value: "" } });

    // The total span is inside the "Total" row, next to the "Total" label
    const totalLabel = screen.getByText("Total");
    const totalRow = totalLabel.parentElement!;
    const totalValueSpan = totalRow.querySelector("span:last-child")!;
    expect(totalValueSpan.textContent).toBe("3.00");
    expect(totalValueSpan.classList.contains("italic")).toBe(false);
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

    expect(result).toEqual({ ok: true, value: undefined });
  });

  it("sends totalOverrideInCents when present in form data", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: true,
      value: undefined,
    });

    const formData = new FormData();
    formData.append("articleId", "art1");
    formData.append("quantity", "2");
    formData.append("totalOverrideInCents", "550");

    await clientAction({
      request: new Request("http://localhost/order", {
        method: "POST",
        body: formData,
      }),
      params: {},
    } as Route.ClientActionArgs);

    expect(deps.apiClient.post).toHaveBeenCalledWith("/orders", {
      items: [{ article: "art1", quantity: 2 }],
      totalOverrideInCents: 550,
    });
  });

  it("does not send totalOverrideInCents when not in form data", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: true,
      value: undefined,
    });

    await clientAction(
      buildOrderRequest([{ articleId: "art1", quantity: "1" }]),
    );

    expect(deps.apiClient.post).toHaveBeenCalledWith("/orders", {
      items: [{ article: "art1", quantity: 1 }],
    });
  });

  it("returns ok:false with message on failure", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({
      ok: false,
      error: { status: 400, message: "Bad request" },
    });

    const result = await clientAction(
      buildOrderRequest([{ articleId: "art1", quantity: "1" }]),
    );

    expect(result).toEqual({ ok: false, error: { status: 400, message: "Bad request" } });
  });
});

const espresso = { id: "art1", description: "Espresso", priceInCents: 300 };
const latte = { id: "art2", description: "Latte", priceInCents: 450 };

describe("cartReducer", () => {
  it("adds an article with quantity 1", () => {
    const state = cartReducer(new Map(), { type: "add", article: espresso });
    expect(state.get("art1")).toEqual({ article: espresso, quantity: 1 });
  });

  it("increments quantity when the same article is added again", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso });
    state = cartReducer(state, { type: "add", article: espresso });
    expect(state.get("art1")?.quantity).toBe(2);
  });

  it("decreases quantity by one", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso });
    state = cartReducer(state, { type: "add", article: espresso });
    state = cartReducer(state, { type: "decrease", articleId: "art1" });
    expect(state.get("art1")?.quantity).toBe(1);
  });

  it("removes the article when quantity reaches zero via decrease", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso });
    state = cartReducer(state, { type: "decrease", articleId: "art1" });
    expect(state.has("art1")).toBe(false);
  });

  it("decrease on missing article returns unchanged state", () => {
    const state = new Map<string, CartEntry>();
    const next = cartReducer(state, { type: "decrease", articleId: "unknown" });
    expect(next).toBe(state);
  });

  it("removes an article", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso });
    state = cartReducer(state, { type: "add", article: latte });
    state = cartReducer(state, { type: "remove", articleId: "art1" });
    expect(state.has("art1")).toBe(false);
    expect(state.has("art2")).toBe(true);
  });

  it("clears all articles", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso });
    state = cartReducer(state, { type: "add", article: latte });
    state = cartReducer(state, { type: "clear" });
    expect(state.size).toBe(0);
  });
});
