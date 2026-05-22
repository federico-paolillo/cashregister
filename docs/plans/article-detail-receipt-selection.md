# Add per-article detail receipt selection

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register always prints one priced order overview receipt and then detail receipts for every ordered article unit. Operators need to suppress those per-unit detail receipts for selected articles without losing the order overview. After this change, an operator can leave the detail-receipt checkbox enabled for articles that need article slips, disable it while bulk creating or editing articles that do not, and print or reprint an order with the overview still present.

## Progress

- [x] (2026-05-22) Inspected article persistence, article API and frontend forms, current receipt print-data projection, receipt builders, tests, architecture docs, and execution-plan conventions.
- [x] (2026-05-22) Added article detail-receipt persistence, explicit API write contracts, current-state receipt projection, and print filtering.
- [x] (2026-05-22) Added frontend checkboxes and form serialization for bulk article creation and article editing.
- [x] (2026-05-22) Added backend and frontend tests that cover defaults, writes, and printing behavior.
- [x] (2026-05-22) Updated architecture and diary documentation, then ran backend and frontend verification.

## Surprises & Discoveries

- Observation: Order items snapshot article id, description, price, and quantity, while receipt print data currently reads only those order-item values.
  Evidence: `be/Cashregister.Database/Entities/OrderItemEntity.cs` has no article print configuration and `be/Cashregister.Database/Queries/FetchOrderPrintDataQuery.cs` projects order item fields only.
- Observation: Receipt construction already has a separate unconditional overview builder and per-unit detail builder.
  Evidence: `ReceiptPrintProgramService.Build` adds `BuildOverview(order)` before iterating items and calling `BuildItemReceipt(order, item)`.
- Observation: Bulk article creation needs explicit row ids for boolean fields.
  Evidence: unchecked checkboxes are absent from `FormData`, while the bulk route creates one API request from parallel description and price arrays.

## Decision Log

- Decision: Reprints use the current article detail-receipt configuration.
  Rationale: The user selected current article configuration, so article edits must affect both future prints and order reprints without snapshotting the flag onto order items.
  Date/Author: 2026-05-22 / User and Codex
- Decision: Article create requests must carry an explicit detail-receipt field.
  Rationale: The user selected an explicit create contract instead of server-side omission defaulting; the bulk UI will still default its checkbox to checked.
  Date/Author: 2026-05-22 / User and Codex
- Decision: Keep article list DTOs unchanged and return the flag through single-article fetches.
  Rationale: The requested checkbox appears in bulk create rows and the selected article edit form, and the selected article path already fetches `GET /articles/{id}` for edit data.
  Date/Author: 2026-05-22 / Codex

## Outcomes & Retrospective

Completed. Articles now store a `PrintDetailReceipt` setting with a migration default of `true`, the API enforces explicit article writes for that setting, the frontend exposes it on bulk-create rows and article edits, and receipt printing filters only per-unit detail programs from current article state. Full backend and frontend verification passed after direct `ArticleEntity` persistence fixtures were updated for the required column.

## Context and Orientation

Articles are stored through `be/Cashregister.Database/Entities/ArticleEntity.cs`, mapped to `be/Cashregister.Domain/Article.cs` by `ArticleEntityMapper`, and changed through article transactions in `be/Cashregister.Application/Articles`. The API DTOs and HTTP handlers for article creation, fetch, and edits live under `be/Cashregister.Api/Articles`.

Receipt printing starts with `ReceiptPrintProgramService`, which fetches an `OrderPrintData` projection through `IFetchOrderPrintDataQuery`. A print program is a printer-independent set of receipt instructions. The overview receipt is the priced order receipt. The detail receipt is the article slip built once per ordered unit after the overview.

The frontend bulk article route lives in `ui/app/routes/articles-bulk`. It posts one create request per bulk row. The selected article editor lives in `ui/app/routes/articles`; the detail panel passes selected article data into the shared `ArticleForm`, and the route action posts the edit request.

## Plan of Work

First, add `PrintDetailReceipt` to the article domain and application write models, persist it on `ArticleEntity`, map and save it, and expose it on article create, change, and fetch API DTOs. Add an EF migration that adds the article column with a database default of `true` so existing persisted articles keep printing detail receipts after migration. Validate write requests as missing when the explicit boolean field is omitted.

Next, extend the receipt print-data item projection with the current article detail-receipt flag. Fetch the flag from the article table at print time, including retired articles for historical order reprints, while retaining order-item description and price snapshots for receipt content. Keep the overview builder unconditional and skip only per-unit detail receipt programs for items with the flag disabled.

Then, add a simple `Detail receipt` checkbox to the selected article edit form and every bulk-create row. The edit action converts checkbox presence into a boolean request property. Bulk creation needs stable row ids in the form because unchecked checkboxes do not appear in `FormData`; each row submits its id and uses that id as the checkbox value before the route creates one request per row.

Finally, cover the new API and receipt behavior in backend tests and the new checkbox behavior in frontend tests. Update `docs/ARCH.md` to describe configured detail slips, append the task result to `docs/DIARY.md`, and keep this ExecPlan current with verification outcomes.

## Concrete Steps

Run backend verification from `be/` after backend source and migration changes:

    dotnet format
    dotnet build
    dotnet test

Run frontend verification from `ui/` after frontend changes:

    npm run lint
    npm run build
    npm run test

The commands should complete successfully. Inspect generated migration files before finalizing and keep all changed files tied to the article detail-receipt selection.

Verification completed with:

    cd be && dotnet format
    cd be && dotnet build
    cd be && dotnet test
    cd ui && npm run lint
    cd ui && npm run build
    cd ui && npm run test

The final backend test run passed 151 Printmon tests, 106 emulator tests, and 135 integration tests. The final frontend test run passed 235 tests across 29 files.

## Validation and Acceptance

Creating an article through the API with `printDetailReceipt` set to `true` or `false` must persist that value and return it from `GET /articles/{id}`. Create and change requests that omit the explicit field must return `BadRequest`. Editing an article from the Articles detail panel must initialize the checkbox from fetched article data and submit the changed boolean. Bulk article rows must begin checked and submit independent boolean values per row.

Printing an order with enabled and disabled articles must still produce an overview receipt containing every ordered item. Enabled article units must produce detail receipts according to quantity. Disabled article units must produce none. If every article in an order is disabled, printing must still return the overview receipt. Changing the checkbox after an order exists must affect a later reprint because the print projection uses current article configuration.

## Idempotence and Recovery

Application edits and verification commands can be repeated safely. The migration is additive and only adds an article boolean with default `true`; if the generated migration shape is incorrect, correct the model and regenerate or edit only the new migration before it is applied outside local test databases.

## Artifacts and Notes

The approved behavior is:

    Per-article detail receipt selection defaults to checked in article creation.
    Overview receipts are never suppressed by article flags.
    Reprints read current article configuration.

## Interfaces and Dependencies

At the article boundary, use `PrintDetailReceipt` in C# and `printDetailReceipt` in JSON and TypeScript contracts. Extend:

    Article
    ArticleDefinition
    ArticleChange
    ArticleEntity
    ArticleDto
    RegisterArticleRequestDto
    ChangeArticleRequestDto
    OrderPrintDataItem

Do not add detail-receipt state to order domain models, order entities, or Printmon instructions. The existing receipt service remains the only place that decides whether to append a per-unit detail print program after the overview.

Plan revision note (2026-05-22): marked implementation and verification complete after adding the article setting, current-state receipt filtering, frontend checkboxes, tests, migration, and documentation.
