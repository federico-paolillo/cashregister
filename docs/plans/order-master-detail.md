# Add an order master-detail view

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md` in this repository.

## Purpose / Big Picture

The orders page currently behaves as a list that sends the user to a separate order detail route when an order is selected. After this change, the user can inspect a saved order without leaving the order list: clicking an order row selects it and opens a right-side detail panel. The selected order is encoded in the URL so browser reloads and history preserve the selection, and the existing cursor-based "Load More" pagination continues to work.

The screenshot supplied with the request is concept art only. The implementation must keep the existing Cashregister styling conventions and must not introduce a new design system.

## Progress

- [x] (2026-04-25 17:06Z) Created the ExecPlan before code edits.
- [x] (2026-04-25 17:17Z) Updated the order overview loader to return the orders page result and optional selected order result.
- [x] (2026-04-25 17:17Z) Replaced table navigation to `/order/:id` with in-place selection links under `/orders?orderId=<id>`.
- [x] (2026-04-25 17:17Z) Added a new read-only persisted-order detail panel component with reprint behavior.
- [x] (2026-04-25 17:17Z) Updated route and component tests for selection, pagination preservation, detail rendering, and reprint behavior.
- [x] (2026-04-25 17:17Z) Updated `docs/DIARY.md` after implementation.
- [x] (2026-04-25 17:17Z) Ran required backend and frontend verification.

## Surprises & Discoveries

- Observation: The order overview now has two distinct loader-triggering interactions, selecting an order and loading another page, but only the pagination submission should change the footer button state.
  Evidence: The implementation uses `useNavigation().formData?.get("until") === page?.next` to distinguish "Load More" from a row-selection reload, and `order-overview.test.tsx` covers both cases.

## Decision Log

- Decision: Use `/orders?orderId=<id>` as the selected-order state.
  Rationale: This preserves reloads, browser history, and cursor pagination without adding global state or a new route.
  Date/Author: 2026-04-25 / Codex

- Decision: Build a new read-only order detail component instead of adapting the make-order summary.
  Rationale: The make-order component represents mutable cart state, while this feature displays a persisted order. Sharing would blur semantics and make the code harder to reason about.
  Date/Author: 2026-04-25 / Codex

- Decision: Keep `/order/:orderId` registered for compatibility but stop linking to it from the orders table.
  Rationale: This avoids breaking direct links while making the order list use the new master-detail behavior.
  Date/Author: 2026-04-25 / Codex

- Decision: Keep `OrderItemsList` as the only reused read-only child component and do not refactor the standalone `/order/:orderId` route into the new panel component.
  Rationale: The requested constraint was to avoid a broader shared order-detail abstraction. Reusing the existing items list is low-risk, but collapsing the standalone route into the panel component would broaden the change without a strong payoff.
  Date/Author: 2026-04-25 / Codex

## Outcomes & Retrospective

The `/orders` screen now behaves as a master-detail view. Selecting a row keeps the user on `/orders`, opens a closable right-side panel, preserves selection in the query string, and keeps cursor pagination working. Receipt reprinting moved from the table row into the panel, which makes the list a pure selection surface.

The implementation stayed frontend-only. Backend endpoints and contracts were already sufficient. Verification passed across both frontend and backend command sets, so the change does not introduce known repository-wide regressions.

## Context and Orientation

The frontend lives under `ui/` and uses React Router v7, React 19, TypeScript, Tailwind CSS, and Vitest. The `/orders` route is implemented by `ui/app/routes/order-overview/order-overview.tsx`. It currently fetches `OrdersPageDto` from `/orders`, renders `OrdersTable`, and provides a "Load More" form using the `until` cursor query parameter.

The order table is split into `ui/app/routes/order-overview/components/orders-table.tsx` and `ui/app/routes/order-overview/components/order-row.tsx`. Each row currently links to `/order/:orderId`, and the row also owns the receipt reprint button.

The full order DTO already exists in `ui/app/model.ts` as `OrderDto`. The backend already exposes `GET /orders/{id}` and `POST /orders/{id}/print`, so this feature does not require backend API changes.

The standalone detail route is `ui/app/routes/order-view/order-view.tsx`. It may remain as direct-link compatibility. The new master-detail panel belongs to the order overview route because it is part of that screen's interaction model.

## Plan of Work

First, change the `/orders` client loader so it parses both `until` and `orderId`. It must request the page with only the pagination parameter and, when `orderId` exists, also request the selected order details. Return an object containing the page result, the selected order result or `null`, and the selected order id or `null`.

Next, update `OrderOverview` so the screen is a horizontal flex layout. The left side contains the existing header, table, spinner, and footer. The right side renders only when an order detail result exists and contains a new read-only order detail component. The "Load More" form must include a hidden `orderId` input when a selection is active.

Then, update `OrdersTable` and `OrderRow`. The table must receive the current selected order id and the current pagination cursor. The row must render selection links to `/orders?orderId=<id>` and preserve `until` when present. It must apply a selected background style when its id matches the selected id. The row must no longer own receipt reprint behavior.

Then, add `ui/app/routes/order-overview/components/order-detail-panel.tsx`. This component must render persisted order details only: order number, order id, formatted date, item list, total, optional overridden total, a close link that removes `orderId` while preserving `until`, and a reprint button. The reprint button must post to `/orders/:id/print`, disable while pending, and report success/failure through `useErrorMessages`.

Finally, update colocated frontend tests to reflect the new behavior, then update `docs/DIARY.md`.

## Concrete Steps

Run targeted frontend tests while implementing:

    cd ui
    npm run test -- app/routes/order-overview

After the feature is complete, run the repository-required verification:

    cd be
    dotnet format
    dotnet build
    dotnet test

    cd ../ui
    npm run lint
    npm run build
    npm run test

## Validation and Acceptance

The behavior is accepted when `/orders` still shows the paged order list, clicking an order row opens a right-side panel without leaving `/orders`, the panel can be closed, and the selected order remains selected through reload because `orderId` is in the URL.

Pagination is accepted when using "Load More" with a selected order preserves `orderId` while advancing the `until` cursor.

Reprint behavior is accepted when the panel's reprint button posts to `/orders/:id/print`, disables while the request is pending, and uses the existing error-message system for success and failure messages.

Automated acceptance is the frontend test suite passing with new or updated tests that cover loader behavior, row selection links, selected-row styling, panel rendering, panel close URL, pagination preservation, and reprint behavior.

Full task acceptance requires the backend and frontend verification commands listed above to pass.

## Idempotence and Recovery

All code edits are ordinary source changes and can be repeated safely by re-running the tests. No database migrations, backend schema changes, or destructive commands are required. If a frontend test fails after a partial edit, use the failing assertion to restore the expected URL and component behavior rather than changing backend contracts.

## Artifacts and Notes

Verification evidence:

    cd ui
    npm run test -- order-overview
    Result: 4 test files passed, 31 tests passed.

    cd ui
    npm run lint
    npm run build
    npm run test
    Result: all commands passed; frontend suite passed with 27 test files and 208 tests.

    cd be
    dotnet format
    dotnet build
    dotnet test
    Result: all commands passed; backend test suites passed with 150 + 106 + 117 tests.

## Interfaces and Dependencies

No public backend API changes are required.

In `ui/app/routes/order-overview/order-overview.tsx`, the loader should return a route-local shape equivalent to:

    {
      ordersPage: Result<OrdersPageDto>,
      selectedOrder: Result<OrderDto> | null,
      selectedOrderId: string | null,
      until: string | null
    }

In `ui/app/routes/order-overview/components/order-detail-panel.tsx`, define a component that accepts:

    order: OrderDto
    closeTo: string

The component computes display values locally using existing `formatPrice` and `new Date(order.date * 1000).toLocaleString()`.

Revision note: 2026-04-25. Updated this ExecPlan after implementation to record completed progress, the navigation-state distinction discovered during the work, and the final verification evidence.
