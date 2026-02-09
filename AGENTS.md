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
│   ├── deps.ts             # Composition root (single entry point for all dependencies)
│   ├── env.d.ts            # Vite environment variable types
│   ├── routes.ts           # Route definitions
│   ├── root.tsx            # Root layout component
│   └── routes/             # Route components (file-based routing)
│       └── home.tsx        # Home page component (/)
├── react-router.config.ts  # React Router configuration
├── vite.config.ts          # Vite configuration
├── tsconfig.json           # TypeScript configuration
└── package.json            # Dependencies and scripts
```

### React Router Framework Mode

The frontend is configured as a **Single Page Application (SPA)** with `ssr: false` in `react-router.config.ts`. This means:

- No server-side rendering
- Client-side routing only
- Static HTML shell generated at build time

### Adding New Routes

1. Create a new component in `app/routes/`:
   ```tsx
   // app/routes/about.tsx
   export default function About() {
     return <h1>About Page</h1>;
   }
   ```

2. Register the route in `app/routes.ts`:
   ```ts
   import { type RouteConfig, route } from "@react-router/dev/routes";

   export default [
     route("/", "routes/home.tsx"),
     route("/about", "routes/about.tsx"),
   ] satisfies RouteConfig;
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

## Guidelines for AI-Assisted Development

When working with AI agents on this project:

1. **Preserve Architecture** - Maintain the clean layered architecture (Domain → Application → Database → API)
2. **Follow Patterns** - Respect existing patterns (Result monad, Transaction pattern, Unit of Work)
3. **Test Coverage** - Ensure existing tests continue to pass
4. **Modern Idioms** - Adopt new language features where they improve clarity
5. **No Over-Engineering** - Keep changes focused and minimal
6. **Documentation** - Update this file for significant contributions

## AI Agent Contributions

This section tracks significant contributions made by AI agents to the Cash Register project.

### .NET 10 Upgrade (2026-01-17)

**Agent**: Claude (Anthropic)
**Branch**: `claude/upgrade-dotnet-10-taGNP`
**Commits**: `1e190b6`, `cb53713`

#### Scope
Complete upgrade of the Cash Register backend from .NET 9.0 to .NET 10 with adoption of modern C# 14 features and new tooling formats.

#### Changes Made

**Framework & SDK**
- Updated `global.json` from .NET SDK 9.0.300 → 10.0.100
- Updated `Directory.Build.props` target framework from net9.0 → net10.0
- Added explicit C# language version 14 configuration

**NuGet Packages (9.x → 10.0.0)**
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.NET.Test.Sdk (17.12.0 → 18.0.0)

**Tooling**
- Updated dotnet-ef tool from 9.0.5 → 10.0.0
- Converted solution from legacy `.sln` to new `.slnx` XML format

**Code Modernization (C# 14 Collection Expressions)**
- `Cashregister.Database/Mappers/OrderEntityMapper.cs` - Replaced `.ToList()` with collection expression spread
- `Cashregister.Application/Orders/Transactions/Defaults/PlaceOrderTransaction.cs` - Array initialization
- `Cashregister.Database/Queries/FetchArticlesQuery.cs` - LINQ to array conversions (2 locations)
- `Cashregister.Tests.Integration/Articles/FetchArticlesListQueryTests.cs` - Test data collection

**Pattern Applied**
```csharp
// Before
var items = source.Select(x => transform(x)).ToArray();

// After
var items = [.. source.Select(x => transform(x))];
```

#### Impact
- Zero breaking changes to public APIs
- Maintained backward compatibility with existing functionality
- Improved code consistency with modern C# idioms
- Better performance through optimized collection expressions
- Future-proofed for .NET 10 features and ecosystem

#### Testing Strategy
All existing integration tests remain functional. The upgrade preserves:
- Article management endpoints
- Order processing workflows
- Database query operations
- Entity Framework Core migrations
