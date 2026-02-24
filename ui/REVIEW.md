# Frontend Code Review

Reviewed: all files under `ui/app/`, configuration files, and all test files.

**Current state**: 91 tests pass, ESLint reports 2 issues (1 error, 1 warning), TypeScript reports 4 errors in `articles.test.tsx`.

---

## 1. Bugs

### 1.1 "New Articles" button opens the create modal AND navigates

`app/routes/articles.tsx:114-122`

```tsx
<Link to="/articles/bulk">
  <button
    type="button"
    onClick={openCreateModal}   // ← wrong handler
    className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
  >
    New Articles
  </button>
</Link>
```

The `onClick` is `openCreateModal` — copied from the "New Article" button above. Clicking "New Articles" opens the single-article create modal *and* navigates to `/articles/bulk`. The `onClick` should be removed entirely; the `<Link>` handles navigation.

Additionally, a `<button>` nested inside a `<Link>` (which renders `<a>`) is **invalid HTML** per the spec (interactive content cannot nest). Replace with a styled `<Link>`:

```tsx
<Link
  to="/articles/bulk"
  className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 inline-block"
>
  New Articles
</Link>
```

### 1.2 `z.coerce.string()` turns `undefined` into `"undefined"`

`app/settings.ts:4`

```ts
const settingsSchema = z.object({
  apiBaseUrl: z.coerce.string()
});
```

`z.coerce.string()` calls `String(input)`. When `VITE_API_BASE_URL` is not set, `import.meta.env.VITE_API_BASE_URL` is `undefined`, and `String(undefined)` is the literal string `"undefined"`. The `ApiClient` would then make requests to `undefined/articles`.

Per the AGENTS.md, an empty string is the intended default (relative paths resolve against the current origin). The schema should enforce this:

```ts
const settingsSchema = z.object({
  apiBaseUrl: z.string().default("")
});
```

Or, if you want to keep `coerce` for other reasons, add `.transform()` to catch the `undefined` → `"undefined"` conversion.

### 1.3 ESLint error in `env.d.ts`

`app/env.d.ts:14`

```ts
declare namespace React {
  interface ButtonHTMLAttributes<T> {
    command?: string;
    commandfor?: string;
  }
}
```

The type parameter `T` is unused, causing `@typescript-eslint/no-unused-vars`. Since this is augmenting React's existing generic interface, the parameter must match the original signature but can use `_T` or be prefixed with an underscore to satisfy the rule.

### 1.4 TypeScript errors in `articles.test.tsx`

`app/routes/articles.test.tsx:80,88,100,111`

The generated `Route.ComponentProps` type includes `params` and `matches` alongside `loaderData`. The test renders `<Articles loaderData={...} />` without these props, and the existing `as any` casts suppress the runtime error but not `tsc`. Either provide the full props shape or add a cast at the render-helper level (as `order.test.tsx` does with its `renderOrder` helper).

---

## 2. React Router Idioms

### 2.1 Inconsistent error handling between routes

`articles.tsx` and `order.tsx` handle loader errors differently:

| Route | Loader error strategy |
|---|---|
| `articles.tsx` | Returns `Result<T>` directly; component checks `loaderData.ok` and calls `addError()` via `useEffect` |
| `order.tsx` | Throws `new Response(...)` from the loader |

React Router's convention is to **throw** from loaders and let an `ErrorBoundary` handle errors. The `articles.tsx` approach forces the component to carry error-handling logic that could live in a boundary.

**Recommendation**: Pick one pattern. Throwing from loaders and defining `ErrorBoundary` exports is the more idiomatic choice and keeps components focused on the happy path. Alternatively, if you prefer the `Result<T>` approach everywhere (which gives you more UI control), use it consistently.

### 2.2 No `ErrorBoundary` exports on any route

Since `order.tsx` throws from its loader, an error will bubble to React Router's default error boundary (a generic error page). None of the routes export an `ErrorBoundary`, so the user sees a framework-level error screen with no way to recover or navigate back.

At minimum, `root.tsx` should export an `ErrorBoundary` component that shows a user-friendly error message with a "try again" action.

### 2.3 No `HydrateFallback`

In SPA mode (`ssr: false`), the user sees a blank screen while JavaScript loads and the `clientLoader` runs. Exporting a `HydrateFallback` from the root layout (or the heavy routes like `/order` and `/articles`) provides a loading indicator during the initial page load.

