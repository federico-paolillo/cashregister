# Runtime receipt mode detail printing

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register currently prints one summary receipt for each order. After this change, an operator can switch a non-persistent runtime setting from the Devices page to print a priced overview receipt followed by one small item receipt for every ordered unit. This lets the same order support both the current summary-only workflow and a detail workflow where individual item slips can be handed off separately.

## Progress

- [x] (2026-05-03) Researched the existing receipt print service, device runtime state, Devices page, and receipt tests.
- [x] (2026-05-03) Locked UI location to the Devices page and detail overview row format to `2x Coffee @ 1.23 = 2.46`.
- [x] (2026-05-03) Add backend receipt-mode runtime state and API endpoints.
- [x] (2026-05-03) Expand receipt print data with prices and totals.
- [x] (2026-05-03) Make receipt printing emit one or more print programs based on the current mode.
- [x] (2026-05-03) Add Devices page toggle and frontend tests.
- [x] (2026-05-03) Update architecture/diary docs and run verification.

## Surprises & Discoveries

- Observation: The existing receipt projection deliberately omits prices and totals.
  Evidence: `OrderPrintDataItem` currently contains only `Description` and `Quantity`, and `ReceiptPrintProgramServiceTests` asserts prices and totals are absent from the normal receipt.
- Observation: The hamburger menu currently links to `/devices`; it does not host configuration controls directly.
  Evidence: `ui/app/components/navigation-menu.tsx` renders a single `Devices` link inside the dropdown.

## Decision Log

- Decision: Keep receipt mode in a singleton runtime store under the receipt application area.
  Rationale: This matches the existing current-printer target behavior: process-local state, initialized at startup, no database migration, no browser persistence.
  Date/Author: 2026-05-03 / Codex
- Decision: Put the toggle on the existing Devices page.
  Rationale: The user chose this location, and printer selection plus receipt-mode selection are both runtime print settings.
  Date/Author: 2026-05-03 / Codex
- Decision: Detail overview rows print unit and line totals as `quantity x description @ unit = line`.
  Rationale: The user chose the clearest row format even though it is wider than a line-total-only receipt.
  Date/Author: 2026-05-03 / Codex

## Outcomes & Retrospective

Implemented the runtime receipt mode toggle end to end. Backend and frontend verification passed.

## Context and Orientation

The backend lives under `be/`. Receipt construction is in `be/Cashregister.Application/Receipts/Services/Defaults/ReceiptPrintProgramService.cs`. The print handler in `be/Cashregister.Application/Receipts/Handlers/Defaults/PrintReceiptHandler.cs` sends the built `PrintProgram` to the configured `IDevice`. Device selection uses a singleton `FileDeviceTargetStore`, which is the model for non-persistent runtime state.

The frontend lives under `ui/`. The hamburger menu is `ui/app/components/navigation-menu.tsx`, and its Devices link opens `ui/app/routes/devices/devices.tsx`. That route currently loads `/devices` and posts selected printer ids to `/devices/{id}`.

## Plan of Work

Add `ReceiptMode` and `ReceiptModeStore` in the receipt application area, register the store as a singleton in receipt service registration, and expose `GET /receipt-mode` plus `POST /receipt-mode/{mode}` in the API. The DTO uses lower-case mode strings: `normal` and `detail`.

Expand `OrderPrintData` and `OrderPrintDataItem` so receipt construction can access item unit price, line total, and effective order total. The effective order total is `TotalOverride` when present; otherwise it is the sum of item unit price times quantity.

Change `IReceiptPrintProgramService.BuildAsync` to return `Result<ImmutableArray<PrintProgram>>`. In normal mode it returns one program matching the current receipt exactly. In detail mode it returns one priced overview program, then one item-ticket program per ordered unit. `PrintReceiptHandler` prints each program in order and returns the first device failure.

Update the Devices frontend route so the loader fetches both devices and receipt mode. The page renders the existing devices table and a checkbox-style form control named `receiptMode`. Submitting the checkbox posts either `/receipt-mode/detail` or `/receipt-mode/normal`, and errors reuse the current action error path.

## Concrete Steps

Run backend checks from `be/`:

    dotnet format
    dotnet build
    dotnet test

Run frontend checks from `ui/`:

    npm run lint
    npm run build
    npm run test

## Validation and Acceptance

Backend acceptance: normal mode still emits one receipt without prices or totals; detail mode emits one overview receipt with prices and total, then one item receipt per ordered unit. Reprinting an order with quantity three prints four programs.

Frontend acceptance: visiting `/devices` shows printer devices and a detail receipt mode checkbox. Checking it posts detail mode; unchecking it posts normal mode. Loader and action failures surface through the existing error message system.

## Idempotence and Recovery

The change is additive and does not require database migrations. Re-running tests and builds is safe. If receipt mode endpoints fail, inspect API route registration in `be/Cashregister.Api/Program.cs` and receipt service registration in `be/Cashregister.Application/Receipts/Extensions/ServiceCollectionExtensions.cs`.

## Artifacts and Notes

Verification completed with these commands:

    cd be && dotnet format
    cd be && dotnet build
    cd be && dotnet test
    cd ui && npm run lint
    cd ui && npm run build
    cd ui && npm run test

Backend tests passed with 383 total tests across Printmon, emulator, and integration projects. Frontend tests passed with 218 tests across 27 files.

## Interfaces and Dependencies

At completion, these interfaces exist:

- `ReceiptMode` enum with `Normal` and `Detail`.
- `ReceiptModeStore.Current` and `ReceiptModeStore.Select(ReceiptMode)`.
- `IReceiptPrintProgramService.BuildAsync(Identifier)` returns `Task<Result<ImmutableArray<PrintProgram>>>`.
- API `GET /receipt-mode` returns `{ "mode": "normal" }` or `{ "mode": "detail" }`.
- API `POST /receipt-mode/{mode}` accepts `normal` and `detail`.
