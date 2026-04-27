# Diary

> This file records implementation decisions, design choices, and strategies per task to avoid re-deriving the same conclusions when picking up work later.

## UI route simplification

Removed stale frontend assumptions left after simplifying article and order navigation. Article editing is now the only single-article form path on `/articles`, while article creation remains on `/articles/bulk`. Order details stay embedded in the order overview instead of depending on the removed single-order route.

### Key decisions

- We removed the article form intent mechanism because the form is no longer shared by create and edit flows.
- We inlined order item rendering into the order overview detail panel because the removed order view route was its only other owner.
- We kept the modal system and modal tests untouched because future UI flows may still use it.

## Central frontend API delay

Added a randomized artificial delay to the frontend `ApiClient` so local API calls take at least 50ms and at most 150ms. The delay starts in parallel with `fetch`, making fast local calls perceptible without adding extra latency to calls that are already slower than the selected delay.

### Key decisions

- We kept the delay frontend-only and centralized in `ApiClient` because all route loaders, actions, and components already call the backend through `deps.apiClient`.
- We delay successful responses, HTTP error responses, and network failures consistently so UI loading and disabled states do not flicker for local calls.

## Article master-detail editor

Changed `/articles` from edit-button modal editing into a master-detail screen symmetric with `/orders`. Selecting an article now stores `articleId` in the URL, highlights the selected row, and opens a right-side detail panel that reuses the existing article form for edits. Cursor pagination keeps the selected article id while loading more rows, and article creation remains in the existing modal flow.

### ExecPlan

`plans/article-master-detail.md`

### Key decisions

- We fetch the selected article through `GET /articles/{id}` as `ArticleDto` instead of deriving it from the current list page so the route matches the orders implementation and the backend API signature.
- We removed the article table actions column because row selection now owns editing and delete belongs to the selected detail panel.
- We made the article form support panel usage through a cancel link while preserving modal cancel behavior for article creation.
- We restored article deletion in the detail panel instead of the table row; delete uses a panel-local API call to `DELETE /articles/{id}` and closes the panel only after success so the route does not revalidate a deleted `articleId`.

## Order overview route cleanup

Cleaned up the order overview master-detail implementation after review. The route now relies on React Router generated loader-data typing instead of a local duplicate type, and order overview URL construction lives in a route-local helper module so selection and close links share the same query-string policy.

### Key decisions

- We kept URL helpers local to the order overview route because `orderId` and `until` are route-specific state, not an app-wide URL abstraction.
- We documented that route components should use React Router generated loader-data typing because local loader-data interfaces drift from the actual loader return shape.

## Orders master-detail view

Changed `/orders` from list-to-route navigation into a master-detail screen. Selecting an order now keeps the user on `/orders`, stores the selection in `orderId` query state, and opens a closable right-side detail panel with persisted order metadata, item lines, totals, and the reprint action. Cursor pagination remains form-based and keeps the current selection when loading more rows.

### ExecPlan

`plans/order-master-detail.md`

### Key decisions

- We implemented a new read-only detail panel component instead of adapting the make-order summary because the order list displays persisted orders, not mutable cart state.
- We moved reprint from the row into the detail panel so the table is a pure selection surface and the action stays attached to the currently inspected order.
- We distinguished row-selection reloads from pagination reloads through `useNavigation().formData` so the "Load More" button only enters its loading state for actual pagination submissions.

## Typed API test payloads

Updated the odd-cent order override API test to use `OrderRequestDto` with `PostAsJsonAsync` instead of hand-written JSON and `StringContent`. Documented the testing convention so API integration tests default to typed DTOs and typed HTTP helpers.

### Key decisions

- We use raw JSON only when the test is explicitly about malformed payloads, unknown fields, or serializer-boundary behavior; normal endpoint behavior should be tested through the public DTO types.

## Harmonized price handling

Refactored price handling so the backend preserves exact integer cents from API DTOs through domain and persistence, while the frontend presents decimal money inputs to users and submits hidden cent fields. Removed the 5-cent CHF rounding from `Cents`, made order creation accept `totalOverrideInCents`, and introduced a shared money input component backed by centralized string-based parsing.

### ExecPlan

`plans/harmonize-price-handling.md`

### Key decisions

- We kept database column names `Price` and `TotalOverride` because their stored `long` values already represent cents; API names remain explicit with `InCents`.
- We normalize valid frontend money input on blur to avoid cursor jumps while typing.
- We reject invalid decimal precision instead of rounding or truncating because amount-in must equal amount-out.

## Submitted order reprint action

Added a reprint action to the `/orders` frontend table. Each submitted order row now exposes a printer icon button that posts to the existing receipt print endpoint and disables only itself while the request is pending. Successful print requests show an informational confirmation with the order id, and failed print requests surface through the existing frontend error message system.

### Key decisions

- We reused `POST /orders/{id}/print` because the backend already exposes explicit receipt reprinting and has endpoint coverage for success, missing orders, and device failures.
- We kept the reprint request inside the row component instead of adding a React Router action because printing is a side effect that does not change order list data and should not revalidate the paginated table.

## Order creation receipt printing saga

Wired `POST /orders` to `PlaceOrderActivity` so order creation now runs the in-process saga: place the order, print its receipt, and fetch the saved order before returning the created pointer. The endpoint still returns `400` for invalid order requests, but printer or orchestration failures now surface as `500`.

### Key decisions

- We register `PlaceOrderActivity` and the generic `Scoped<T>` helper through an Activities service collection extension because activities are composition services, not order transaction implementations.
- We keep the saga steps in independent scopes because that is the existing activity model; if printing fails after order placement, the order remains committed and the API reports the print failure.
- We added both activity-level and API-level tests because previous order endpoint coverage only proved persistence, not receipt printing.

