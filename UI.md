# UI Code Review

**Scope:** `ui/app/components/` and `ui/app/routes/` — all React components.
**Excluded:** test files, generated `.react-router/` types.
**Cross-reference:** findings already present in `REVIEW.md` (4.5 emoji buttons, 5.7 experimental attributes, 5.8 spinner accessibility) are not repeated here.

---

### 1 — MissingMainLandmark

**File:** `ui/app/routes/articles/articles.tsx` (line 121), `ui/app/routes/articles-bulk/articles-bulk.tsx` (line 61), `ui/app/routes/order/order.tsx` (line 122), `ui/app/routes/order-overview/order-overview.tsx` (line 42), `ui/app/routes/order-view/order-view.tsx` (line 31)
**Severity:** Major
**Category:** Semantics

Every route's primary content area is a `<div className="relative flex-1 overflow-auto p-4">`. The `<main>` element is the designated HTML5 landmark for the page's primary content. Without it, assistive technology users cannot jump to the main content area and screen readers cannot expose the correct document structure.

**Fix:** Replace each content-area `<div>` with `<main>`:
```tsx
<main className="relative flex-1 overflow-auto p-4">
  ...
</main>
```
**Status:** DONE

---

### 2 — MissingNavLandmark

**File:** `ui/app/routes/articles/articles.tsx` (lines 106–120)
**Severity:** Minor
**Category:** Semantics

The toolbar row containing "New Article" (button) and "New Articles" (Link) is wrapped in a plain `<div className="flex justify-end p-4 gap-2">`. The "New Articles" link navigates to a different route. A grouping that includes navigation links should use `<nav>` so the browser and assistive technology can identify it as a navigation landmark.

**Fix:**
```tsx
<nav aria-label="Article actions" className="flex justify-end p-4 gap-2">
  ...
</nav>
```
**Status:** DONE

---

### 3 — MissingFooterLandmark

**File:** `ui/app/routes/articles/articles.tsx` (line 125), `ui/app/routes/order-overview/order-overview.tsx` (line 46), `ui/app/routes/order-view/order-view.tsx` (line 48)
**Severity:** Minor
**Category:** Semantics

The bottom bar (pagination button / metadata) in three routes is a `<div className="flex justify-center p-4 border-t">`. This area semantically qualifies as a page `<footer>` — it contains secondary/supplementary content for the page. Using `<footer>` communicates the document structure to assistive technology.

**Fix:** Replace with `<footer className="flex justify-center p-4 border-t">`.
**Status:** DONE

---

### 4 — InaccessibleClickableTableRow

**File:** `ui/app/routes/order-overview/components/order-row.tsx` (lines 13–26)
**Severity:** Major
**Category:** Semantics

`OrderRow` attaches an `onClick` handler to a `<tr>` element to navigate to the order detail view. The `<tr>` has no `tabIndex`, no `role`, and no `onKeyDown` handler. Keyboard-only users cannot reach or activate any order row; the navigation is entirely invisible to assistive technology.

**Fix:** Either add `tabIndex={0}`, `role="button"`, and `onKeyDown` handling to the `<tr>`, or — preferably — render the row's content as `<td>` cells each containing an anchor `<Link>`, which gives keyboard and AT support for free:
```tsx
<td className="p-2">
  <Link to={`/order/${order.id}`}>{order.number}</Link>
</td>
```
**Status:** DONE

---

### 5 — UnassociatedBulkRowLabels

**File:** `ui/app/routes/articles-bulk/components/bulk-row.tsx` (lines 10–30)
**Severity:** Major
**Category:** Semantics

Both `<label>` elements in `BulkRow` are missing `htmlFor` attributes, and both `<input>` elements are missing `id` attributes. The labels are visually adjacent but programmatically unrelated to their inputs. Screen readers will not announce the field name when the input receives focus. The corresponding `ArticleForm` (`article-form.tsx`) correctly uses `htmlFor`/`id` pairs — this component should follow the same pattern.

**Fix:**
```tsx
<label htmlFor="description" className="...">Description</label>
<input id="description" name="description" ... />

<label htmlFor="priceInCents" className="...">Price (cents)</label>
<input id="priceInCents" name="priceInCents" ... />
```
Note: because multiple `BulkRow` instances exist in the same form, `id` values must be unique — accept a row index or generate an id with `useId()`.
**Status:** DONE

