# Diary

> This file records implementation decisions, design choices, and strategies per task to avoid re-deriving the same conclusions when picking up work later.

## Past developments

This section consolidates older diary entries. Exact historical wording is intentionally left to git history and the referenced ExecPlans; the current diary keeps the durable implementation decisions and cross-layer contracts needed for future work.

### Frontend workflows and routing

The UI moved toward route-local, master-detail workflows for articles and orders. Article editing lives on `/articles` with `articleId` query selection, while creation stays on `/articles/bulk`; order inspection lives on `/orders` with `orderId` query selection instead of a separate order route. Row selection owns inspection, table actions were removed where the selected detail panel owns the action, and order reprinting moved into the selected detail surface. Route components should rely on React Router generated loader-data typing, and route-specific URL helpers should stay local when their query state is not app-wide.

The frontend API delay is centralized in `ApiClient`, where every response path waits for the same randomized 50ms to 150ms floor without adding latency to already-slower calls. The New Order multiplier keypad remains frontend-only and reuses existing per-article quantity fields because the backend order request already accepts quantities. The reusable tabber uses an uncontrolled compound API with ARIA tab semantics because the statistics page needed composable tabs without route-level tab state.

ExecPlans: `plans/article-master-detail.md`, `plans/order-master-detail.md`.

### Money, totals, and statistics

Money is preserved as exact integer cents from API DTOs through domain and persistence, while the frontend presents decimal money inputs and submits hidden cent fields such as `priceInCents` and `totalOverrideInCents`. The old 5-cent CHF rounding was removed from `Cents`; invalid decimal precision is rejected instead of rounded or truncated because amount-in must equal amount-out. Database column names such as `Price` and `TotalOverride` stayed unchanged because their stored `long` values already represent cents.

Order display separates stored item totals from optional overrides: detail and overview contracts expose `totalInCents` and `totalOverrideInCents` distinctly, and displayed item totals come from order-item price snapshots so later article edits do not rewrite history. Statistics also use persisted order items as the historical source of truth. Retired articles are excluded from active-article tables but still included in order-level totals, current article descriptions are useful for UI scanning, sale-time descriptions are kept in raw CSV for auditability, and order overrides are not allocated down to article rows because overrides are stored only at order level.

Statistics CSV behavior belongs in Application handlers, while filenames and `text/csv` content type stay in the API boundary. Human-facing aggregate CSV money columns use invariant decimal strings with two fractional digits and price-oriented headers; the later raw `sales.csv` export emits one order-item row per sale with integer cents, stable flattened columns, no totals, and enough historical fields for Excel analysis. API integration tests should use public DTOs and typed HTTP helpers unless the test is explicitly about malformed JSON or serializer-boundary behavior.

ExecPlans: `plans/harmonize-price-handling.md`, `plans/statistics-tab.md`, `plans/statistics-inventory-and-raw-export.md`.

### Receipt printing and devices

Receipt construction is Application behavior. The receipt service builds `PrintProgram` instances from receipt-specific projections and returns `Result<PrintProgram>` with `Problem` values for expected missing-order cases. The receipt projection deliberately avoids depending on Domain aggregate models, formats dates as UTC, and orders items by persisted item id because order items have no explicit line-number column. Printing itself is not a transaction because it orchestrates external printer I/O and does not mutate persistence.

`POST /orders/{id}/print` remains the explicit reprint endpoint. Order creation now runs the `PlaceOrderActivity` saga: place the order, print the receipt, and fetch the saved order before returning. Saga steps run in independent scopes; if printing fails after order placement, the order remains committed and the API reports the print failure. The frontend reprint action calls the print endpoint directly instead of a React Router action because printing does not change order list data and should not revalidate pagination.

Device selection is runtime state. The backend exposes `/devices` without an `/api` prefix, while `/api` remains a frontend proxy and deployment concern. Device ids are URL-safe identifiers derived from writable Linux printer file paths, and selection validates against the catalog rather than accepting arbitrary paths. Runtime printer settings use singleton stores, not Options cache mutation, because options binding is configuration input. The development markdown printer runs encoded bytes through the emulator pipeline in `Development` so local output exercises the same ESC/POS bytes as production. The retired receipt-mode work introduced runtime normal/detail selection; later detailed entries document its removal and replacement with always-detail printing.