For a local-network kiosk app, cold-start latency is minimal, but a blank white screen on every hard refresh is still noticeable.

### 2.4 Action errors are silently lost in `articles.tsx`

The `clientAction` in `articles.tsx` returns a `Result<T>`, but the component never reads `actionData`. If a create or edit operation fails, the API error is returned from the action, but nothing in the component displays it. The page revalidates (re-fetches articles via loader), the modal closes, and the user has no idea the operation failed.

The `order.tsx` route handles this correctly: it reads `actionData` and calls `addError()` on failure. The `articles.tsx` route should do the same, or the `ArticleForm` component should check `fetcher.data` for errors.

---

## 3. React Mental Model

### 3.1 Ref-based state transition tracking in `ArticleForm`

`app/components/article-form.tsx:24-31`

```tsx
const prevFetcherState = useRef(fetcher.state);

useEffect(() => {
  if (prevFetcherState.current !== "idle" && idling) {
    onSubmit?.();
  }
  prevFetcherState.current = fetcher.state;
}, [fetcher.state, onSubmit]);
```

This detects "submitting → idle" transitions using a ref to track the previous value. It works but is fragile:

- ESLint flags a missing `idling` dependency (which is derived from `fetcher.state`, so the warning is technically a false positive, but the indirection confuses the linter and future readers).
- The ref-based transition pattern is imperative and doesn't survive well under React Compiler optimizations or Strict Mode double-renders.

A cleaner approach: React to `fetcher.data` instead of `fetcher.state`. When the fetcher completes, `fetcher.data` is populated. You can use a similar ref-check on `fetcher.data` (did it just appear?) or simplify:

```tsx
useEffect(() => {
  if (fetcher.state === "idle" && fetcher.data !== undefined) {
    onSubmit?.();
  }
}, [fetcher.state, fetcher.data, onSubmit]);
```

This has a different semantic (it fires whenever the fetcher is idle with data), but given that the form is key-mounted and destroyed after each submission cycle, it fires exactly once.

### 3.2 `Map` as React state

`app/routes/order.tsx:59`

```tsx
const [cart, setCart] = useState<Map<string, CartEntry>>(new Map());
```

`Map` is a mutable data structure. The code correctly clones it on each update (`new Map(prev)`), so React detects changes. However, `Map` is unconventional in React state:

- It doesn't serialize to JSON (relevant if you ever want to persist/restore cart state).
- DevTools and debugging display it as `Map {}` rather than showing entries.
- A plain `Record<string, CartEntry>` or an array would be more idiomatic.

`Map` does preserve insertion order, which matters for cart display order. `Object.entries` on a `Record` also preserves insertion order in modern JS, so a plain object would work here. This is a minor style preference, not a bug.

### 3.3 `useEffect` for reacting to `actionData`

`app/routes/order.tsx:69-75`

```tsx
useEffect(() => {
  if (actionData?.ok === true) {
    setCart(new Map());
  } else if (actionData?.ok === false) {
    addError(actionData.message);
  }
}, [actionData, addError]);
```

Using `useEffect` to synchronize component state with `actionData` is a reasonable approach in React Router — there's no built-in callback for "action completed". However, be aware that `actionData` is **sticky** in React Router: it stays set until the next action runs. If the component re-renders for other reasons while `actionData` is still `{ ok: true }`, the effect's dependency array won't change, so it won't re-fire. This is currently fine but would break if additional state changes caused re-renders that recreated the `addError` reference.

---

## 4. Code Smells & Refactoring Opportunities

### 4.1 Duplicated price formatting

`app/routes/order.tsx:48-53`:
```tsx
function formatPrice(price: number): string {
  return price.toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}
```

`app/components/article-row.tsx:16-19`:
```tsx
{article.price.toLocaleString(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})}
```

Same logic in two places. Extract a shared `formatPrice` utility.

### 4.2 `price` vs `priceInCents` inconsistency

The API returns `ArticleListItemDto.price` (in whole currency units), but the create/update DTOs use `priceInCents`. The conversion happens in `articles.tsx:154`:

```tsx
priceInCents: editingArticle.price * 100,
```

This is scattered and error-prone. If someone adds a new place that converts between units and gets it wrong, the bug is silent. Consider either:

- Making the API consistent (always cents or always decimal).
- Creating a single conversion utility (`toCents(price)` / `fromCents(cents)`) used everywhere.

