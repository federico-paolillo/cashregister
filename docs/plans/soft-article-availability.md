# Add soft article availability warnings

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`.

## Purpose / Big Picture

Cashiers need a lightweight stock hint while orders are being taken without turning Cash Register into a hard inventory gate. After this change, selected articles can carry a manually editable available quantity, successful orders reduce that soft balance, and the new-order screen highlights articles whose current cart would leave the balance at or below the configured warning threshold.

## Progress

- [x] (2026-05-22) Inspected article persistence, article API and forms, order placement, order selector and summary UI, configuration hooks, tests, architecture docs, and execution-plan conventions.
- [x] (2026-05-22) Added nullable article availability to backend persistence, contracts, migration, and order-side soft decrement behavior.
- [x] (2026-05-22) Added article availability controls and order warning styling to the frontend with focused tests.
- [x] (2026-05-22) Updated architecture and diary documentation and ran backend and frontend verification.

## Surprises & Discoveries

- Observation: Order placement reads article snapshots through a no-tracking query before creating order items.
  Evidence: `be/Cashregister.Database/Queries/FetchArticlesQuery.cs` calls `AsNoTracking()` and `PlaceOrderTransaction` builds order items from the returned articles.
- Observation: Existing article settings already flow through create, detail fetch, edit form, and bulk rows.
  Evidence: `PrintDetailReceipt` is present in article input models, API DTOs, the edit form, and bulk row serialization.
- Observation: Current configuration only exposes frontend Vite settings for browser-only behavior.
  Evidence: `ui/app/settings.ts` parses `VITE_API_BASE_URL`; the low-quantity threshold can follow that path because it changes warning styling only.
- Observation: Migration generation must use the database design-time factory for this repo.
  Evidence: `dotnet ef migrations add ArticleQuantityAvailable --project Cashregister.Database --startup-project Cashregister.Api` failed because the API project does not reference EF Design, while the same command with `--startup-project Cashregister.Database` succeeded through `ApplicationDbContextFactory`.

## Decision Log

- Decision: Use `QuantityAvailable = null` as the disabled state.
  Rationale: Existing articles must stay disabled after migration, and a nullable quantity avoids maintaining a separate enable flag.
  Date/Author: 2026-05-22 / User and Codex
- Decision: Successful orders decrement enabled quantities and may drive the balance negative.
  Rationale: The feature is advisory for an order-before-fulfillment workflow; oversell should remain representable and should not block an order.
  Date/Author: 2026-05-22 / User and Codex
- Decision: Compute warnings from projected cart balance and configure the threshold from frontend environment with default `5`.
  Rationale: The warning is a live order-taking presentation concern and should react before submission as the cart changes.
  Date/Author: 2026-05-22 / User and Codex
- Decision: Implement decrements in application/database code instead of a SQLite trigger.
  Rationale: The user chose the visible C# path and accepts that concurrent read-modify-write decrements are not a full concurrency-control mechanism.
  Date/Author: 2026-05-22 / User and Codex

## Outcomes & Retrospective

Completed. Articles now round-trip nullable soft availability through detail and list contracts, existing rows stay disabled through a nullable migration, bulk creation and selected article editing can enable or clear quantities, successful order placement subtracts enabled quantities without blocking negative balances, and the new-order UI warns from projected cart balance with a Vite-configured threshold.

## Context and Orientation

Articles are modeled in `be/Cashregister.Domain/Article.cs`, changed through application article transactions, stored by `ArticleEntity`, and exposed through article DTOs under `be/Cashregister.Api/Articles`. The article list API also feeds the new-order selector, so any warning data needed while placing an order must flow through the article list projection.

Order placement starts in `PlaceOrderTransaction`. It fetches article data, snapshots description and price into order items, and saves a pending order under the existing EF unit of work. Soft availability means an editable balance that can become stale or negative; it is a cashier warning signal, not a reservation system.

The selected article edit form lives in `ui/app/routes/articles/components/article-form.tsx`. Bulk creation posts one request per row from `ui/app/routes/articles-bulk`. The order selector and summary live beside `ui/app/routes/order/order.tsx`, which already owns cart quantities and can compute projected remaining availability.

## Plan of Work

First, add nullable signed availability across the article domain, application input/output models, API DTOs, mapper and save paths, entity configuration, and article projections. Generate an EF migration whose new nullable article column leaves existing rows disabled with no backfill.

Next, add a focused order-side data command for availability decrements. The order transaction should call it after article existence is validated and before the unit of work saves changes. The database command should update only enabled article balances and never fail an order because the resulting value is zero or negative.

Then, expose availability controls in selected-article editing and bulk article creation. Add a Vite-parsed warning threshold with default `5`, preserve nullable availability in the article list frontend DTO, and color order selector buttons and summary entries orange whenever enabled availability minus cart quantity is at or below the threshold.

Finally, cover API persistence, order decrements, form serialization, threshold parsing, and warning transitions with tests. Update `docs/ARCH.md` and `docs/DIARY.md`, then run full backend and frontend verification.

## Concrete Steps

Run backend verification from `be/` after backend and migration edits:

    dotnet format
    dotnet build
    dotnet test

Run frontend verification from `ui/` after frontend edits:

    npm run lint
    npm run build
    npm run test

Verification completed with all required commands. The final backend test run passed 151 Printmon tests, 106 emulator tests, and 139 integration tests. The final frontend test run passed 241 tests across 29 files.

## Validation and Acceptance

An article created or edited with availability disabled must return `quantityAvailable: null`; one with an enabled quantity must round-trip that number through list and detail API responses. Existing rows upgraded by the migration must behave as disabled because the new article column is nullable.

Submitting an order for an enabled article must save the order and reduce its stored availability by the ordered quantity. Submitting an order for a disabled article must save the order and leave article availability disabled. A quantity that crosses below zero must still save.

In the new-order UI, an enabled article must turn orange when its projected remaining quantity after cart entries is at or below the configured threshold. The matching summary entry must use the warning palette too. Removing or decreasing cart quantity above the threshold must clear the warning, and disabled articles must never warn.

## Idempotence and Recovery

Source edits and verification commands can be repeated safely. The migration is additive; if the generated migration is wrong before it reaches shared databases, correct the model and regenerate or adjust only the new migration files.

## Artifacts and Notes

The accepted concurrency tradeoff is:

    Availability is a soft manually correctable balance.
    Orders do not reserve stock or reject oversell.
    Application-side decrements are intentionally not a full multi-register concurrency solution.

## Interfaces and Dependencies

Use `QuantityAvailable` in C# and `quantityAvailable` in JSON and TypeScript article contracts. Extend the existing article detail and list flows so the new-order page receives warning data without a new order-only endpoint.

Add one order-side application data command for decrementing article availability from `OrderRequestItem` quantities and register its database implementation with the other query and command ports. Do not add inventory events, locking primitives, or order response changes for this feature.

Plan revision note (2026-05-22): created from the agreed soft availability implementation plan after repo exploration.

Plan revision note (2026-05-22): marked implementation complete after backend migration, soft decrement behavior, frontend controls and warnings, documentation, and full verification passed.
