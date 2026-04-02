import { Fragment, useEffect, useReducer, useState } from "react";
import { Form, useNavigation } from "react-router";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import { decimalToCents } from "@cashregister/money";
import {
  type EntityPointerDto,
  type ArticleListItemDto,
  type ArticlesPageDto,
  type PlaceOrderRequestDto,
} from "@cashregister/model";
import { ArticleSelector } from "./components/article-selector";
import { OrderSummary } from "./components/order-summary";
import type { Route } from "./+types/order";

export async function clientLoader() {
  return deps.apiClient.get<ArticlesPageDto>("/articles", { pageSize: "500" });
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();

  const articleIds = formData.getAll("articleId") as string[];
  const quantities = formData.getAll("quantity") as string[];
  const totalOverrideRaw = formData.get("totalOverrideInCents") as string | null;

  const body: PlaceOrderRequestDto = {
    items: articleIds.map((article, i) => ({
      article,
      quantity: Number(quantities[i]),
    })),
    ...(totalOverrideRaw ? { totalOverrideInCents: Number(totalOverrideRaw) } : {}),
  };

  return deps.apiClient.post<EntityPointerDto>("/orders", body);
}

export interface CartEntry {
  article: ArticleListItemDto;
  quantity: number;
}

export type CartAction =
  | { type: "add"; article: ArticleListItemDto }
  | { type: "decrease"; articleId: string }
  | { type: "remove"; articleId: string }
  | { type: "clear" };

export function cartReducer(state: Map<string, CartEntry>, action: CartAction): Map<string, CartEntry> {
  const next = new Map(state);
  switch (action.type) {
    case "add": {
      const existing = next.get(action.article.id);
      if (existing) {
        next.set(action.article.id, { ...existing, quantity: existing.quantity + 1 });
      } else {
        next.set(action.article.id, { article: action.article, quantity: 1 });
      }
      return next;
    }
    case "decrease": {
      const existing = next.get(action.articleId);
      if (!existing) return state;
      if (existing.quantity <= 1) {
        next.delete(action.articleId);
      } else {
        next.set(action.articleId, { ...existing, quantity: existing.quantity - 1 });
      }
      return next;
    }
    case "remove": {
      next.delete(action.articleId);
      return next;
    }
    case "clear":
      return new Map();
  }
}

export default function Order({ loaderData, actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const { addError } = useErrorMessages();

  const [cart, dispatch] = useReducer(cartReducer, new Map<string, CartEntry>());
  const [totalOverride, setTotalOverride] = useState<string>("");

  const articles = loaderData.ok ? loaderData.value.items : [];
  const isPending = navigation.state !== "idle";
  const cartEntries = Array.from(cart.values());
  const computedTotalCents = cartEntries.reduce(
    (sum, e) => sum + e.article.priceInCents * e.quantity,
    0,
  );
  const hasOverride = totalOverride !== "" && !isNaN(Number(totalOverride));
  const totalOverrideCents = hasOverride
    ? decimalToCents(Number(totalOverride))
    : null;
  const displayTotalCents = totalOverrideCents ?? computedTotalCents;

  useLoaderError(loaderData);

  useEffect(() => {
    if (actionData?.ok === true) {
      dispatch({ type: "clear" });
      setTotalOverride("");
      addError(`Order '${actionData.value.id}' created`, "info");
    } else if (actionData?.ok === false) {
      addError(actionData.error.message);
    }
  }, [actionData, addError]);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">New Order</h1>
      </header>
      <main className="flex flex-1 overflow-hidden">
        <div className="flex-7 overflow-auto p-4 border-r">
          <ArticleSelector articles={articles} onSelect={(article) => dispatch({ type: "add", article })} />
        </div>
        <div className="flex-3 flex flex-col">
          <Form method="post" className="flex h-full flex-col">
            {cartEntries.map((entry) => (
              <Fragment key={entry.article.id}>
                <input
                  type="hidden"
                  name="articleId"
                  value={entry.article.id}
                />
                <input
                  type="hidden"
                  name="quantity"
                  value={String(entry.quantity)}
                />
              </Fragment>
            ))}
            {totalOverrideCents !== null && (
              <input
                type="hidden"
                name="totalOverrideInCents"
                value={String(totalOverrideCents)}
              />
            )}
            <OrderSummary
              cartEntries={cartEntries}
              hasOverride={hasOverride}
              displayTotalCents={displayTotalCents}
              totalOverride={totalOverride}
              onTotalOverrideChange={setTotalOverride}
              onDecrease={(articleId) => dispatch({ type: "decrease", articleId })}
              onRemove={(articleId) => dispatch({ type: "remove", articleId })}
            />
            <div className="p-4 border-t">
              <button
                type="submit"
                disabled={isPending || cartEntries.length === 0}
                className="w-full btn-primary"
              >
                {isPending ? "Submitting..." : "Submit Order"}
              </button>
            </div>
          </Form>
        </div>
      </main>
    </>
  );
}