---

### 6 — ToastContainerMissingLiveRegion

**File:** `ui/app/components/error-message-list.tsx` (lines 11–21)
**Severity:** Minor
**Category:** Semantics

`ErrorMessageList` renders error toasts in a fixed-position `<div>`. Each individual `ErrorMessageItem` already carries `role="alert"` (an implicit `aria-live="assertive"` region). However, `role="alert"` is designed for one-off critical messages — applying it to every toast in a list can cause screen readers to interrupt repeatedly. The container itself should be a `role="log"` region with `aria-live="polite"` and `aria-label`, which is the correct ARIA pattern for a notification list. Individual items inside a `role="log"` do not need additional `role="alert"`.

**Fix:**
```tsx
<div
  role="log"
  aria-live="polite"
  aria-label="Notifications"
  className="fixed bottom-4 right-4 z-50 flex flex-col gap-2"
>
```
Remove `role="alert"` from `ErrorMessageItem`.
**Status:** DONE

---

### 7 — DuplicatedPageShellStructure

**File:** `ui/app/routes/articles/articles.tsx` (lines 102–163), `ui/app/routes/articles-bulk/articles-bulk.tsx` (lines 56–110), `ui/app/routes/order/order.tsx` (lines 118–171), `ui/app/routes/order-overview/order-overview.tsx` (lines 37–62), `ui/app/routes/order-view/order-view.tsx` (lines 24–57)
**Severity:** Major
**Category:** Style

All five page-level routes reproduce the same outer shell independently:
```tsx
<div className="flex h-screen flex-col">
  <header className="p-4 border-b">
    <h1 className="text-xl font-semibold">...</h1>
  </header>
  {/* content area */}
  {/* footer bar */}
</div>
```
This is five copies of the same structural code. A change to the shell (e.g. adding a global nav bar, changing padding) requires editing five files. Per AGENTS.md cross-cutting components belong in `app/components/`.

**Fix:** Extract a `PageLayout` component to `ui/app/components/page-layout.tsx`:
```tsx
interface PageLayoutProps {
  title: string;
  footer?: ReactNode;
  children: ReactNode;
}
export function PageLayout({ title, footer, children }: PageLayoutProps) {
  return (
    <div className="flex h-screen flex-col">
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">{title}</h1>
      </header>
      <main className="relative flex-1 overflow-auto p-4">{children}</main>
      {footer && <footer className="flex justify-center p-4 border-t">{footer}</footer>}
    </div>
  );
}
```
**Status:** DONE

---

### 8 — DuplicatedButtonStyles

**File:** `ui/app/routes/articles/articles.tsx` (lines 108, 129), `ui/app/routes/articles-bulk/articles-bulk.tsx` (lines 85, 101), `ui/app/routes/order/order.tsx` (line 162), `ui/app/routes/order-overview/order-overview.tsx` (line 53), `ui/app/routes/order-view/order-view.tsx` — and `root.tsx` (line 57)
**Severity:** Minor
**Category:** Style

The primary button class string `rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50` appears verbatim in at least 7 locations. Secondary/outline button styles are similarly repeated. This violates the DRY principle and makes visual restyling a multi-file effort.

**Fix:** Define reusable button style constants or a `Button` component in `ui/app/components/`. Alternatively, use Tailwind CSS `@apply` in `app.css` to create `.btn-primary` and `.btn-secondary` utilities. Either approach collapses all duplicates to a single source.
**Status:** DONE

---

### 9 — DuplicatedInputStyles

**File:** `ui/app/routes/articles/components/article-form.tsx` (lines 54, 68), `ui/app/routes/articles-bulk/components/bulk-row.tsx` (lines 16, 28), `ui/app/routes/order/components/order-summary.tsx` (line 87)
**Severity:** Minor
**Category:** Style

The text input class string `rounded border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500` is copy-pasted across three separate form components in two different routes. Any change to focus styling requires touching all three.

**Fix:** Extract to a shared `@apply` utility class in `app.css`, a `inputClassName` constant, or an `Input` wrapper component in `ui/app/components/`.
**Status:** DONE

---

### 10 — DuplicatedCartEntryInterface

