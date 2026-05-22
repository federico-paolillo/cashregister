# Diary

> This file records implementation decisions, design choices, and strategies per task to avoid re-deriving the same conclusions when picking up work later.

## UI Docker image

Added a buildx-oriented Dockerfile for publishing the React frontend as static files served by Caddy on port 65000. The image builds the UI with Node, builds a static Caddy binary with the official Caddy builder, and runs from a distroless nonroot runtime image. The root `Caddyfile` owns the server configuration. The build accepts `API_BASE_URL` and bakes it into Vite through `VITE_API_BASE_URL`.

### Key decisions

- We build Caddy from source with `CGO_ENABLED=0` instead of using the official Caddy runtime image because the final stage must be distroless.
- We keep API routing outside this image because the frontend artifact is a static bundle and deployment should decide how the API is exposed.
- We put Caddy runtime state under `/var/lib/caddy` so application files and configuration stay read-only while Caddy still has an explicit writable state volume.
- We document `VITE_API_BASE_URL` in the runtime environment, but rely on the build argument because Vite replaces that value at build time.

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

## Docker Compose gateway entry point

Completed the Docker Compose deployment entry point with `gateway`, `api`, and `ui` services. The gateway uses the official Caddy image and mounts `rp.Caddyfile`; the UI image now consumes `ui.Caddyfile` for its internal static-file server. External `/api/*` traffic is stripped before reaching the backend so API route groups remain unprefixed.

### Key decisions

- We split gateway and UI Caddy configuration into `rp.Caddyfile` and `ui.Caddyfile` to keep reverse-proxy routing separate from static SPA serving.
- We removed the legacy root `Caddyfile` after moving its only live consumer to `ui.Caddyfile`.
- We derive Docker build artifacts from BuildKit target architecture because Compose should produce runnable local images instead of mixing native base images with hardcoded amd64 binaries.

## Runtime receipt mode detail printing

Added a non-persistent receipt mode runtime setting, exposed through `/receipt-mode` and surfaced on the Devices page beside printer selection. Normal mode preserves the existing one-receipt summary output. Detail mode prints one priced overview receipt followed by one item receipt per ordered unit, so an order for three coffees prints four receipts.

### ExecPlan

`plans/receipt-mode-detail-printing.md`

### Key decisions

- We store receipt mode in a singleton runtime store because it should behave like the current printer selection and reset on backend restart.
- We return multiple `PrintProgram` instances from the receipt service so each receipt keeps the existing builder lifecycle, feed, and cut behavior.
- We put the toggle on the Devices page because printer selection and receipt mode are both runtime print configuration.

## Review finding cleanup for receipt mode and Docker Compose

Fixed review findings from the receipt-mode and Compose changes. Docker build references now match the tracked `Api.Dockerfile` and `Ui.Dockerfile` casing, and the edited receipt C# files conform to the repository final-newline formatter rule.

### Key decisions

- We kept the existing uppercase Dockerfile names because they are already tracked that way and changing case-only filenames is unnecessary for the finding.
- We used the project formatter for the final-newline cleanup so the result matches `.editorconfig` exactly.

## Statistics navigation tab

Added an all-time Statistics tab with backend aggregate statistics, JSON and CSV endpoints, and frontend tables for active-article sales and order-volume totals. The article table excludes retired articles and includes active articles with zero sales, while order-volume totals include all historical orders and order items. CSV export is split into one file per table.

### ExecPlan

`plans/statistics-tab.md`

### Key decisions

- We aggregate per-article volume from historical order-item prices because article edits must not rewrite past sales statistics.
- We keep retired article sales out of the article table but inside order-level totals because historical order value must remain complete.
- We expose two CSV downloads instead of one mixed CSV because each file stays rectangular and mirrors one visible table.

## Statistics CSV price formatting

Changed statistics CSV exports so money columns use decimal price strings with two fractional digits instead of raw integer cents. The JSON API still returns cents because the frontend money contract is cents-only; only human-facing CSV files changed.

### Key decisions

- We renamed CSV headers from `*InCents` to price-oriented names so the exported values are not mislabeled.
- We format with invariant culture and `0.00` because CSV files should be stable regardless of server locale.

## Statistics CSV application handlers

Moved statistics CSV generation out of the API route handlers into Application-layer handlers. The API now allocates the response stream, asks the Application handler to write the CSV content, and returns the populated bytes with HTTP content type and filename metadata.

### Key decisions

- We keep CsvHelper in `Cashregister.Application` because CSV row projection and serialization are application behavior for this export use case, not HTTP behavior.
- We keep filenames and `text/csv` content type in `Cashregister.Api` because those are response transport concerns.
- We convert the API-owned `MemoryStream` to bytes before returning so the stream can be disposed immediately without handing ASP.NET a dead stream.

## Statistics CSV class maps

Refactored statistics CSV exports to map Application output models directly with CsvHelper `ClassMap<T>`. CSV files no longer include total rows because they are intended for Excel, where users can compute totals from the exported data rows.

### Key decisions

- We removed CSV-specific record types because the exports now map cleanly from `ArticleStatisticsItem` and `OrderStatisticsSummary`.
- We keep the UI and JSON totals unchanged; only CSV output omits totals.
- We made the map types public because CsvHelper instantiates them through map registration and the analyzer does not treat that as direct internal usage.

## Statistics inventory and raw sales export