## Order receipt printing endpoint

Added an Application-layer `IPrintReceiptHandler` and exposed receipt printing through `POST /orders/{id}/print`. The handler builds the receipt `PrintProgram` through the existing receipt service and sends it to the configured `IDevice`. The API maps missing order print data to `404`, successful device delivery to `204`, and unexpected device failures to `500`.

### Key decisions

- We made receipt printing a handler instead of a transaction because it orchestrates external printer I/O and does not mutate persistence.
- We reused `IReceiptPrintProgramService` instead of fetching receipt data in the handler because the service already owns the order-to-`PrintProgram` workflow.
- We initially kept order creation separate from printing; `POST /orders` now prints through `PlaceOrderActivity`, while `POST /orders/{id}/print` remains available for explicit reprints.

## Technician device selection page

Added a `/devices` frontend route and backend device endpoints for selecting the receipt printer target at runtime. The backend exposes `/devices` only; `/api` remains a frontend proxy/base-url concern. Device ids are URL-safe identifiers derived from writable Linux printer file paths because the current `FileDevice` writes bytes through `FileStream`.

### Key decisions

- We keep selected target state in a singleton runtime store initialized from `FileDeviceSettings.Target`. We do not mutate the Options cache because options binding is configuration input, not runtime application state.
- We validate selection ids against the current device catalog before updating the runtime target. We do not accept arbitrary path strings from the client.
- We enumerate `/dev/usb/lp*` and `/dev/lp*` file devices instead of CUPS queues because CUPS URIs are not valid `FileStream` targets.
- We use `/dev/null` as the development default target because `FileDevice` opens an existing file path for writing.

## Receipt PrintProgram template service

Added an Application-layer receipt service that builds a `PrintProgram` for an order from a receipt-specific projection. The projection contains only order number, id, date, item descriptions, and quantities; it deliberately excludes prices and totals. The service is pure and only constructs the print template, leaving actual printer I/O for later order-submission orchestration.

### Key decisions

- We return `Result<PrintProgram>` and `NoSuchOrderPrintDataProblem` for missing orders because expected Application failures use `Result<T>` and `Problem`, not exceptions.
- We project receipt data directly from EF entities into Application output models because receipt construction should not depend on Domain aggregate models.
- We format receipt dates as UTC because `TimeStamp` stores Unix seconds generated from UTC time.

## Stable receipt item order

Fixed receipt projection item ordering so printed receipt lines are deterministic. The projection now orders order items before materializing receipt print data, and the integration test asserts the item sequence instead of only checking membership.

### Key decisions

- We order receipt items by their persisted item id because order items currently have no explicit line-number column. This keeps receipt output stable without changing the schema.

## Architecture, ESC/POS, and conventions documentation refresh

Refreshed the project documentation split so `ARCH.md` covers high-level backend/frontend architecture, `ESCPOS.md` owns all `Cashregister.Printmon.*` command-level details, and root `CONVENTIONS.md` captures reusable backend/frontend development rules for a future `AGENTS.md` update.

### ExecPlan

`plans/arch-escpos-conventions-refresh.md`

### Key decisions

- We kept ESC/POS instruction, encoder, emulator, CLI, and printer-device details out of `ARCH.md` because duplicating them there would create documentation drift.
- We put `CONVENTIONS.md` at repository root because it is intended to feed future agent instructions rather than explain one subsystem.
- We treated this as documentation-only work and did not touch source code or the existing unrelated `AGENTS.md` modification.

## Development markdown printer device

Added a development-only `MarkdownDevice` in `Cashregister.Printmon.Emulator` and wired the API startup to use it only in the `Development` environment. The device encodes a `PrintProgram`, runs the existing emulator pipeline, renders the final receipt to markdown, and writes it to a timestamped file under the configured root folder. Non-development environments still use `FileDevice`.

### Key decisions

- We placed `MarkdownDevice` in `Cashregister.Printmon.Emulator/Device` because it is defined by the emulator pipeline, not by raw printer file output.
- We kept `BinaryEncoder` in the flow and ran the full emulator decode/execute path so development output exercises the same ESC/POS bytes that production printing emits.
- We switched the API composition root on `builder.Environment.IsDevelopment()` instead of adding a runtime toggle because the requirement is environment-specific, not operator-selectable.

## Backend API Docker image

Added a buildx-oriented Dockerfile for publishing `Cashregister.Api` as a self-contained linux-x64 single-file executable without Native AOT. The runtime image uses the official .NET 10 Ubuntu chiseled `runtime-deps` base because the backend now opts into invariant globalization, and the application files are copied as root-owned read-only files under `/app` while SQLite state lives under `/var/lib/cashregister`.

### Key decisions

- We use `runtime-deps` instead of `aspnet` because the published API is self-contained and carries the .NET runtime with the app.
- We keep `/app` read-only for the non-root runtime user and reserve `/var/lib/cashregister` as the writable application state directory.
- We put the build command in `build-be-dockerfile.sh` so local and CI invocations use the same buildx arguments.

## DevOps conventions document

Added `docs/DOCKER.md` as the home for Docker, container-image, and shell-script conventions. The document records hardened container defaults and minimal shell-script defaults so deployment-related work does not have to rediscover the same baseline.

### Key decisions

- We keep DevOps conventions separate from backend and frontend conventions because container hardening and shell-script behavior apply across project areas.
- We mention `docs/DOCKER.md` from `AGENTS.md` and `docs/CONVENTIONS.md` so future work can find the new convention surface.
