import { Fragment, useEffect, useState } from "react";
import { Form, useNavigation } from "react-router";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import { decimalToCents } from "@cashregister/money";
import type {
  ArticleListItemDto,
  ArticlesPageDto,
  PlaceOrderRequestDto,
} from "@cashregister/model";
import { ArticleSelector } from "./components/article-selector";
import { OrderSummary } from "./components/order-summary";
import type { Route } from "./+types/order";

export async function clientLoader() {
  const result = await deps.apiClient.get<ArticlesPageDto>("/articles", {
    pageSize: "500",
  });
  if (!result.ok)
    throw new Response(result.error.message, { status: result.error.status });
  return result.value.items;
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();

  const articleIds = formData.getAll("articleId") as string[];
  const quantities = formData.getAll("quantity") as string[];
  const totalOverrideRaw = formData.get("totalOverride") as string | null;

  const body: PlaceOrderRequestDto = {
    items: articleIds.map((article, i) => ({
      article,
      quantity: Number(quantities[i]),
    })),
    ...(totalOverrideRaw ? { totalOverride: Number(totalOverrideRaw) } : {}),
  };

  const result = await deps.apiClient.post("/orders", body);

  if (!result.ok) {
    return { ok: false as const, message: result.error.message };
  }

  return { ok: true as const };
}

interface CartEntry {
  article: ArticleListItemDto;
  quantity: number;
}

export default function Order({ loaderData, actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const { addError } = useErrorMessages();

  const [cart, setCart] = useState<Map<string, CartEntry>>(new Map());
  const [totalOverride, setTotalOverride] = useState<string>("");

  const articles = loaderData;
  const isPending = navigation.state !== "idle";
  const cartEntries = Array.from(cart.values());
  const computedTotal = cartEntries.reduce(
    (sum, e) => sum + e.article.price * e.quantity,
    0,
  );
  const hasOverride = totalOverride !== "" && !isNaN(Number(totalOverride));
  const displayTotal = hasOverride ? Number(totalOverride) : computedTotal;
  const totalOverrideCents = hasOverride
    ? decimalToCents(Number(totalOverride))
    : null;

  useEffect(() => {
    if (actionData?.ok === true) {
      setCart(new Map());
      setTotalOverride("");
    } else if (actionData?.ok === false) {
      addError(actionData.message);
    }
  }, [actionData, addError]);

  function decreaseQuantity(articleId: string) {
    setCart((prev) => {
      const next = new Map(prev);
      const existing = next.get(articleId);
      if (!existing) return prev;
      if (existing.quantity <= 1) {
        next.delete(articleId);
      } else {
        next.set(articleId, { ...existing, quantity: existing.quantity - 1 });
      }
      return next;
    });
  }

  function removeFromCart(articleId: string) {
    setCart((prev) => {
      const next = new Map(prev);
      next.delete(articleId);
      return next;
    });
  }

  function addToCart(article: ArticleListItemDto) {
    setCart((prev) => {
      const next = new Map(prev);
      const existing = next.get(article.id);
      if (existing) {
        next.set(article.id, { ...existing, quantity: existing.quantity + 1 });
      } else {
        next.set(article.id, { article, quantity: 1 });
      }
      return next;
    });
  }

  return (
    <div className="flex h-screen flex-col">
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">New Order</h1>
      </header>
      <div className="flex flex-1 overflow-hidden">
        <div className="flex-7 overflow-auto p-4 border-r">
          <ArticleSelector articles={articles} onSelect={addToCart} />
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
                name="totalOverride"
                value={String(totalOverrideCents)}
              />
            )}
            <OrderSummary
              cartEntries={cartEntries}
              hasOverride={hasOverride}
              displayTotal={displayTotal}
              totalOverride={totalOverride}
              onTotalOverrideChange={setTotalOverride}
              onDecrease={decreaseQuantity}
              onRemove={removeFromCart}
            />
            <div className="p-4 border-t">
              <button
                type="submit"
                disabled={isPending || cartEntries.length === 0}
                className="w-full rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {isPending ? "Submitting..." : "Submit Order"}
              </button>
            </div>
          </Form>
        </div>
      </div>
    </div>
  );
}
