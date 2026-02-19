import { Fragment, useEffect, useState } from "react";
import { Form, useNavigation } from "react-router";
import { useErrorMessages } from "../components/use-error-messages";
import { deps } from "../deps";
import type {
  ArticleListItemDto,
  ArticlesPageDto,
  PlaceOrderRequestDto,
} from "../model";
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

  const body: PlaceOrderRequestDto = {
    items: articleIds.map((article, i) => ({
      article,
      quantity: Number(quantities[i]),
    })),
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

function formatPrice(price: number): string {
  return price.toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

export default function Order({ loaderData, actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const { addError } = useErrorMessages();

  const [cart, setCart] = useState<Map<string, CartEntry>>(new Map());

  const articles = loaderData;
  const isPending = navigation.state !== "idle";
  const cartEntries = Array.from(cart.values());
  const total = cartEntries.reduce(
    (sum, e) => sum + e.article.price * e.quantity,
    0,
  );

  useEffect(() => {
    if (actionData?.ok === true) {
      setCart(new Map());
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
          <div className="grid grid-cols-3 gap-3">
            {articles.map((article) => (
              <button
                key={article.id}
                type="button"
                onClick={() => addToCart(article)}
                className="rounded border border-gray-200 p-4 text-left hover:bg-blue-50 active:bg-blue-100"
              >
                <div className="font-medium text-sm">{article.description}</div>
                <div className="text-sm text-gray-500">
                  {formatPrice(article.price)}
                </div>
              </button>
            ))}
            {articles.length === 0 && (
              <p className="col-span-3 text-sm italic text-gray-500">
                No articles available.
              </p>
            )}
          </div>
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
            <div className="flex-1 overflow-auto p-4">
              <h2 className="mb-3 font-semibold">Order Summary</h2>
              {cartEntries.length === 0 ? (
                <p className="text-sm italic text-gray-500">No items yet.</p>
              ) : (
                <>
                  {cartEntries.map((entry) => (
                    <div
                      key={entry.article.id}
                      className="flex items-center justify-between border-b py-1 text-sm"
                    >
                      <span>
                        {entry.article.description} × {entry.quantity}
                      </span>
                      <div className="flex items-center gap-2">
                        <button
                          type="button"
                          aria-label={`Decrease ${entry.article.description}`}
                          onClick={() => decreaseQuantity(entry.article.id)}
                          className="cursor-pointer text-gray-400 hover:text-blue-600"
                        >
                          −
                        </button>
                        <span>
                          {formatPrice(entry.article.price * entry.quantity)}
                        </span>
                        <button
                          type="button"
                          aria-label={`Remove ${entry.article.description}`}
                          onClick={() => removeFromCart(entry.article.id)}
                          className="cursor-pointer text-gray-400 hover:text-red-600"
                        >
                          ✕
                        </button>
                      </div>
                    </div>
                  ))}
                  <div className="mt-3 flex justify-between pt-3 font-semibold">
                    <span>Total</span>
                    <span>{formatPrice(total)}</span>
                  </div>
                </>
              )}
            </div>
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
