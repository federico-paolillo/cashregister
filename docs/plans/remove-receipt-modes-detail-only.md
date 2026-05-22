# Remove receipt modes and keep detail printing

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register currently has a runtime receipt-mode switch between a compact summary receipt and a detail receipt workflow. The operator no longer needs that choice. After this change, placing or reprinting an order always prints the existing detail workflow: one priced order overview receipt, followed by one article slip for every ordered unit.

The removal should leave the existing order and device printing flow intact while deleting the now-meaningless mode-selection API and Devices-page control. The per-unit article slip builder must remain distinct from the priced overview builder because a later task will decide which articles should produce per-unit slips without making the overview conditional.

## Progress

- [x] (2026-05-22) Inspected receipt construction, receipt-mode API/runtime state, Devices route, receipt tests, architecture docs, and execution-plan conventions.
- [x] (2026-05-22) Refactored receipt construction and printing tests so detail output is the only backend path.
- [x] (2026-05-22) Removed backend receipt-mode runtime state and HTTP endpoints.
- [x] (2026-05-22) Removed frontend receipt-mode DTO, loader/action branch, Devices-page toggle, and tests.
- [x] (2026-05-22) Updated architecture and diary documentation and ran backend/frontend verification.

## Surprises & Discoveries

- Observation: The detail workflow already returned multiple print programs from the application receipt service before mode removal.
  Evidence: The pre-refactor receipt service added one detail overview program before adding one item receipt for every ordered unit, and `IReceiptPrintProgramService.BuildAsync` already returned `Result<ImmutableArray<PrintProgram>>`.
- Observation: The article and receipt projections do not yet expose an article-level setting for per-unit slips.
  Evidence: `Article`, `ArticleEntity`, and `OrderPrintDataItem` carry description, price, quantity, and retirement-related data only.
- Observation: Three higher-level order flow tests asserted the old one-program summary receipt count outside the receipt test folder.
  Evidence: The first full backend test run failed in order reprint endpoint, order creation endpoint, and place-order activity tests with expected print count `1` and actual print count `3` for quantity-two fixtures.

## Decision Log

- Decision: Delete the receipt-mode API and Devices-page toggle instead of keeping a compatibility shape with one valid mode.
  Rationale: The user confirmed there is no remaining mode choice to expose.
  Date/Author: 2026-05-22 / Codex
- Decision: Preserve the current detail overview and per-unit article slips as separate receipt program builders.
  Rationale: The next task will gate only article slips by article configuration while the priced overview remains unconditional.
  Date/Author: 2026-05-22 / Codex
- Decision: Do not add article configuration fields, migrations, API changes, or filtering in this refactor.
  Rationale: The user scoped this task to mode removal with preparation only for the next implementation.
  Date/Author: 2026-05-22 / Codex

## Outcomes & Retrospective

Receipt mode selection is removed end to end. Receipt construction now has one default path that preserves the priced overview and per-unit article slips, while the overview and slip builder methods remain separate for the next article-configuration task. Backend and frontend verification passed after updating order-flow integration assertions that encoded the old one-program behavior.

## Context and Orientation

Receipt construction lives in `be/Cashregister.Application/Receipts/Services/Defaults/ReceiptPrintProgramService.cs`. It fetches the receipt-specific order projection through `IFetchOrderPrintDataQuery` and builds `PrintProgram` instances, which are portable printer instruction programs. `be/Cashregister.Application/Receipts/Handlers/Defaults/PrintReceiptHandler.cs` sends every returned program to the configured printer device in order.

Before this change, the mode switch was process-local backend state. `ReceiptMode` and `ReceiptModeStore` lived under `be/Cashregister.Application/Receipts`, the store was registered by receipt service registration, and `be/Cashregister.Api/ReceiptModes` exposed `/receipt-mode`. The frontend Devices route in `ui/app/routes/devices/devices.tsx` fetched `/devices` and `/receipt-mode`, rendered printer selection plus a receipt-mode checkbox, and posted mode or device selection through the same action.

Backend receipt behavior is covered by receipt integration tests under `be/Cashregister.Tests.Integration/Receipts`. The deleted receipt-mode API had endpoint tests under `be/Cashregister.Tests.Integration/Api`. The frontend Devices route has route-level tests under `ui/app/routes/devices`.