ExecPlan: `plans/receipt-mode-detail-printing.md`.

### Deployment and operations

The UI image serves a Vite static bundle through Caddy on port 65000 and bakes `API_BASE_URL` into the build as `VITE_API_BASE_URL`; API routing stays outside that image because deployment decides how the backend is exposed. The backend image publishes `Cashregister.Api` as a self-contained linux-x64 single-file executable on .NET 10 Ubuntu chiseled `runtime-deps`; `/app` stays read-only for the non-root runtime user and `/var/lib/cashregister` is the writable SQLite state directory.

Docker and shell-script conventions live in `docs/DOCKER.md` instead of backend/frontend convention docs. Compose deployment uses a separate gateway Caddy config and UI Caddy config, stripping external `/api/*` before traffic reaches unprefixed backend route groups. Build artifacts derive from BuildKit target architecture so local Compose builds runnable images without hardcoded amd64 binaries. Review cleanup kept tracked `Api.Dockerfile` and `Ui.Dockerfile` casing because case-only filename churn was unnecessary.

### Architecture and documentation

The documentation split is intentional: `ARCH.md` owns current high-level backend/frontend architecture, `ESCPOS.md` owns `Cashregister.Printmon.*` command, encoder, emulator, CLI, and printer-device details, and development conventions live in `docs/CONVENTIONS.md`. DevOps conventions remain separate in `docs/DOCKER.md`. Documentation-only work should avoid source edits and use `git diff --check` as the minimum verification.

`ARCH.md` was kept focused on the implemented system instead of future architecture. Future BYOD and daemon work is documented separately in later detailed entries so the current architecture remains accurate.

ExecPlan: `plans/arch-escpos-conventions-refresh.md`.

---

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

## V2 BYOD architecture breakdown

Documented the intended v2 Bring Your Own Device architecture separately from the current implemented architecture. The v2 document captures the future print-station and spooler-daemon model, daemon mTLS identity, long-poll job leasing, at-least-once printing semantics, observability requirements, and all-in-one deployment expectations without changing the v1 architecture references.

### Key decisions

- We kept `ARCH.md` focused on the currently implemented system because v2 is a future architecture and should not make the current project documentation misleading.
- We documented `PrintStation` as the routing concept instead of cashier device identity because BYOD devices are transient while printer destinations are operationally stable.
- We explicitly accept at-least-once printing and duplicate receipts because the daemon cannot prove physical paper output and acknowledgement loss can happen after a successful device write.

## V2 daemon machine-to-machine authentication

Updated the v2 architecture document to make identity-provider machine-to-machine authentication the default daemon authentication model. Daemons use per-device service accounts or clients and bearer tokens over ordinary HTTPS, while Cashregister keeps tenant and print station assignment in its own database. Mutual TLS remains documented as an optional stronger transport-level model rather than a baseline requirement.

### Key decisions

- We prefer one machine-to-machine credential per daemon because shared credentials make revocation and attribution weak.
- We keep daemon credentials tenant-neutral because tenant assignment belongs to Cashregister print station configuration, not the identity provider credential.
- We avoid requiring Scaleway TLS passthrough and a Cashregister-managed CA for v2 because API-layer token validation works with ordinary HTTPS ingress.

## GitHub Actions release pipeline

Added CI and CD workflows for source validation and manual releases. CI runs backend formatting, build, and tests plus frontend install, lint, build, and tests on pushes to `main`. CD reruns the same validation, then publishes multi-platform GHCR images for the backend and frontend, creates the release git tag, and opens a draft GitHub release. Local workflow checks use Mise tasks that run `act` through Docker Compose without pushing images or creating releases.

### ExecPlan

`plans/github-actions-release-pipeline.md`

### Key decisions

- We kept CI/CD workflows on GitHub-native setup actions and official Docker actions instead of using Mise inside hosted workflows.
- We publish `cashregister-api:<version>` and `cashregister-fe:<version>` only, without a mutable `latest` tag.
- We document version reuse as a release-management discipline instead of adding fragile GHCR preflight scripts.

## README local startup and release notes refresh

Updated the README with local startup instructions for Docker Compose and Mise-based development, and aligned the release documentation with the current workflow policy: operator-owned version strings, no strict SemVer enforcement, and discipline-driven version reuse. Fixed the draft-release helper notes so the frontend digest links to the frontend image and the BuildKit attestation OS/Arch note uses the correct `unknown/unknown` spelling.

