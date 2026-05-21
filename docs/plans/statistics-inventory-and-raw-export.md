# Statistics Inventory and Raw Sales Export

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register currently shows all-time statistics that mix screen summaries with CSV summaries. The user needs a more useful statistics view focused on how many articles were produced and what order volume was expected before discounts versus what was real after an overridden order price. They also need CSV exports that can be loaded into Excel and verified from raw rows, without trusting server-side totals.

After this change, `/statistics` shows compact summary cards, an article inventory table, and an order-volume table. The page has one export link, `Sales CSV`, backed by `GET /statistics/sales.csv`. The CSV contains one row per persisted order item, including the order id, item id, article id, current article description, sale-time description, sale-time unit price, quantity, and optional order override. It contains no aggregate rows or totals.

## Progress

- [x] (2026-05-20T19:22:55Z) Read the relevant architecture, conventions, existing statistics plan, backend statistics query, API handlers, frontend route, and tests.
- [x] (2026-05-20T19:22:55Z) Created this ExecPlan with the resolved decisions from planning.
- [x] (2026-05-20T19:49:18Z) Refactored backend statistics output models, EF query, API DTOs, route handlers, and CSV writer.
- [x] (2026-05-20T19:49:18Z) Updated backend integration tests for current article names, retired article inclusion, repricing, override handling, and raw CSV shape.
- [x] (2026-05-20T19:49:18Z) Updated frontend DTOs, `/statistics` UI, CSV link, and route tests.
- [x] (2026-05-20T19:49:18Z) Ran backend and frontend verification.
- [x] (2026-05-20T19:49:18Z) Appended `docs/DIARY.md`.

## Surprises & Discoveries

- Observation: The current database already has the historical facts needed for this refactor.
  Evidence: `OrderItemEntity` stores `ArticleId`, `Description`, `Price`, and `Quantity`, while `OrderEntity` stores optional `TotalOverride`.

- Observation: Naming an Application output model `Statistics` conflicts with the enclosing `Statistics` namespace in C#.
  Evidence: `dotnet build` failed with `CS0118: 'Statistics' is a namespace but is used like a type`; renaming the model to `StatisticsReport` fixed the build.

- Observation: Mapping `OrderDate` twice in one CsvHelper `ClassMap` did not emit both date columns.
  Evidence: statistics CSV tests showed the header was missing `OrderDateUnixSeconds`; projecting to a flattened CSV record restored the planned header.

- Observation: The sandbox blocks Roslyn build-host named-pipe connections.
  Evidence: `dotnet format --no-restore` failed with `SocketException (13): Permission denied /tmp/...`; running the verification command with escalation succeeded.

## Decision Log

- Decision: Use persisted order items as the historical source of truth for sold inventory and expected volume.
  Rationale: Order items snapshot the article price and sale-time description, so later article edits must not rewrite historical volume.
  Date/Author: 2026-05-20 / Codex

- Decision: Display current article descriptions in the UI while keeping sale-time descriptions in the CSV.
  Rationale: The user chose current article names for statistics, but the CSV remains auditable when an article was renamed after sale.
  Date/Author: 2026-05-20 / Codex

- Decision: Export a single raw CSV with integer cents and both Unix and ISO UTC dates.
  Rationale: One line-level export avoids server-side totals and lets Excel recompute article counts and volumes. Integer cents avoid locale-sensitive decimal parsing.
  Date/Author: 2026-05-20 / Codex

- Decision: Do not allocate order-level overrides to article rows.
  Rationale: `TotalOverride` applies to the whole order. Dividing it across articles would invent accounting semantics that are not stored in the system.
  Date/Author: 2026-05-20 / Codex

- Decision: Name the complete Application read model `StatisticsReport`.
  Rationale: It avoids the C# namespace/type collision while preserving clear API DTO names such as `StatisticsDto`.
  Date/Author: 2026-05-20 / Codex

- Decision: Use a flattened CSV record inside the CSV handler.
  Rationale: The Application statistics rows keep domain value objects, while the CSV record gives CsvHelper explicit scalar columns for both `OrderDateUnixSeconds` and `OrderDateUtc`.
  Date/Author: 2026-05-20 / Codex

## Outcomes & Retrospective