## Plan of Work

First, simplify `ReceiptPrintProgramService` so it no longer receives `ReceiptModeStore`, no longer branches between normal and detail receipt templates, and directly builds the priced overview plus per-unit article slips after a successful print-data lookup. Keep the current multiple-program application interface and `PrintReceiptHandler` loop because the only remaining workflow still prints multiple programs.

Next, delete the receipt-mode application state and HTTP route group. Remove the store registration from receipt dependency registration and remove the receipt-mode route mapping from the API composition root. Rewrite receipt service and handler tests to assert that detail output is now the default path, and remove tests that exist only for the deleted store or deleted endpoint.

Then, simplify the Devices frontend route to use only device DTOs and device requests. Remove the receipt-mode DTO from shared frontend models, delete the receipt-mode section from the Devices page, and update Devices route tests so they assert device loading, device selection, action errors, and missing device-id failure without mode setup.

Finally, update `docs/ARCH.md` to remove the receipt-mode route and runtime-state description and describe the one remaining receipt workflow. Append the required `docs/DIARY.md` entry for the completed refactor. This plan itself remains in `docs/plans/remove-receipt-modes-detail-only.md` and should be updated with progress, outcomes, and verification evidence as the implementation proceeds.

## Concrete Steps

Run backend verification from `be/` after the source changes:

    dotnet format
    dotnet build
    dotnet test

Run frontend verification from `ui/` after frontend changes:

    npm run lint
    npm run build
    npm run test

All commands should complete successfully. If a formatter changes touched source files, inspect those diffs before continuing and keep changes scoped to receipt-mode removal.

## Validation and Acceptance

Backend receipt service tests must show that an existing order with multiple quantities returns the priced overview first and then one article slip for every ordered unit without selecting any receipt mode. Handler tests must show that the printer receives all programs in order and stops on a printer failure during multi-program output. Missing-order behavior must continue to return `NoSuchOrderPrintDataProblem`.

The API should no longer register `/receipt-mode`, and there should be no endpoint tests or DTOs for that deleted route. The Devices page should load printer devices only, render printer selection only, post selected device ids to `/devices/{id}`, and keep its current error handling for bad actions.

Documentation acceptance is that `docs/ARCH.md` describes a single priced-overview-plus-slips workflow and no longer describes receipt mode runtime state or a `/receipt-mode` route. The task diary entry must identify the implementation decision and link this ExecPlan relative to `docs/`.

## Idempotence and Recovery

The change does not require database schema work or external state migration. Re-running builds and tests is safe. If a deleted receipt-mode reference remains, use repository search for `ReceiptMode` and `receipt-mode` to find the missed code or test before broadening the refactor.

## Artifacts and Notes

The approved implementation intent is:

    Always print the current detail output.
    Delete the receipt-mode backend API and frontend toggle.
    Keep the priced overview unconditional and prepare only structurally for future article slip filtering.

Verification completed with:

    cd be && dotnet format
    cd be && dotnet build
    cd be && dotnet test
    cd ui && npm run lint
    cd ui && npm run build
    cd ui && npm run test

The final backend test run passed with 150 Printmon tests, 106 emulator tests, and 127 integration tests. The final frontend test run passed with 232 tests across 29 files.

## Interfaces and Dependencies

Keep `IReceiptPrintProgramService.BuildAsync(Identifier)` returning:

    Task<Result<ImmutableArray<PrintProgram>>>

The array remains necessary because one order produces an overview program and article-slip programs. Remove these interfaces entirely:

    ReceiptMode
    ReceiptModeStore
    GET /receipt-mode
    POST /receipt-mode/{mode}
    ui ReceiptModeDto

No new article API, persistence field, or receipt print-data field should be introduced in this task.

Plan revision note (2026-05-22): marked the backend and frontend implementation steps complete after deleting receipt-mode code paths and preserving the detail receipt builders as the only receipt construction path.

Plan revision note (2026-05-22): recorded the completed documentation and verification steps, the order-flow test discovery, and the successful final verification results.

Plan revision note (2026-05-22): clarified pre-refactor context after completion so the living plan does not describe deleted mode state as current behavior.