### 4.3 `intent` prop is untyped `string`

`app/components/article-form.tsx:13`

```tsx
intent: string;
```

The actual values are `"create"` and `"edit"`. A union type would prevent typos and make the API self-documenting:

```tsx
intent: "create" | "edit";
```

### 4.4 Permanently disabled delete button

`app/components/article-row.tsx:30-37`

```tsx
<button
  type="button"
  aria-label={`Delete ${article.description}`}
  className="cursor-pointer text-gray-500 hover:text-red-600"
  disabled
>
  ✕
</button>
```

This button is always disabled and has no `onClick`. If deletion is planned for the future, it belongs in a task tracker — not in shipped UI. It takes up space and confuses users who see a grayed-out button with no explanation.

### 4.5 No shared navigation / layout

There is no navigation bar or header with links between routes. The `home.tsx` route says "It works!" with no links to `/articles` or `/order`. The user of this cash register app must know URLs by heart or use the browser's address bar.

For a town-fair kiosk, you'd want a simple top-level navigation: "Order" and "Articles" tabs at minimum.

### 4.6 `closedby="none"` browser support

`app/components/modal.tsx:32`

The `closedby` attribute on `<dialog>` is part of the Invoker Commands API, supported in Chrome 135+. If the cash register runs an older browser, the dialog will be closeable via Escape or backdrop click, potentially interrupting form entry.

A fallback `onCancel` handler (`e.preventDefault()`) ensures the dialog stays open regardless of browser support:

```tsx
<dialog
  ...
  onCancel={(e) => e.preventDefault()}
>
```

Similarly, the `command`/`commandfor` attributes on the Cancel button in `ArticleForm` only work in browsers supporting the Invoker Commands API. In unsupported browsers, the Cancel button does nothing (it has no `onClick` fallback).

### 4.7 Duplicated Tailwind class strings

Input styling classes appear verbatim in `article-form.tsx` and `articles-bulk.tsx`:

```
rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500
```

For a small codebase this is tolerable, but if these classes change (e.g., the focus ring color), you'd need to update multiple files. Tailwind v4's `@theme` or a CSS layer with `@apply` can centralize this. Alternatively, a small `<Input>` wrapper component.

### 4.8 Decomposition and folder structure

```
app/
├── components/          # 7 components: mix of shared and domain-specific
│   ├── article-form     ← only used by routes/articles.tsx
│   ├── article-row      ← only used by articles-table
│   ├── articles-table   ← only used by routes/articles.tsx
│   ├── error-message-list  ← shared (used by root.tsx)
│   ├── modal               ← shared
│   ├── spinner             ← shared
│   ├── use-error-messages  ← shared
│   └── use-modal           ← shared
├── routes/
│   ├── articles.tsx
│   ├── articles-bulk.tsx   # BulkRow defined inline here
│   ├── home.tsx
│   └── order.tsx
├── api-client.ts        # infrastructure, sitting at root
├── deps.ts              # infrastructure, sitting at root
├── model.ts             # all DTOs in one file
├── result.ts            # infrastructure, sitting at root
├── settings.ts          # infrastructure, sitting at root
├── root.tsx             # routing entry point
└── routes.ts            # routing entry point
```

Three observations:

**`components/` mixes domain-specific and truly shared components.** `ArticleForm`, `ArticleRow`, and `ArticlesTable` are only consumed by the `articles.tsx` route — they are article feature components that happen to live in the shared bucket. `Modal`, `Spinner`, and `ErrorMessageList` are genuinely route-agnostic. At 7 components the flat list is manageable, but the distinction is blurred.

**Inconsistent co-location.** `BulkRow` in `articles-bulk.tsx:44-81` is defined inline in its route file, which is reasonable for a small single-use component. But `ArticleRow`, `ArticleForm`, and `ArticlesTable` are extracted to `components/` even though they are equally single-use (only `articles.tsx` imports them). The choice of what to extract vs co-locate is not consistent.

**No home for domain utilities.** The duplicated `formatPrice` (section 4.1) is a symptom: there is no established place for shared domain helpers. Infrastructure files (`api-client.ts`, `result.ts`, `deps.ts`, `settings.ts`) sit at the `app/` root alongside routing entry points (`root.tsx`, `routes.ts`). If more helpers are added they will also land at the root, and the root becomes a catch-all.