**File:** `ui/app/routes/order/order.tsx` (lines 48–51), `ui/app/routes/order/components/order-summary.tsx` (lines 4–7)
**Severity:** Minor
**Category:** Composability

`CartEntry` is defined identically in both files:
```ts
interface CartEntry {
  article: ArticleListItemDto;
  quantity: number;
}
```
The two definitions must be kept in sync manually. If the shape changes, both files need updating.

**Fix:** Move `CartEntry` to a single shared location. Per AGENTS.md it is only used in the `order` route, so the canonical definition should live in `order.tsx` and be imported by `order-summary.tsx`:
```ts
// order.tsx
export interface CartEntry { ... }

// order-summary.tsx
import type { CartEntry } from "../order";
```
**Status:** DONE

---

### 11 — DuplicatedLoaderErrorEffect

**File:** `ui/app/routes/articles/articles.tsx` (lines 95–99), `ui/app/routes/order-overview/order-overview.tsx` (lines 31–35), `ui/app/routes/order-view/order-view.tsx` (lines 18–22)
**Severity:** Minor
**Category:** Composability

All three routes contain an identical `useEffect`:
```ts
useEffect(() => {
  if (!loaderData.ok) {
    addError(loaderData.error.message);
  }
}, [loaderData, addError]);
```
This pattern will appear in every future route that returns a `Result` from `clientLoader`. It is a candidate for extraction into a custom hook.

**Fix:** Add a `useLoaderError` hook to `ui/app/components/`:
```ts
// ui/app/components/use-loader-error.tsx
export function useLoaderError(loaderData: Result<unknown>) {
  const { addError } = useErrorMessages();
  useEffect(() => {
    if (!loaderData.ok) addError(loaderData.error.message);
  }, [loaderData, addError]);
}
```
**Status:** DONE

---

### 12 — InconsistentClientActionReturnShapes

**File:** `ui/app/routes/articles/articles.tsx` (lines 32–58), `ui/app/routes/order/order.tsx` (lines 24–46), `ui/app/routes/articles-bulk/articles-bulk.tsx` (lines 8–33)
**Severity:** Major
**Category:** Idioms

The three `clientAction` functions return incompatible shapes:

| Route | Returns |
|---|---|
| `articles.tsx` | `Result<unknown>` from `@cashregister/result` |
| `order.tsx` | `{ ok: true }` or `{ ok: false, message: string }` (hand-rolled) |
| `articles-bulk.tsx` | `redirect("/articles")` or `{ message: string }` (no `ok` field) |

Components consuming `actionData` therefore need route-specific type guards. The `Result<T>` pattern already exists in `@cashregister/result` and is used by the API client throughout. There is no reason for `order.tsx` to reinvent a parallel discriminated union.

**Fix:** All `clientAction` functions that return data (not redirects) should return `Result<T>` from `@cashregister/result`. Use the existing `ok(value)` and `failure(error)` constructors.
**Status:** DONE

---

### 13 — InconsistentLoaderErrorStrategy

**File:** `ui/app/routes/order/order.tsx` (lines 15–22) vs `ui/app/routes/articles/articles.tsx` (lines 19–30), `ui/app/routes/order-overview/order-overview.tsx` (lines 10–21), `ui/app/routes/order-view/order-view.tsx` (lines 9–11)
**Severity:** Major
**Category:** Idioms

`order.tsx` throws `new Response(result.error.message, { status: ... })` from its `clientLoader`, which routes the error through React Router's `ErrorBoundary`. Every other loader returns the `Result` directly and handles the error inside the component via `useEffect`. The inconsistency means different routes behave differently on load failure: some show an inline toast; one replaces the entire page with an error screen.

**Fix:** Pick one strategy and apply it everywhere. The in-component `Result` approach (used by the other three routes) is preferable because it keeps the page chrome intact and shows a dismissable toast, which is more appropriate for a cash register UI. Convert `order.tsx`'s loader to return the result rather than throw.
**Status:** DONE

---

### 14 — RowComponentCompositionStrategyInconsistent

**File:** `ui/app/routes/order-overview/components/order-row.tsx` (lines 1–27) vs `ui/app/routes/articles/components/article-row.tsx` (lines 1–39)
**Severity:** Minor
**Category:** Idioms