Completed. `/statistics` now presents summary metrics, sold-article inventory, and per-order volume rows. `GET /statistics/sales.csv` replaces the old aggregate CSV endpoints with one raw, line-level export that Excel can recompute from integer cents and quantities. No database migration was needed.

## Context and Orientation

The backend solution lives under `be/`. Application-level statistics interfaces and models live under `be/Cashregister.Application/Statistics`. The EF Core implementation lives in `be/Cashregister.Database/Queries/FetchStatisticsQuery.cs`. API routes live under `be/Cashregister.Api/Statistics`. Integration tests live under `be/Cashregister.Tests.Integration/Statistics` and `be/Cashregister.Tests.Integration/Api`.

The frontend lives under `ui/`. Shared TypeScript DTOs live in `ui/app/model.ts`. The `/statistics` route is `ui/app/routes/statistics/statistics.tsx`, with route-local table components under `ui/app/routes/statistics/components`.

Expected volume means the sum of sale-time item prices multiplied by quantities. Real volume means the expected order total unless the order has `TotalOverride`, in which case the override is the real total. Produced articles means the sum of all order item quantities.

## Plan of Work

First, replace the current aggregate-oriented application models with models for article inventory rows, order statistics rows, order summary, and raw CSV rows. Keep `IFetchStatisticsQuery.FetchAsync` as the read model entry point, but change its returned model shape. Add a new CSV handler interface for the single raw sales export and remove the old article/order CSV handler registrations.

Next, rewrite `FetchStatisticsQuery` so it starts from persisted orders and order items. Inventory rows group sold `OrderItemEntity` rows by `ArticleId`, join to `ArticleEntity` with query filters ignored, and display current description plus retired status. Order rows group items by order and compute produced articles, expected volume, real volume, delta, and override status. CSV rows project one row per order item and include both current and sale-time article descriptions.

Then, update the API DTOs and endpoints. `GET /statistics` returns the new shape. `GET /statistics/sales.csv` returns `statistics-sales.csv`. Remove the old `/statistics/articles.csv` and `/statistics/orders.csv` endpoints from the mapped route group.

Finally, update the frontend route to show summary cards, an article inventory table, and an order table. The route loader builds only the sales CSV URL. Update route tests and backend tests to lock the new semantics.

## Concrete Steps

Run backend checks from `be/`:

    dotnet format
    dotnet build
    dotnet test

Observed on 2026-05-20: `dotnet format --no-restore`, `dotnet build`, and `dotnet test` passed when run outside the sandbox because Roslyn/MSBuild build-host pipes were blocked inside the sandbox.

Run frontend checks from `ui/`:

    npm run lint
    npm run build
    npm run test

Observed on 2026-05-20: all three frontend commands passed.

## Validation and Acceptance

Acceptance is met when a user can open `/statistics`, see produced articles, order count, expected volume, real volume, override delta, sold article inventory, and order-volume rows. Downloading `Sales CSV` must return one raw row per order item with integer cents and no totals.

Backend tests must prove that retired sold articles are included, unsold articles are excluded, current article descriptions are displayed after rename, historical prices still drive expected volume after repricing, order overrides drive real volume, and the CSV contains no aggregate fields.

Frontend tests must prove the new page renders the summary, tables, retired marker, and sales CSV link, and that loader failures still surface through the existing error-message system.

## Idempotence and Recovery

The refactor is source-only and does not require a database migration. If a step fails, inspect `git diff`, fix only the statistics-related files, and rerun the same validation commands. Do not run destructive git commands to recover.

## Artifacts and Notes

No verification transcript yet.

Verification summary:

    dotnet format --no-restore
    dotnet build
    dotnet test
    npm run lint
    npm run build
    npm run test

All commands completed successfully. `dotnet test` reported 150 Printmon tests, 106 emulator tests, and 134 integration tests passing. `npm run test` reported 28 test files and 222 tests passing.

## Interfaces and Dependencies

The backend continues to use EF Core and CsvHelper, both already present in the solution. The API remains unprefixed, so the backend endpoint is `/statistics/sales.csv` and the frontend reaches it through `/api/statistics/sales.csv` via the existing API client URL builder.

At the end, `IFetchStatisticsQuery.FetchAsync` returns the new full statistics model. The CSV handler exposed from Application is `IWriteSalesStatisticsCsvHandler.ExecuteAsync(Stream stream, CancellationToken cancellationToken = default)`.
