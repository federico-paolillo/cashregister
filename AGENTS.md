# Cash Register - Development Guide

This document provides development guidelines and project structure information for the Cash Register application.

## Project Structure

```
cashregister/
├── Cashregister.Api/           # ASP.NET Core Minimal API
├── Cashregister.Application/   # Business logic and transactions
├── Cashregister.Database/      # Entity Framework Core persistence
├── Cashregister.Domain/        # Domain models and value objects
├── Cashregister.Tests.*/       # Test projects
├── escpos/                     # ESC/POS printer library
└── ui/                         # React frontend
```

## Frontend (ui/)

The frontend uses **React Router v7 Framework mode** with Vite for a modern file-based routing experience.

### Technology Stack

- React 19
- React Router v7 (Framework mode)
- TypeScript
- Vite
- Tailwind CSS v4
- Vitest with React Testing Library

### Directory Structure

```
ui/
├── app/
│   ├── result.ts              # Result<T> type (ok/error union)
│   ├── api-client.ts          # ApiClient class
│   ├── deps.ts                # Composition root (single entry point for all dependencies)
│   ├── env.d.ts               # Vite environment variable types
│   ├── model.ts               # All shared DTO types
│   ├── routes.ts              # Route definitions (registers all routes)
│   ├── root.tsx               # Root layout component
│   ├── components/            # Cross-cutting, reusable components only
│   │   ├── modal.tsx          # Generic modal dialog wrapper
│   │   ├── spinner.tsx        # Loading spinner overlay
│   │   ├── use-modal.tsx      # Modal state hook and context
│   │   ├── error-message-list.tsx   # Error toast list (uses ErrorMessageItem)
│   │   ├── error-message-item.tsx   # Single error toast item
│   │   └── use-error-messages.tsx   # Error message state hook and context
│   └── routes/                # Route components – one folder per route
│       ├── home/
│       │   └── home.tsx       # Home page (/)
│       ├── articles/
│       │   ├── articles.tsx   # Articles route (/articles)
│       │   └── components/    # Components used only by this route
│       │       ├── article-form.tsx
│       │       ├── article-row.tsx
│       │       └── articles-table.tsx
│       ├── articles-bulk/
│       │   ├── articles-bulk.tsx   # Bulk article creation (/articles/bulk)
│       │   └── components/
│       │       └── bulk-row.tsx
│       └── order/
│           └── order.tsx      # Order creation (/order)
├── react-router.config.ts     # React Router configuration
├── vite.config.ts             # Vite configuration
├── tsconfig.json              # TypeScript configuration (defines @cashregister/* paths)
└── package.json               # Dependencies and scripts
```

### Component Layout Rules

The folder structure enforces a clear ownership model:

1. **One component per file** — every `.tsx` file exports exactly one React component. Do not define multiple components in the same file.

2. **Route folder** — each route in `routes.ts` lives in its own subfolder under `app/routes/`. The route file name matches the folder name (e.g. `routes/articles/articles.tsx`).

3. **Route-specific components** — components that are only used by a single route live in a `components/` subfolder next to that route (e.g. `routes/articles/components/article-form.tsx`). They are imported with the full `@cashregister/routes/<route>/components/<name>` path.

4. **Cross-cutting components** — components used by more than one route, or by `root.tsx`, live in `app/components/`. These are generic utilities with no route-specific knowledge.

**Decision guide: where does a new component go?**
- Used only in route `foo`? → `routes/foo/components/<component-name>.tsx`
- Used in multiple routes, or in the root layout? → `components/<component-name>.tsx`

**Example:** adding `BulkRow` used only in the bulk articles route:
```
routes/articles-bulk/components/bulk-row.tsx   ✓
components/bulk-row.tsx                        ✗  (not cross-cutting)
routes/articles-bulk/articles-bulk.tsx         ✗  (must be its own file)
```

### React Router Framework Mode

The frontend is configured as a **Single Page Application (SPA)** with `ssr: false` in `react-router.config.ts`. This means:

- No server-side rendering
- Client-side routing only
- Static HTML shell generated at build time

### Adding New Routes

1. Create a new folder and route file in `app/routes/`:
   ```tsx
   // app/routes/about/about.tsx
   export default function About() {
     return <h1>About Page</h1>;
   }
   ```

2. Register the route in `app/routes.ts`:
   ```ts
   import { type RouteConfig, route } from "@react-router/dev/routes";

   export default [
     route("/", "routes/home/home.tsx"),
     route("/about", "routes/about/about.tsx"),
   ] satisfies RouteConfig;
   ```

3. Place any components specific to this route under `app/routes/about/components/`:
   ```tsx
   // app/routes/about/components/about-card.tsx
   export function AboutCard() { ... }
   ```

### NPM Scripts

```bash
npm run dev        # Start development server
npm run build      # Build for production
npm run start      # Serve production build
npm run lint       # Run ESLint
npm run typecheck  # Generate types and run TypeScript check
npm run test       # Run tests once (vitest run)
npm run test:watch # Run tests in watch mode (vitest)
```

### API Client

The frontend uses a `fetch()`-based API client located in `app/api/`. It implements a lightweight `Result<T>` pattern mirroring the backend's approach.

#### Usage

Import the singleton `apiClient` directly wherever needed — in `clientLoader`, `action`, components, or any other module:

```ts
import { deps } from "~/deps";

export async function clientLoader() {
  const result = await deps.apiClient.get<ArticlesPage>("/articles");
  if (!result.ok) throw new Response(result.error.message, { status: result.error.status });
  return result.value;
}
```

