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

### Directory Structure

```
ui/
├── app/
│   ├── api/
│   │   ├── result.ts              # Result<T> type (ok/error union)
│   │   └── api-client.ts          # ApiClient class
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