### Key decisions

- We document `docker-compose.local.yaml` as the local Compose entry point because the generic compose file was retired.
- We keep release version guidance descriptive instead of adding validation rules the workflow no longer enforces.
- We mention BuildKit attestation manifests in README because GHCR exposes them as `unknown/unknown` entries and that display can look like an unexpected third image.

## Diary compaction

Compacted older diary entries into a consolidated `Past developments` section while keeping the ten most recent pre-existing task entries in full. The compacted section preserves durable decisions, source-of-truth rules, cross-layer contracts, and ExecPlan references, while exact old prose remains recoverable from git history.

### Key decisions

- We replaced older detailed entries instead of adding only an index because the goal was to reduce the working length of `DIARY.md`.
- We kept the recent receipt, availability, v2, release, and README entries detailed because they are the current context most likely to guide near-term work.
- We did not use an ExecPlan because this was documentation maintenance, not a complex feature or significant refactor.

## Newest-first order overview API

Changed only the paginated `GET /orders` path to return orders from newest to oldest while preserving the existing cursor shape. The order list query now owns descending cursor semantics so article pagination and statistics exports keep their existing ordering.

### Key decisions

- We made `after` continue after the cursor in displayed newest-first order, which means fetching older orders by id.
- We left `/statistics` and `/statistics/sales.csv` unchanged because the requested UX change targets only the order overview API.
- We did not use an ExecPlan because this was a small, scoped query and test change.

## Order article status icon badges

Replaced the order screen's orange low-availability surface highlighting with compact icon badges and added a muted printer-off badge for articles whose detailed receipt printing is disabled. The paginated article list API now includes the detail-receipt flag so the order screen can render both statuses from its existing `/articles` load.

### Key decisions

- We use icon badges instead of additional colored article surfaces so low stock and receipt configuration can coexist without adding another full-row highlight color.
- We keep disabled detail receipts visually neutral because it is article configuration, not a sale-blocking warning.
- We expose `printDetailReceipt` on article list items rather than adding an order-specific API because the list already carries the order screen's article metadata.

## Order article status icon component split

Split the order screen status badge SVGs into dedicated route-local icon components while keeping the status wrapper responsible only for deciding which article badges to render.

### Key decisions

- We keep icon components untested because they are static SVG presentation and the order route tests already cover whether the statuses appear in the intended UI surfaces.
- We leave the wrapper route-local because the statuses are currently only part of the order-taking screen.

## Consolidated multi-agent codebase review

Produced a root `REVIEW.md` from five read-only backend and frontend review slices. The review scores overall codebase quality, separates backend and frontend findings, and focuses on idioms, SOLID, enterprise-pattern consistency, and project conventions. No source code was changed as part of the review.

### Key decisions

- We kept the review at the repository root to avoid replacing existing nested review documents.
- We treated the backend's enterprise-heavy structure as intentional and flagged only places where implementations weaken the documented architectural guarantees.
- We did not use an ExecPlan because this was a documentation-only review task.

## Grouped order article selection

Grouped the order screen article selector by initial letter while preserving the existing article button design and order submission behavior. The selector now derives sections client-side from the loaded article descriptions, sorts articles alphabetically within each section, and places non-letter starts under `#`.

### Key decisions

- We kept grouping and sorting in the frontend because the order UI already loads enough article data and backend article pagination remains ID-based.
- We split the selector into route-local React components so the existing article button rendering stays isolated from section composition.

## Fixed order multiplier while scrolling articles

Adjusted the order screen layout so the article selector owns the scroll area while the multiplier keypad remains fixed in the left order pane. This keeps quantity entry visible while browsing grouped article sections and does not change cart behavior or order submission.

### Key decisions

- We moved scrolling to the article-list wrapper instead of making the multiplier sticky because the multiplier is already a sibling of the article selector.
- We kept the change in the route layout only because no backend contract or shared component behavior changed.

## Removed local act harness

Removed the local GitHub Actions emulator harness from active project behavior. Hosted GitHub Actions are now the only workflow execution path, and local verification relies on the direct backend and frontend commands documented in `ARCH.md`.

### Key decisions

