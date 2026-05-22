# Cash Register Architecture

> This document describes the high-level architecture of Cash Register. It intentionally does not document ESC/POS command details. Printer instruction, encoder, emulator, CLI, and file-device details live in [`ESCPOS.md`](ESCPOS.md).

## System Shape

Cash Register is a small order-taking application. It does not track money or payments; it records articles and orders, and it has printer infrastructure for receipt output.

The repository has two main application roots:

```text
be/   .NET backend, database, domain, printer libraries, CLI, and tests
ui/   React frontend, routing, API client, styling, and tests
```

The intended deployment target is a constrained Linux device. Keep the runtime simple: SQLite for local persistence, ASP.NET Core Minimal APIs, a React single-page app, and file-device printer output.

## Backend

The backend solution is `be/Cashregister.slnx`. Shared compiler settings live in `be/Directory.Build.props`: .NET 10, C# 14, nullable reference types, implicit usings, latest analysis, and warnings as errors.

Current projects:

```text
Cashregister.Domain                 Domain models and value objects
Cashregister.Commons                Result, Problem, Transaction, UnitOfWork, Scoped helpers
Cashregister.Application            Use cases, handlers, transactions, pagination, receipt program service, and device registration
Cashregister.Database               EF Core SQLite persistence, queries, commands, mappers, migrations
Cashregister.Api                    ASP.NET Core Minimal API composition root and HTTP endpoints
Cashregister.Activities             Cross-transaction orchestration experiments; not currently API-wired
Cashregister.Printmon               ESC/POS print program model, encoders, and file device abstraction
Cashregister.Printmon.Emulator      ESC/POS binary emulator, Markdown renderer, and development markdown device
Cashregister.Cli                    Printmon developer CLI
Cashregister.Tests.Integration      API, database, application, and integration tests
Cashregister.Printmon.Tests         Printmon encoder and builder tests
Cashregister.Printmon.Emulator.Tests Emulator tests
```

Dependency direction is intentionally simple:

- `Domain` has no dependency on application, database, API, or printing code.
- `Commons` contains cross-cutting primitives used by multiple backend projects.
- `Printmon` depends on `Commons` for `Result<T>` and `Unit`; it does not depend on the application or database.
- `Application` depends on `Domain`, `Commons`, `Printmon`, and `Printmon.Emulator` for receipt program construction and device registration.
- `Database` depends on `Application`, `Domain`, and `Commons` to implement application ports.
- `Api` is the composition root: it depends on the backend projects it wires together and maps them to HTTP.

`Application` defines use cases and ports. `Database` implements persistence ports using EF Core. `Api` wires services, configuration, devices, persistence, and HTTP route modules.

## Domain and Application

Domain models are plain C# types and value objects. Important value objects include `Identifier`, `Cents`, `OrderNumber`, and `TimeStamp`. `Cents` stores exact non-negative integer cents and does not round. Domain collections use immutable arrays where aggregate state must not be mutated by consumers.

Application behavior is organized by feature area:

```text
Application/Articles
Application/Orders
Application/Receipts
Application/Statistics
Application/Pagination
```

Expected business and application failures use `Result<T>` plus `Problem`, not exceptions. Transactions derive from `Transaction<TInput, TOutput>`, receive an `IUnitOfWork`, and save changes only when the result is successful. Default implementations of interfaces live under `Defaults/` folders.

Queries and commands are interfaces in Application and implementations in Database. Handlers such as `FetchOrdersPageHandler` compose application queries and pagination behavior without depending on HTTP.

## Persistence

Persistence is implemented with EF Core and SQLite in `Cashregister.Database`.

`ApplicationDbContext` implements both `IApplicationDbContext` and `IUnitOfWork`. Starting and rolling back are no-ops for EF because a scoped `DbContext` begins tracking on construction and discards unsaved changes on disposal. `SaveChangesAsync` commits successful transactions.

Money columns such as `ArticleEntity.Price`, `OrderItemEntity.Price`, and `OrderEntity.TotalOverride` are stored as `long` cents. Their database names are intentionally domain-oriented, while API DTO names use explicit `*InCents` suffixes. Articles also persist `PrintDetailReceipt`, the current article-level selection for whether order printing emits per-unit detail receipts, and nullable `QuantityAvailable`, a manually editable soft availability balance disabled when unset.

The database connection is configured from `DataSource`. At runtime, `be/Cashregister.Api/Program.cs` adds environment variables with the `CASHREGISTER_` prefix, so `CASHREGISTER_DATASOURCE` supplies this value. The development launch profile sets it to `cashregister.db`.

SQLite connection PRAGMAs are applied by `SqlitePragmasDbConnectionInterceptor` when connections open:

- WAL journaling
- one-second busy timeout
- normal synchronous mode
- 20 MiB cache
- foreign keys enabled
- memory temp store

The API applies EF Core migrations on startup before serving requests.

## API