`OrderRow` couples itself to the router by calling `useNavigate()` internally — the navigation destination is invisible to the caller. `ArticleRow` takes an `onEdit` callback prop — the parent decides what happens on interaction. These are two different philosophies applied to equivalent components. The mixed strategy makes the components harder to test and reuse.

**Fix:** Align both rows to the callback pattern. Replace `useNavigate` in `OrderRow` with an `onSelect` prop passed down from `OrdersTable`. This keeps routing concerns in the route layer, not in leaf components.
**Status:** DONE

---

### 15 — CartStateMapCloningOnEveryMutation

**File:** `ui/app/routes/order/order.tsx` (lines 82–115)
**Severity:** Minor
**Category:** Performance

`addToCart`, `decreaseQuantity`, and `removeFromCart` each clone the entire `Map` via `new Map(prev)` inside `setCart` updaters. For a typical article catalog this is negligible, but the pattern also scatters three independent mutation functions across the component where a single `useReducer` would be cleaner and testable.

**Fix:** Replace the three state-update functions and the `useState<Map<...>>` with a `useReducer`:
```ts
type CartAction =
  | { type: "add"; article: ArticleListItemDto }
  | { type: "decrease"; articleId: string }
  | { type: "remove"; articleId: string };

function cartReducer(state: Map<string, CartEntry>, action: CartAction): Map<string, CartEntry> { ... }
```
The reducer is a pure function that can be unit-tested directly.
**Status:** DONE

---

## Summary

**Finding count by severity:** 5 Major, 10 Minor, 0 Critical.

### Most systemic issues

1. **No HTML5 landmark elements in page content (findings 1–3, 7).** Every route uses generic `<div>` for the content area, footer bar, and navigation toolbar. The missing `<main>`, `<footer>`, and `<nav>` elements affect all six routes simultaneously. Finding 7 (duplicated shell) is the structural root cause: once a shared `PageLayout` is extracted, the landmark elements can be added in one place and propagate everywhere automatically.

2. **Duplicated style primitives (findings 8–9).** The primary button class string and the text input class string are copy-pasted across 7+ and 3+ locations respectively. This is a Tailwind anti-pattern — the framework provides `@apply` or component extraction precisely to avoid this. Any future restyling (e.g. changing the brand colour from blue to a custom token) requires editing every file.

3. **Inconsistent data contracts across loaders and actions (findings 12–13).** The loader error strategy diverges between `order.tsx` (throw → ErrorBoundary) and every other route (return Result → toast). Action return shapes diverge across all three action-bearing routes. This inconsistency makes the codebase harder to reason about as the route count grows.

### Recommended resolution order

1. **Finding 7 (PageLayout) + Findings 1–3 (landmarks)** — extract the shared shell first, then add `<main>`, `<footer>`, `<nav>` in one place. High impact, low risk.
2. **Findings 12–13 (loader/action consistency)** — pick one strategy and apply it. Prevents the pattern from spreading to future routes.
3. **Finding 4 (inaccessible OrderRow) + Finding 5 (unassociated BulkRow labels)** — targeted fixes with immediate accessibility benefit.
4. **Findings 8–9 (style duplication) + Findings 10–11 (composability)** — deduplication pass after structural changes settle.
5. **Findings 6, 14, 15** — lower-priority polish.

---

## Work Summary

### Resolutions applied

**Findings 1–3, 7 — HTML5 landmark elements and page shell (Major/Minor)**
The outer flex container (`div.flex.h-screen.flex-col`) was moved into `root.tsx`'s `Root` component — it is truly shared infrastructure and the natural home for it is the root layout, not a separate component. Each route inlines its own `<header><h1>` directly, since the title is route-specific content with no common logic to abstract. Inside each route, the primary content area was changed from `<div>` to `<main>`, the bottom bar to `<footer>`, and the articles toolbar to `<nav aria-label="Article actions">`. A `PageLayout` component was considered and rejected: it only wrapped `div.flex.h-screen.flex-col` + `<header>`, while `<main>`, `<footer>`, and `<nav>` remained in each route — the abstraction boundary added no meaningful reduction in duplication.