- We deleted the local harness instead of replacing it with another workflow emulator because GitHub-hosted workflows are the source of truth.
- We removed the Mise workflow tasks instead of repointing them to direct validation commands because their previous contract was specifically local GitHub Actions execution.

## V2 daemon setup access point

Documented daemon network onboarding for v2 around a temporary setup access point and captive-portal-style local page. The daemon enters setup mode on first boot, missing Wi-Fi configuration, or repeated connection failure, using five twelve-second attempts as the baseline threshold. The setup mode changes Wi-Fi credentials without requiring `adb`, shell access, `nmcli`, or a Linux workstation, and it is explicitly separate from destructive factory reset.

### Key decisions

- We made setup AP onboarding the baseline because changing mobile hotspots through `adb` is not acceptable for normal operators.
- We treat a new Wi-Fi profile as a candidate commit when the hardware cannot run access-point and client mode together, so a bad password automatically returns the daemon to setup mode.
- We kept a dedicated setup/reset button as future work because it requires explicit communication between the Arduino microcontroller and the Linux CPU.

## V2 product observability

Expanded the v2 observability model around OpenTelemetry instrumentation for the API server and daemon. The updated design focuses on product signals such as order creation, print-job leasing, receipt writes, polling, acknowledgements, setup-mode entry, and Wi-Fi failures rather than generic host monitoring. Telemetry flows through an OpenTelemetry Collector or Grafana Alloy gateway before Grafana Cloud so tail sampling, batching, retries, and cloud credentials stay outside individual application processes.

### Key decisions

- We require collector-side tail sampling because the .NET SDK cannot reliably keep all failed traces while dropping successful traces before the outcome is known.
- We keep all error traces and only ten percent of successful traces to preserve failure diagnostics while limiting normal traffic volume.
- We define "receipt written" as successful ESC/POS bytes written to the configured printer file device because the daemon cannot prove physical paper output.

## V2 architecture sanity check

Reviewed the v2 BYOD architecture for consistency across daemon setup, tenant scoping, Zitadel-backed authentication, print stations, BYOD constraints, all-in-one deployments, and observability. The document now separates daemon identity from tenant authority, treats Cashregister as the owner of daemon credential metadata rather than identity-provider secrets, makes print-job payload ownership explicit, and aligns observability language with outbound OTLP telemetry instead of inbound daemon scraping.

### Key decisions

- We keep daemon credentials tenant-neutral and derive tenant authority only from Cashregister print-station assignment because reassignment should not require issuing a new identity-provider credential.
- We require durable print jobs to include an immutable print payload because daemons should execute leased work, not reconstruct receipt business content from mutable order state.
- We document unresolved decisions explicitly because daemon credential provisioning, print payload format, Zitadel tenant mapping, reprint semantics, and offline all-in-one behavior need owner input before implementation.

## Repo-local multi-agent workflow

Added `.agents` coordinator, worker, reviewer, merger, skill, and workflow-template files for repo-local parallel feature coordination. The workflow is adapted to Cash Register's backend, Printmon, and UI surfaces and explicitly keeps canonical runtime behavior in the existing project docs and source code.

### ExecPlan

`plans/repo-local-multi-agent-workflow.md`

### Key decisions

- We kept `.agents` non-canonical because the repository already has dedicated source-of-truth documents for architecture, ESC/POS behavior, conventions, plans, and diary history.
- We created backend, Printmon, and UI worker/reviewer pairs because those are the independently assignable implementation surfaces.
- We skipped runtime verification because the change adds documentation and workflow tooling only.

## Removed standalone convention docs

Removed `docs/CONVENTIONS.md` and `docs/DOCKER.md` after migrating their active backend, frontend, documentation-only, Docker, and shell-script guidance into `.agents/skills/*`. Product and runtime contracts remain in `docs/ARCH.md` and `docs/ESCPOS.md`; planning and bookkeeping remain in `AGENTS.md`, `docs/PLANS.md`, and `docs/DIARY.md`.

### ExecPlan

`plans/repo-local-multi-agent-workflow.md`

### Key decisions

- We moved agent-facing implementation practice into skills to avoid duplicating the same rules in both docs and `.agents`.
- We left historical diary and old ExecPlan references untouched because they document past repository states rather than active guidance.
- We skipped runtime verification because the change removes and updates documentation/workflow files only.
