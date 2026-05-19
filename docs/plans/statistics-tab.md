# Statistics Navigation Tab

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md` from the repository root.

## Purpose / Big Picture

Cash Register records articles and orders but currently has no aggregate view of what was sold. After this change, a user can open the new Statistics tab, inspect all-time article sales and order-volume totals, and download each table as a CSV file. The feature is observable through `GET /statistics`, `GET /statistics/articles.csv`, `GET /statistics/orders.csv`, and the frontend route `/statistics`.

## Progress

- [x] (2026-05-19 00:00Z) Created this ExecPlan with the fixed API contract and statistics semantics.
- [x] (2026-05-19 00:00Z) Add backend statistics application models and query port.
- [x] (2026-05-19 00:00Z) Implement EF Core statistics query and register it in database DI.
- [x] (2026-05-19 00:00Z) Add API statistics routes, DTOs, and CsvHelper export.
- [x] (2026-05-19 00:00Z) Add frontend statistics route, navigation tab, DTOs, CSV links, and tests.
- [x] (2026-05-19 00:00Z) Update architecture and diary documentation.
- [x] (2026-05-19 00:00Z) Run backend and frontend verification.

## Surprises & Discoveries

- Observation: Active articles with no matching order items need explicit nullable sums in the EF projection.
  Evidence: The implementation now uses nullable aggregate projections with `?? 0L`, and `FetchAsync_ReturnsActiveArticlesWithZeroSales` verifies the behavior.

## Decision Log

- Decision: The first version is all-time only.
  Rationale: Date filters would require extra UI, query parameters, and timezone policy that were not requested for this first version.
  Date/Author: 2026-05-19 / Codex

- Decision: The per-article table excludes retired articles, but order-level totals include all historical orders and items.
  Rationale: The user explicitly wants retired articles excluded from the article table while preserving order totals as historical order value.
  Date/Author: 2026-05-19 / Codex

- Decision: Per-article volume uses historical order-item price, not current article price.
  Rationale: Order items snapshot description and price at sale time, so statistics should not change when an article is edited later.
  Date/Author: 2026-05-19 / Codex

- Decision: Delta is real volume minus nominal volume.
  Rationale: Discounts become negative and markups become positive, which makes direction explicit.
  Date/Author: 2026-05-19 / Codex

- Decision: CSV export uses two files.
  Rationale: Each table remains rectangular and each download mirrors one visible table.
  Date/Author: 2026-05-19 / Codex

## Outcomes & Retrospective

Completed. The Statistics tab renders all-time article sales and order-volume summaries, and the API exposes JSON plus two CSV downloads. Backend format/build/test, frontend lint/build/test, and a browser smoke check of `/statistics` passed.

## Context and Orientation

The backend lives under `be/` and uses ASP.NET Core Minimal APIs, EF Core, SQLite, and application/query interfaces. Current route modules live under `be/Cashregister.Api/<Feature>/` with `Endpoints.cs`, `Handlers.cs`, and `Models/`. Database query implementations live under `be/Cashregister.Database/Queries/` and are registered in `be/Cashregister.Database/Extensions/ServiceCollectionExtensions.cs`.

Article persistence uses `ArticleEntity.Retired` with an EF global query filter that hides retired articles from `dbContext.Articles`. Order items snapshot `Description`, `Price`, and `Quantity`, so historical sales statistics must aggregate `OrderItemEntity`, not current article price. `OrderEntity.TotalOverride` stores an optional order-level override; when present it is the real order total.

The frontend lives under `ui/` and uses React Router framework mode. Routes are registered in `ui/app/routes.ts`; shared layout and navigation live in `ui/app/root.tsx` and `ui/app/components/navigation-menu.tsx`; DTOs live in `ui/app/model.ts`; API calls go through `deps.apiClient`.

## Plan of Work

Add `Application/Statistics` with immutable output models for the full statistics payload, article rows, article totals, order summary row, and order summary totals. Add `IFetchStatisticsQuery` as the application port. This is a read-only feature, so no transaction or unit-of-work save path is required.

Implement `FetchStatisticsQuery` in `Cashregister.Database`. For article rows, start from `dbContext.Articles.AsNoTracking()` so the existing retired-article filter applies, left join or correlated aggregate over `dbContext.OrderItems`, and return active articles even when their aggregates are zero. For order totals, aggregate all `dbContext.Orders` and their items without joining to active articles. Compute nominal volume as the sum of `OrderItemEntity.Price * Quantity`; compute real volume per order as `TotalOverride` when non-null, otherwise the order nominal total; compute delta as real minus nominal.

Add `Cashregister.Api/Statistics` routes. `GET /statistics` returns JSON. `GET /statistics/articles.csv` returns a CSV attachment for article rows plus one total row. `GET /statistics/orders.csv` returns a CSV attachment for the order summary row plus one total row. Add `CsvHelper` version `33.1.0` to `Cashregister.Api` and use `CsvWriter` with `CultureInfo.InvariantCulture` and typed CSV records.

Add the frontend route `ui/app/routes/statistics/statistics.tsx` and small single-use table components in its folder. The loader calls `/statistics`; the component renders both tables and footer totals; money uses `formatPrice`. Add a public `buildUrl`-style helper to `ApiClient` for CSV links, and use it for two anchor buttons.

Update `docs/ARCH.md` to include the new backend and frontend routes. Append one diary entry to `docs/DIARY.md` after implementation. Do not update `docs/ESCPOS.md` or `docs/CONVENTIONS.md`.

## Concrete Steps

From the repository root, edit the backend files first:

1. Create application models and query interface under `be/Cashregister.Application/Statistics/`.
2. Create `be/Cashregister.Database/Queries/FetchStatisticsQuery.cs` and register it in database DI.
3. Create `be/Cashregister.Api/Statistics/Endpoints.cs`, `Handlers.cs`, and `Models/StatisticsDto.cs`.
4. Add `CsvHelper` to `be/Cashregister.Api/Cashregister.Api.csproj` and map statistics routes in `Program.cs`.
5. Add backend integration tests under `be/Cashregister.Tests.Integration/Statistics/` and `be/Cashregister.Tests.Integration/Api/StatisticsEndpointTests.cs`.

Then edit the frontend:

1. Add statistics DTOs to `ui/app/model.ts`.
2. Add a public URL helper to `ui/app/api-client.ts`.
3. Add the `/statistics` route in `ui/app/routes.ts`.
4. Add the navigation item in `ui/app/components/navigation-menu.tsx`.
5. Create route and tests under `ui/app/routes/statistics/`.

Finally update documentation and run verification.

## Validation and Acceptance

Run backend verification from `be/`:

    dotnet format
    dotnet build
    dotnet test

Run frontend verification from `ui/`:

    npm run lint
    npm run build
    npm run test

Acceptance criteria:

The backend `GET /statistics` returns active article rows, article footer totals, order count, nominal volume, real volume, and delta. Retired articles are not listed in article rows. Historical order items from retired articles still affect the order summary. CSV endpoints return successful responses with CSV content and attachment filenames.

The frontend displays a `Statistics` navigation tab. Opening `/statistics` renders both tables, correct footer totals, and two CSV links. Loader failures surface through the existing error-message provider.

## Idempotence and Recovery

The feature is additive and does not require a database migration. Re-running tests and builds is safe. If a verification command fails, fix only failures caused by this task. Existing unrelated dirty files must not be reverted.

## Artifacts and Notes

CSV export should produce stable headers. Article CSV rows should include article id, article description, sold units, orders included, and volume in cents. Order CSV rows should include order count, nominal volume in cents, real volume in cents, and delta in cents.

## Interfaces and Dependencies

Backend application interface:

    namespace Cashregister.Application.Statistics.Data;

    public interface IFetchStatisticsQuery
    {
        Task<OrderStatistics> FetchAsync();
    }

The exact model names may be adjusted to match implementation style, but the public API JSON fields must include:

    articles.items[].articleId
    articles.items[].description
    articles.items[].soldUnits
    articles.items[].ordersIncluded
    articles.items[].volumeInCents
    articles.totals.soldUnits
    articles.totals.ordersIncluded
    articles.totals.volumeInCents
    orders.orderCount
    orders.nominalVolumeInCents
    orders.realVolumeInCents
    orders.deltaInCents
    ordersTotals.orderCount
    ordersTotals.nominalVolumeInCents
    ordersTotals.realVolumeInCents
    ordersTotals.deltaInCents