For 4 routes and 7 components, restructuring now would be premature. But if the app grows, the natural direction is feature-based grouping:

```
app/
├── components/          # truly shared: modal, spinner, error-messages
├── routes/
├── articles/            # article-form, article-row, articles-table, article DTOs
├── orders/              # order DTOs, formatPrice
├── infra/               # api-client, result, deps, settings
├── root.tsx
└── routes.ts
```

This makes the dependency graph explicit: routes import from their feature module, shared components, and infra.

### 4.9 `articles-bulk.tsx` fires N parallel requests

`app/routes/articles-bulk.tsx:13-22`

Each article is created with a separate `POST /articles` call via `Promise.all`. If 3 of 10 fail, the remaining 7 are already persisted and there's no rollback. The user sees "3 of 10 article(s) failed to save" but doesn't know which ones failed and can't easily retry just the failures.

For a local-network single-user app this is acceptable, but consider a batch endpoint on the backend if bulk creation is a frequent operation.

---

## 5. Strengths

1. **Right-sized architecture**: No Redux, no Zustand, no complex state management. React Router's data APIs handle the majority of data flow. Local `useState` for UI-only state. This is appropriate for a single-user kiosk app.

2. **Composition root (`deps.ts`)**: Clean dependency injection via ES module caching. Easy to mock in tests. The `Deps` interface makes the boundary explicit.

3. **`Result<T>` pattern**: Consistent error handling via discriminated unions. Matches the backend's pattern, creating a shared mental model across the stack.

4. **Thorough test coverage**: 91 tests across 10 test files. Components, hooks, loaders, and actions are all tested. Tests properly isolate dependencies using `vi.mock`. The error-message hook alone has 18 test cases covering auto-dismiss timing, eviction, and cleanup.

5. **`useFetcher` for modal forms**: The `ArticleForm` uses `useFetcher` rather than a page-level `<Form>`, so submitting from a modal doesn't cause a full-page navigation. This is the correct React Router pattern for "inline" mutations.

6. **Error message system**: Well-designed with auto-dismiss, FIFO eviction, configurable limits, proper timer cleanup on unmount. The `useRef` for timers and ID counter is correct — these are bookkeeping values that shouldn't trigger re-renders.

7. **Uncontrolled form inputs**: Using `defaultValue` + `FormData` instead of controlled inputs with `useState` is appropriate for forms submitted via React Router's `<Form>`. Less state, fewer re-renders.

8. **Clean component decomposition**: `ArticlesTable` → `ArticleRow`, `ErrorMessageList` → `ErrorMessageItem`, `Modal` + `ModalIdProvider` + `useModal`. Each component has a single responsibility.

9. **Type-safe route props**: Using React Router's generated `Route.ComponentProps` and `Route.ClientLoaderArgs` types ensures type safety between loaders, actions, and components.

10. **Minimal dependencies**: The app uses React, React Router, Tailwind, and Zod. No bloated component libraries or unnecessary abstractions. This minimizes supply-chain risk and keeps the bundle small — important for a local-network deployment.

---

## 6. Summary of Recommended Actions

**Fix (bugs)**:
1. Remove `onClick={openCreateModal}` from the "New Articles" `<Link>` and unwrap the nested `<button>`.
2. Fix the Zod schema in `settings.ts` so an unset `VITE_API_BASE_URL` defaults to `""`.
3. Fix the unused `T` in `env.d.ts`.
4. Fix the TypeScript errors in `articles.test.tsx`.

**Improve (idiom compliance)**:
5. Pick one error-handling strategy for loaders (throw + ErrorBoundary, or Result + component handling) and apply it consistently.
6. Export an `ErrorBoundary` from `root.tsx` at minimum.
7. Surface action errors in `articles.tsx` (read `actionData` or check `fetcher.data`).

**Consider (maintainability)**:
8. Extract shared `formatPrice` utility.
9. Add a `<nav>` layout with links to `/order` and `/articles`.
10. Type the `intent` prop as `"create" | "edit"`.
11. Add an `onCancel` fallback on `<dialog>` for browsers without `closedby` support.
12. Add an `onClick` fallback on the Cancel button for browsers without Invoker Commands support.
13. Remove or un-disable the dead delete button.
14. Clarify the `components/` vs co-located split: either move article-specific components next to their route or into a feature directory, and establish a home for domain utilities and infrastructure files.