**Finding 4 — Inaccessible clickable table row / Finding 14 — Inconsistent row composition (Major/Minor)**
`OrderRow` was replaced entirely. The `useNavigate` call and `onClick` on `<tr>` were removed. Each `<td>` now wraps its content in `<Link to={to}>` where `to` is a new prop supplied by `OrdersTable`. This gives keyboard users a focusable, activatable element with an `href`, and makes the navigation destination explicit in the parent rather than hidden inside the leaf component — consistent with how `ArticleRow` exposes `onEdit` to its parent.

**Finding 5 — Unassociated BulkRow labels (Major)**
`BulkRow` imports `useId` from React and generates unique IDs per instance. Each `<label>` now carries a `htmlFor` that matches the `id` of its corresponding `<input>`. Two new tests assert `screen.getByLabelText("Description")` and `screen.getByLabelText("Price (cents)")`.

**Finding 6 — Toast container missing live region (Minor)**
The `ErrorMessageList` container gained `role="log"`, `aria-live="polite"`, and `aria-label="Notifications"`. `role="log"` is the correct ARIA pattern for a notification list; it announces additions politely without interrupting the user on every item. `role="alert"` was removed from `ErrorMessageItem` — individual items inside a `role="log"` region do not need their own `role="alert"` and doing so was causing overly aggressive interruptions for screen reader users.

**Finding 8 — Duplicated button styles / Finding 9 — Duplicated input styles (Minor)**
Four Tailwind `@layer components` utilities were added to `app.css`: `.btn-primary`, `.btn-secondary`, `.btn-outline`, and `.input-field`. Every button and input across all routes and components was updated to use these classes. The long class strings (22–60 characters each) are now single tokens. A future brand-colour change requires editing one CSS declaration rather than seven files.

**Finding 10 — Duplicated CartEntry interface (Minor)**
`CartEntry` is now exported from `order.tsx` and imported by `order-summary.tsx` via `import type { CartEntry } from "../order"`. The duplicate definition in `order-summary.tsx` was deleted.

**Finding 11 — Duplicated loader error effect (Minor)**
A `useLoaderError` hook was extracted to `app/components/use-loader-error.ts`. It accepts any `Result<unknown>` and calls `addError` if the result is not ok. The identical three-line `useEffect` blocks that existed in `articles.tsx`, `order-overview.tsx`, `order-view.tsx`, and `order.tsx` were each replaced with a single `useLoaderError(loaderData)` call.

**Finding 12 — Inconsistent clientAction return shapes (Major)**
`order.tsx` was hand-rolling `{ ok: true as const }` and `{ ok: false as const, message }`. The `clientAction` was simplified to `return deps.apiClient.post(...)` directly — the `Result<T>` already has the right shape. `articles-bulk.tsx` was returning `{ message: string }` on failure; it now imports `failure` from `@cashregister/result` and returns `failure({ message, status: 400 })`. All three action-bearing routes now consistently return `Result<T>` or a redirect.

**Finding 13 — Inconsistent loader error strategy (Major)**
`order.tsx` was throwing `new Response(...)` from `clientLoader`, which routed errors through the `ErrorBoundary` and replaced the entire page. The other three routes return the `Result` directly and show a dismissable toast. The `order.tsx` loader was changed to `return deps.apiClient.get(...)` and the component now extracts articles via `loaderData.ok ? loaderData.value.items : []`, with `useLoaderError(loaderData)` for error display. All loaders are now uniform.

### Refactorings performed

**Finding 15 — Cart state useReducer (Minor)**
The three `setCart` closures (`addToCart`, `decreaseQuantity`, `removeFromCart`) and the `useState<Map>` were replaced with `useReducer(cartReducer, new Map())`. The exported `cartReducer` is a pure function — it receives the current state and a `CartAction` and returns the next state. It supports `add`, `decrease`, `remove`, and `clear` actions. The component dispatch calls are inline lambdas at the call sites. The reducer is unit-tested directly in `order.test.tsx` (7 new test cases), independent of the React component tree.

### Tests

All 15 findings are resolved. 2 new files were created: `use-loader-error.ts`, `use-loader-error.test.tsx`. 6 existing test files were updated to match changed component APIs (loader/action shapes, ARIA roles, navigation approach). The test suite grew from 156 tests across 23 files to 167 tests across 24 files. All pass with no errors; one pre-existing lint warning in `article-form.tsx` (missing `useEffect` dependency) was present before this work and is not introduced by these changes.