Refactored statistics around persisted order items as the historical source of truth. The statistics page now shows summary metrics, sold-article inventory, and per-order expected versus real volume. The old aggregate CSV downloads were replaced with one raw `sales.csv` export containing one order-item row per sale, integer cents, sale-time descriptions, current article descriptions, and optional order overrides. No totals are emitted in CSV.

### ExecPlan

`plans/statistics-inventory-and-raw-export.md`

### Key decisions

- We use order-item price snapshots for expected volume so later article price edits do not change history.
- We show current article descriptions in the UI, but keep sale-time descriptions in the CSV for auditability.
- We do not allocate order overrides to article rows because overrides are stored at order level only.
- We use a flattened CSV record in the writer so CsvHelper emits stable raw columns for Excel.

## Statistics reusable tabber

Added a small shared tabber for frontend views and refactored the Statistics page into article and order panels. Article inventory stays the first view, order summary metrics move beside order-volume rows, and the raw sales CSV export remains visible outside the tab selection.

### Key decisions

- We use an uncontrolled compound tabber API because the statistics page needs composable triggers and panels without route-level tab state.
- We include ARIA tab semantics and keyboard tab selection in the shared component so its first consumer does not set a weak accessibility baseline.

## Order multiplier keypad

Added an always-visible Multiplier keypad to New Order so cashiers can enter a one-use article quantity before selecting an article. The keypad is frontend-only: it accumulates a two-digit optional multiplier, clears leading zero entry, resets after the next article selection, and reuses the order form's existing per-article quantity fields.

### Key decisions

- We changed cart insertion to accept a quantity increment because multiplier entry should add to an existing article quantity instead of creating a separate order path.
- We kept the backend and order request contract unchanged because placed orders already carry article quantities.

## Separate order totals from overrides

Fixed order detail and overview totals so the displayed total is the stored order-item sum and the optional override remains a separate value. The order detail API keeps `totalInCents` and `totalOverrideInCents` distinct for overridden orders, and overview rows now carry both values for the two-column orders table.

### Key decisions

- We kept override-aware domain totals unchanged because this bug concerns order-display contracts, while other workflows use the effective total intentionally.
- We compute displayed item totals from order-item price snapshots and quantities so later article edits do not rewrite order history.

## Detail-only receipt printing refactor

Removed the runtime receipt mode selection path. Receipt printing now always emits the existing detail workflow: one priced order overview followed by one item receipt per ordered unit. The backend no longer exposes `/receipt-mode`, and the Devices page now handles printer selection only.

### ExecPlan

`plans/remove-receipt-modes-detail-only.md`

### Key decisions

- We kept the overview and item-slip builders separate because the next article configuration task should gate only per-unit slips.
- We did not add article print configuration or filtering yet because this refactor removes modes only.

## Startup printer preselection

Changed printer target startup state to come from discovered runtime devices instead of `FileDevice` configuration. API startup now preselects the first printer catalog entry when one exists, while manual `/devices` selection can still replace it. A physical `FileDevice` print attempt with no selected target now returns an explicit device problem instead of relying on a configured sink target.

### ExecPlan

`plans/startup-printer-preselection.md`

### Key decisions

- We removed the configured startup target so the normal one-printer path follows device discovery directly.
- We keep missing printer selection as a print failure because silently skipping or discarding a receipt would hide an operational problem.
- We keep CLI printing explicit by loading its existing `--device` value into the runtime target store before it prints.

## Per-article detail receipt selection

Added an article checkbox that controls whether that article emits per-unit detail receipts. Bulk article creation and the selected article editor both write the setting, existing articles migrate to the previous detail-printing behavior, and order printing still always emits the priced overview even when every ordered article has detail receipts disabled.

### ExecPlan

`plans/article-detail-receipt-selection.md`

### Key decisions

- We read the article setting from current article state during printing so later edits affect reprints without copying receipt configuration into historical order items.
- We require the article write API to receive the boolean explicitly while the frontend creation checkbox starts checked to preserve the operator default.
- We keep receipt price and description content on order-item snapshots while filtering only the per-unit detail programs from current article configuration.

## Soft article availability warnings

Added optional article availability balances that can be enabled from bulk creation or article editing. New orders decrement enabled balances without blocking oversell, and the order screen highlights selector buttons and summary entries when the current cart would leave an enabled article at or below the configured warning threshold.

### ExecPlan

`plans/soft-article-availability.md`

### Key decisions

- We use nullable article quantity as the disabled state so existing articles migrate with soft inventory off.
- We warn from projected cart balance in the frontend because the cashier needs the signal before placing the order.
- We keep application-side decrements soft and manually correctable instead of adding reservations, hard rejection, or locking behavior for future concurrent registers.

## Soft availability warning palette refinement

Adjusted low-availability order styling so warning text stays visibly orange and the summary row controls get orange surfaces instead of transparent icon buttons.

### Key decisions

- We avoid near-black orange text shades in the warning state because the warning should read as orange throughout the article button and summary entry.
- We assert both warning text and warning control surfaces in order UI tests so later styling changes do not quietly reintroduce neutral button text.

## Articles availability overview column

Added available quantity to the Articles table after price so cashiers can scan soft article balances without opening each article editor.

### Key decisions

- We reuse the quantity already present in article list data instead of adding a table-specific backend path.
- We render disabled quantity tracking as `-` and keep the management table plain because low-quantity warning colors belong to order-taking.
