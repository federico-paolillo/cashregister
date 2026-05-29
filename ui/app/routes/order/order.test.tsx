import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent, within } from "@testing-library/react";
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
    lowQuantityWarningThreshold: 5,
  },
}));

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

const articles = [
  { id: "art1", description: "Espresso", priceInCents: 300, printDetailReceipt: true, quantityAvailable: null },
  { id: "art2", description: "Latte", priceInCents: 450, printDetailReceipt: true, quantityAvailable: null },
];

const loaderResult = { ok: true as const, value: { next: null, hasNext: false, items: articles } };

const groupedArticles = [
  { id: "art-latte", description: "Latte", priceInCents: 450, printDetailReceipt: true, quantityAvailable: null },
  { id: "art-badge-no-detail", description: "Badge No Detail", priceInCents: 450, printDetailReceipt: false, quantityAvailable: null },
  { id: "art-number", description: "5 Arrosticini", priceInCents: 700, printDetailReceipt: true, quantityAvailable: null },
  { id: "art-acqua", description: "Acqua", priceInCents: 100, printDetailReceipt: true, quantityAvailable: null },
  { id: "art-brioche", description: "Brioche", priceInCents: 250, printDetailReceipt: true, quantityAvailable: null },
  { id: "art-badge-low-stock", description: "Badge Low Stock", priceInCents: 300, printDetailReceipt: true, quantityAvailable: 5 },
];

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

  it("groups articles by initial section and omits missing sections", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: groupedArticles,
        },
      },
    });

    const articleSectionHeadings = screen
      .getAllByRole("heading", { level: 2 })
      .filter((heading) => heading.textContent?.length === 1);

    expect(articleSectionHeadings.map((heading) => heading.textContent)).toEqual([
      "#",
      "A",
      "B",
      "L",
    ]);
    expect(screen.queryByRole("heading", { name: "C" })).toBeNull();
  });

  it("sorts articles alphabetically inside each initial section", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: groupedArticles,
        },
      },
    });

    const bSection = screen.getByRole("heading", { name: "B" }).closest("section")!;
    expect(within(bSection).getAllByRole("button").map((button) => button.getAttribute("aria-label"))).toEqual([
      "Badge Low Stock",
      "Badge No Detail",
      "Brioche",
    ]);
  });

  it("groups non-letter article starts under hash", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: groupedArticles,
        },
      },
    });

    const hashSection = screen.getByRole("heading", { name: "#" }).closest("section")!;
    expect(within(hashSection).getByRole("button", { name: "5 Arrosticini" })).toBeDefined();
  });

  it("adds an article to the cart from a grouped section", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: groupedArticles,
        },
      },
    });

    fireEvent.click(screen.getByRole("button", { name: "Acqua" }));

    expect(screen.getByText(/Acqua × 1/)).toBeDefined();
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

  it("adds an article using the multiplier and resets it", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: "5" }));
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));

    expect(screen.getByText(/Espresso × 5/)).toBeDefined();
    expect(screen.getByText("No multiplier")).toBeDefined();
  });

  it("adds the multiplier to an existing article quantity", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: "2" }));
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));

    expect(screen.getByText(/Espresso × 3/)).toBeDefined();
  });

  it("ignores a leading zero and keeps zeros after the first multiplier digit", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: "0" }));

    expect(screen.getByText("No multiplier")).toBeDefined();

    fireEvent.click(screen.getByRole("button", { name: "1" }));
    fireEvent.click(screen.getByRole("button", { name: "0" }));

    expect(screen.getByText("10x")).toBeDefined();
  });

  it("keeps the multiplier to two digits", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: "1" }));
    fireEvent.click(screen.getByRole("button", { name: "2" }));
    fireEvent.click(screen.getByRole("button", { name: "3" }));

    expect(screen.getByText("12x")).toBeDefined();
    expect(screen.queryByText("123x")).toBeNull();
  });

  it("clears the multiplier without changing the cart", () => {
    renderOrder();
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: "2" }));

    fireEvent.click(screen.getByRole("button", { name: "Clear multiplier" }));

    expect(screen.getByText(/Espresso × 1/)).toBeDefined();
    expect(screen.getByText("No multiplier")).toBeDefined();
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

  it("warns on selector and summary when cart reaches the quantity threshold", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: [{ ...articles[0], quantityAvailable: 6 }],
        },
      },
    });

    const articleButton = screen.getByRole("button", { name: /^Espresso/i });
    expect(screen.queryByLabelText("Low stock")).toBeNull();

    fireEvent.click(articleButton);

    expect(screen.getAllByLabelText("Low stock")).toHaveLength(2);
    expect(articleButton.className).not.toContain("bg-orange");
    expect(screen.getByText(/Espresso × 1/).closest("div")?.className).not.toContain("bg-orange");
    expect(screen.getByRole("button", { name: "Decrease Espresso" }).className).not.toContain("bg-orange");
    expect(screen.getByRole("button", { name: "Remove Espresso" }).className).not.toContain("text-orange");
  });

  it("clears quantity warning when cart quantity moves above the threshold", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: [{ ...articles[0], quantityAvailable: 7 }],
        },
      },
    });

    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));
    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));

    expect(screen.getAllByLabelText("Low stock")).toHaveLength(2);

    fireEvent.click(screen.getByRole("button", { name: "Decrease Espresso" }));

    expect(screen.queryByLabelText("Low stock")).toBeNull();
  });

  it("warns immediately for stored enabled quantity at the threshold", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: [{ ...articles[0], quantityAvailable: 5 }],
        },
      },
    });

    expect(screen.getByLabelText("Low stock")).toBeDefined();
  });

  it("shows detail receipt disabled status on selector and summary", () => {
    renderOrder({
      loaderData: {
        ok: true,
        value: {
          next: null,
          hasNext: false,
          items: [{ ...articles[0], printDetailReceipt: false }],
        },
      },
    });

    expect(screen.getByLabelText("Detail receipt disabled")).toBeDefined();

    fireEvent.click(screen.getByRole("button", { name: /^Espresso/i }));

    expect(screen.getAllByLabelText("Detail receipt disabled")).toHaveLength(2);
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

const espresso = { id: "art1", description: "Espresso", priceInCents: 300, printDetailReceipt: true, quantityAvailable: null };
const latte = { id: "art2", description: "Latte", priceInCents: 450, printDetailReceipt: true, quantityAvailable: null };

describe("cartReducer", () => {
  it("adds an article with quantity 1", () => {
    const state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    expect(state.get("art1")).toEqual({ article: espresso, quantity: 1 });
  });

  it("adds a batched quantity", () => {
    const state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 5 });
    expect(state.get("art1")?.quantity).toBe(5);
  });

  it("adds quantity when the same article is added again", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "add", article: espresso, quantity: 2 });
    expect(state.get("art1")?.quantity).toBe(3);
  });

  it("decreases quantity by one", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "decrease", articleId: "art1" });
    expect(state.get("art1")?.quantity).toBe(1);
  });

  it("removes the article when quantity reaches zero via decrease", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "decrease", articleId: "art1" });
    expect(state.has("art1")).toBe(false);
  });

  it("decrease on missing article returns unchanged state", () => {
    const state = new Map<string, CartEntry>();
    const next = cartReducer(state, { type: "decrease", articleId: "unknown" });
    expect(next).toBe(state);
  });

  it("removes an article", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "add", article: latte, quantity: 1 });
    state = cartReducer(state, { type: "remove", articleId: "art1" });
    expect(state.has("art1")).toBe(false);
    expect(state.has("art2")).toBe(true);
  });

  it("clears all articles", () => {
    let state = cartReducer(new Map(), { type: "add", article: espresso, quantity: 1 });
    state = cartReducer(state, { type: "add", article: latte, quantity: 1 });
    state = cartReducer(state, { type: "clear" });
    expect(state.size).toBe(0);
  });
});
