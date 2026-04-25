# Harmonize Price Handling

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document must be maintained according to `docs/PLANS.md`.

## Purpose / Big Picture

After this change, Cash Register has one money boundary. The backend receives, stores, calculates, and returns exact integer cents. The frontend shows decimal amounts to users and converts those decimal strings to cents before calling the API. A cashier can enter `12`, `12.3`, or `12.34`; the UI normalizes those to `12.00`, `12.30`, and `12.34`, and the backend stores `1200`, `1230`, and `1234` exactly. Inputs such as `12.345` are invalid because the system must never round.

## Progress

- [x] (2026-04-25T12:11:39Z) Created this ExecPlan before implementation.
- [x] (2026-04-25T12:11:39Z) Remove 5-cent rounding from the backend `Cents` value object and add exactness tests.
- [x] (2026-04-25T12:11:39Z) Make order creation use `totalOverrideInCents` at the API boundary and prove exact raw JSON round-trip behavior.
- [x] (2026-04-25T12:11:39Z) Centralize frontend decimal-to-cents parsing and exact cents-to-decimal formatting.
- [x] (2026-04-25T12:11:39Z) Add and adopt a shared `MoneyInput` component for article prices and order total overrides.
- [x] (2026-04-25T12:11:39Z) Update tests, architecture/convention documentation, and diary.
- [x] (2026-04-25T12:23:50Z) Run backend and frontend verification commands.

## Surprises & Discoveries

- Observation: The first backend build failed because the raw JSON API test constructed `StringContent` inline.
  Evidence: `dotnet build` reported CA2000 for `new StringContent(requestJson, Encoding.UTF8, "application/json")`; changing it to `using var requestContent` fixed the build.

- Observation: `dotnet format` also normalized indentation in `be/Cashregister.Application/Devices/Extensions/ServiceCollectionExtensions.cs`.
  Evidence: The formatter changed only indentation around the existing `AddOptions<FileDeviceSettings>()` call.

## Decision Log

- Decision: Keep database column names `Price` and `TotalOverride`.
  Rationale: The persisted values are already `long` cents; renaming columns would add schema churn without improving runtime behavior. API names must remain explicit with the `InCents` suffix.
  Date/Author: 2026-04-25 / Codex

- Decision: Normalize valid money inputs on blur, not while typing.
  Rationale: Blur normalization avoids cursor jumps while still making the submitted cents value deterministic.
  Date/Author: 2026-04-25 / Codex

- Decision: Add an EF migration only if the EF model changes.
  Rationale: This refactor is expected to keep the persistence model unchanged; normal migrations remain the project policy if a schema change becomes necessary.
  Date/Author: 2026-04-25 / Codex

- Decision: Invalid decimals are rejected instead of rounded or truncated.
  Rationale: The requirement is "amount X in, amount X out"; accepting `12.345` would require either rounding, truncation, or storing fractional cents.
  Date/Author: 2026-04-25 / Codex

## Outcomes & Retrospective

Implemented. Backend `Cents` now preserves exact cent values, order creation accepts `totalOverrideInCents`, and API tests prove odd-cent article prices and order overrides round-trip exactly. Frontend money parsing is string-based, `MoneyInput` centralizes decimal entry plus hidden cent submission, and article/order forms no longer ask users to enter cents directly.

Verification completed on 2026-04-25:

    cd be && dotnet format
    cd be && dotnet build
    cd be && dotnet test
    cd ui && npm run lint
    cd ui && npm run build
    cd ui && npm run test

## Context and Orientation

The repository has a .NET backend in `be/` and a React frontend in `ui/`. Backend money is represented by `Cashregister.Domain.Cents`, currently in `be/Cashregister.Domain/Cents.cs`. API DTOs live under `be/Cashregister.Api`, with article DTOs using `PriceInCents` and order creation using `TotalOverrideInCents` so the JSON request body uses `totalOverrideInCents`.

Persistence uses EF Core entities in `be/Cashregister.Database/Entities`. `ArticleEntity.Price`, `OrderItemEntity.Price`, and `OrderEntity.TotalOverride` are `long` values and already store cents. Their database column names will remain unchanged by design.

Frontend money helpers live in `ui/app/money.ts`. UI components display cents through `formatPrice`, and user-entered amounts use `ui/app/components/money-input.tsx` so visible decimal strings submit hidden cent fields.

## Plan of Work

First, change `Cents.From(long)` so it validates non-negative input and returns the exact value. Add focused tests under the integration test project for exact values including `1`, `2`, and `999`, plus a negative input failure.

Next, rename the order creation request DTO property to `TotalOverrideInCents` and update the handler to read that property. Add an API test that posts raw JSON with `totalOverrideInCents` and verifies `GET /orders/{id}` returns the same odd cent value exactly. Add article endpoint coverage for an odd cent value such as `101`.

Then, rework `ui/app/money.ts` to use string/integer operations only. `formatPrice(cents: number)` must convert exact integer cents to `<integer>.<two_digits>` without `toFixed`. `decimalToCents(input: string)` must return `number | null`, accepting only digits with zero, one, or two decimal places. It must reject empty input, negative values, comma decimals, non-numeric strings, and more than two decimal places.

Add `ui/app/components/money-input.tsx`. The component renders a visible text input using decimal strings and a hidden input containing cents when the visible value is valid. It accepts `label`, `name`, `id`, `defaultCents`, `required`, and optional controlled `value`/`onValueChange` props needed by the order total override. Required inputs default to `0.00`; optional empty inputs submit no hidden cents field. On blur, valid non-empty values normalize to two decimal places.

Use `MoneyInput` in the article form, bulk article rows, and order summary. Route actions continue reading `priceInCents` and `totalOverrideInCents` from hidden fields, then send cents-only DTOs to the API.

Finally, update tests and documentation. `docs/ARCH.md` should describe the cents-only backend and decimal-only user input boundary. `docs/CONVENTIONS.md` should require `MoneyInput` and `ui/app/money.ts` for money entry and conversion. `docs/DIARY.md` should record the refactor and reference this plan.

## Concrete Steps

Work from repository root `/Users/federico.paolillo/src/cashregister`.

Run backend verification from `be/`:

    dotnet format
    dotnet build
    dotnet test

Run frontend verification from `ui/`:

    npm run lint
    npm run build
    npm run test

Expected successful outcome: each command exits with code 0.

## Validation and Acceptance

Backend acceptance requires `Cents.From(999).Value == 999`, article registration with `priceInCents: 101` returning `101`, and order creation with raw JSON `totalOverrideInCents: 999` returning `totalOverrideInCents: 999` and `totalInCents: 999`.

Frontend acceptance requires utility tests proving exact parsing and formatting, a shared `MoneyInput` that normalizes on blur, and route tests proving decimal-visible form values submit integer cents to the API without rounding.

## Idempotence and Recovery

All source edits are safe to repeat. No database migration should be created unless EF reports a model change requiring one. If a verification command fails, fix the reported failure and rerun the same command before moving on.

## Artifacts and Notes

None yet.

## Interfaces and Dependencies

At completion, `ui/app/money.ts` must expose:

    export function formatPrice(cents: number): string;
    export function decimalToCents(input: string): number | null;

At completion, `ui/app/components/money-input.tsx` must expose:

    export interface MoneyInputProps { ... }
    export function MoneyInput(props: MoneyInputProps): JSX.Element;

At completion, order creation JSON must use:

    {
      "items": [{ "article": "<id>", "quantity": 1 }],
      "totalOverrideInCents": 999
    }