The backend API is an ASP.NET Core Minimal API in `Cashregister.Api`.

`Program.cs`:

- configures simple single-line console logging;
- loads `CASHREGISTER_` environment variables;
- configures printer-device settings through the registered device extension methods;
- registers database, articles, orders, receipts, and printer-device services;
- maps article, order, and device route modules;
- applies database migrations;
- preselects the first discovered printer target when one is available;
- starts the web application.

Route modules follow the same pattern:

```text
<Feature>/Endpoints.cs    route-group registration
<Feature>/Handlers.cs     static Minimal API handler methods
<Feature>/Models/         HTTP DTOs
```

Current backend route groups:

```text
/articles
/orders
/devices
/statistics
```

The backend does not map `/api/*`. `/api` is a frontend development and deployment convention.

The API money contract is cents-only. Request and response DTO fields such as `priceInCents`, `totalInCents`, and `totalOverrideInCents` carry integer cents, not decimal currency amounts.

## Frontend

The frontend lives in `ui/` and uses:

- React 19
- React Router v7 in framework mode with `ssr: false`
- TypeScript
- Vite
- Tailwind CSS v4 through `@tailwindcss/vite`
- Vitest with jsdom and React Testing Library

Routes are registered in `ui/app/routes.ts`:

```text
/                 routes/order/order.tsx
/articles         routes/articles/articles.tsx
/articles/bulk    routes/articles-bulk/articles-bulk.tsx
/devices          routes/devices/devices.tsx
/orders           routes/order-overview/order-overview.tsx
/statistics       routes/statistics/statistics.tsx
```

`ui/app/root.tsx` provides the shared layout, navigation menu, error message provider, error list, hydrate fallback, and route error boundary.

`ui/app/deps.ts` is the frontend composition root. It parses settings and constructs the singleton `ApiClient`. Most route loaders and actions import `deps` instead of constructing dependencies directly.

`ui/app/settings.ts` defaults `apiBaseUrl` to `/api`. In development, `ui/vite.config.ts` proxies `/api/*` to `http://localhost:5122` and rewrites the prefix away. Therefore browser code calls paths such as `/api/orders`, while the backend receives `/orders`.

Frontend DTOs live in `ui/app/model.ts`. The client-side `Result<T>` mirrors the backend's explicit success/failure style.

Frontend money display and entry are decimal-only for users. `ui/app/money.ts` owns exact conversion between backend cents and decimal strings, and shared money entry uses `ui/app/components/money-input.tsx` so visible decimal inputs submit hidden integer cent fields.

## Printing

Receipt printing is split across application and Printmon code.

Application receipt code builds `PrintProgram` instances through `IReceiptPrintProgramService`. `PrintProgram` is the portable representation of what should be printed. Actual ESC/POS command records, encoders, emulator behavior, CLI tooling, and file-device rules are documented in [`ESCPOS.md`](ESCPOS.md).

The API always wires `FileDeviceTargetStore`, the file-printer catalog, and `BinaryEncoder` for device selection and ESC/POS encoding. On startup it selects the first catalog printer when one is available; manual selection can change that runtime target later. Outside development it registers `FileDevice`, which writes encoded bytes to the selected filesystem target and fails when no target is selected. In development it registers `MarkdownDevice`, which runs the emulator pipeline and writes rendered markdown receipts to files under the configured root folder.

`IPrintReceiptHandler` orchestrates receipt printing by asking `IReceiptPrintProgramService` to build print programs for an order id and then sending each program to the configured `IDevice`. `PlaceOrderActivity` uses independent scoped operations to place an order, print its receipt, and fetch the saved order for the API response. Receipt reprinting remains exposed as an explicit order action at `POST /orders/{id}/print`.

Order receipt printing always emits one priced overview receipt. Per-unit item detail receipts are emitted only for ordered articles whose current `PrintDetailReceipt` selection is enabled, so article edits affect later prints and reprints without changing the overview.

## Testing

Backend tests are split by concern:

- `Cashregister.Tests.Integration` covers API endpoints, application transactions and handlers, database queries, pagination, and receipt program service behavior.
- `Cashregister.Printmon.Tests` covers Printmon builder and encoder behavior.
- `Cashregister.Printmon.Emulator.Tests` covers decoder, executor, emulator, and Markdown rendering behavior.

Frontend tests are colocated with source files under `ui/app/**/*.test.{ts,tsx}` and run with Vitest. Route tests mock `deps.apiClient` where needed and exercise loaders, actions, and components.

## Operational Notes

Backend verification runs from `be/`:

```bash
dotnet format
dotnet build
dotnet test
```

Frontend verification runs from `ui/`:

```bash
npm run lint
npm run build
npm run test
```

For local development, run the backend on the development launch profile and the frontend with Vite. The frontend uses `/api` and the dev proxy to reach the backend. In deployment, the frontend and backend should be served so that `/api/*` reaches the backend route groups after the prefix is stripped or otherwise routed equivalently.
