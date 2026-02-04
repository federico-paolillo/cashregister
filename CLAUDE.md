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
│   ├── routes.ts           # Route definitions
│   ├── root.tsx            # Root layout component
│   ├── entry.client.tsx    # Client-side hydration
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