The `deps` object is the application's **Composition Root** (`app/deps.ts`), inspired by [Mark Seemann's Pure DI](https://blog.ploeh.dk/2014/06/10/pure-di/) approach. It is the single place where configuration is parsed and all root-level dependencies are constructed. ES module evaluation ensures `deps.ts` runs once and the result is cached — this is the only file that relies on that mechanism.

#### Result<T> Pattern

All client methods return `Promise<Result<T>>` — a discriminated union:

```ts
// Success
{ ok: true, value: T }

// Failure (HTTP error or network error)
{ ok: false, error: { status: number, message: string } }
```

Network errors use `status: 0`. For HTTP errors the `message` contains the URL that failed.

#### Available Methods

- `apiClient.get<T>(path, params?)` — GET request with optional query parameters
- `apiClient.post<T>(path, body?)` — POST request with JSON body
- `apiClient.del(path)` — DELETE request (returns `Result<void>`)

#### Configuration

The backend URL is configured via the `VITE_API_BASE_URL` Vite environment variable. When empty (the default), requests use relative paths and resolve against the current origin — which is the production behavior where frontend and backend share the same host. For local development with a separate backend, set the variable:

```bash
VITE_API_BASE_URL=http://localhost:5000 npm run dev
```

### Styling (Tailwind CSS)

The frontend uses **Tailwind CSS v4** with the `@tailwindcss/vite` plugin. Configuration:

- The Vite plugin is registered in `vite.config.ts` (before `reactRouter()`)
- The global stylesheet `app/app.css` imports Tailwind via `@import "tailwindcss"`
- `app/app.css` is imported in `app/root.tsx` so styles are available to all routes
- Tailwind v4 uses CSS-first configuration — customize themes and utilities directly in `app/app.css` using `@theme` directives rather than a `tailwind.config.js` file

### Testing (Vitest)

The frontend uses **Vitest** with **jsdom** for unit and component testing. Configuration lives in `vite.config.ts` under the `test` key (using `defineConfig` from `"vitest/config"`).

- Test files follow the pattern `app/**/*.test.{ts,tsx}`
- **React Testing Library** (`@testing-library/react`) and **user-event** (`@testing-library/user-event`) are available for component tests
- Use Vitest's built-in `expect` matchers — no additional matcher libraries are installed

#### Writing Tests

```ts
import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";

describe("MyComponent", () => {
  it("renders", () => {
    render(<MyComponent />);
    expect(screen.getByText("hello")).toBeDefined();
  });
});
```

### Generated Files

The `.react-router/` directory contains auto-generated type definitions. This directory is:
- Git-ignored
- Regenerated on `npm run dev` or `npm run typecheck`
- Required for TypeScript type safety with routes

### Error Management System

The frontend has a context-based error message system in `ui/app/components/`. It consists of three files:

- **`use-error-messages.tsx`** — State management hook and React context provider
- **`error-message-list.tsx`** — Renders the stack of error toasts (uses `ErrorMessageItem`)
- **`error-message-item.tsx`** — Presentational component for a single error toast

#### Architecture

`ErrorMessagesProvider` wraps the app and exposes `addError(message)` and `dismissError(id)` via `useErrorMessages()`. Internally the state logic lives in `useErrorMessagesState`, which is also exported so tests can exercise it directly without a provider.

#### Key design decisions

- **`useRef` for timers** — The auto-dismiss timer map (`timers`) is stored in a `useRef`, not `useState`, because nothing in the render output depends on it. Updating it should not trigger a re-render.
- **`useRef` for the ID counter** — Same reasoning: `nextId` is internal bookkeeping only.
- **FIFO eviction** — When `maxMessages` is exceeded the oldest error is shifted off and its timer is cancelled.
- **Cleanup on unmount** — A `useEffect` cleanup function clears all pending timers to avoid firing `setState` on an unmounted component.
- **Configurable behaviour** — `autoDismissMs` (default 5 000 ms) and `maxMessages` (default 5) are props on the provider. Setting `autoDismissMs` to 0 disables auto-dismiss.

#### Testing

Tests live alongside the source files (`use-error-messages.test.tsx`, `error-message-list.test.tsx`). They use `vi.useFakeTimers()` to exercise the auto-dismiss and eviction paths deterministically.

## Backend

### Technology Stack

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core with SQLite
- C# 14

### Architecture

The backend follows Clean Architecture principles:

1. **Domain** - Core business entities and value objects
2. **Application** - Business logic, transactions, and use cases
3. **Database** - EF Core persistence and queries
4. **API** - HTTP endpoints and route handlers

### Key Patterns

- **Result Pattern** - `Result<T>` for operation outcomes (no exceptions for flow control)
- **Transaction Pattern** - `Transaction<TInput, TOutput>` for business operations
- **Value Objects** - `Identifier`, `Cents`, `OrderNumber`
- **Immutability** - Records with init-only properties
- **Collection Safety** - `ImmutableArray<T>` for domain models

### Running the Backend

```bash
dotnet build
dotnet run --project Cashregister.Api
```

### Running Tests

```bash
dotnet test
```

## Development Workflow

1. Backend changes: Work in the appropriate layer following Clean Architecture
2. Frontend changes: Add routes in `app/routes/` and register in `app/routes.ts`
3. Run tests before committing
4. Follow conventional commit format (e.g., `feat:`, `fix:`, `chore:`)

## Guidelines specifically for AI agents

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.
