# Add an article master-detail editor

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md` in this repository.

## Purpose / Big Picture

The articles page currently edits an article by opening a modal from an edit button in the table. After this change, the user can click an article row, keep the article list visible, and edit the selected article in a closable right-side detail panel. The page remains a cursor-based paginated list: loading more articles must continue to work, and any selected article must remain selected through reloads and pagination because the selection is encoded in the URL.

The supplied screenshot is concept art only. It shows the interaction shape: a master list on the left and a closable detail surface on the right. The implementation must not copy the screenshot verbatim, must not redesign the application, and must reuse the existing Cashregister styling and components where they already fit.

## Progress

- [x] (2026-04-26 12:20Z) Created the ExecPlan before code edits.
- [x] (2026-04-26 16:54Z) Replaced modal-based article editing with query-string row selection and a right-side detail panel.
- [x] (2026-04-26 16:54Z) Preserved article cursor pagination while a detail panel is open.
- [x] (2026-04-26 16:54Z) Updated focused frontend tests for loader, table, row, panel, form submission, and pagination behavior.
- [x] (2026-04-26 16:54Z) Updated `docs/DIARY.md` after implementation.
- [x] (2026-04-26 16:56Z) Ran required frontend and backend verification.

## Surprises & Discoveries

- Observation: The orders page already implements the target URL-state pattern on `/orders?orderId=<id>`, including a helper module for selection and close links.
  Evidence: `ui/app/routes/order-overview/order-overview.tsx` loads both the page and optional selected order, and `ui/app/routes/order-overview/url.ts` builds selection and close links while preserving the `until` cursor.

- Observation: Article list items currently contain the fields needed by the edit form, but the selected detail should still use the detail endpoint.
  Evidence: `ui/app/model.ts` defines `ArticleListItemDto` with `id`, `description`, and `priceInCents`, while `be/Cashregister.Api/Articles/Endpoints.cs` maps `GET /articles/{id}` to a separate detail DTO.

- Observation: The backend already exposes `GET /articles/{id}` and `POST /articles/{id}`.
  Evidence: `be/Cashregister.Api/Articles/Endpoints.cs` maps `GetArticle` and `ChangeArticle`, so the master-detail route does not require backend API changes.

- Observation: `GET /articles/{id}` returns backend `ArticleDto`, not `ArticleListItemDto`.
  Evidence: `be/Cashregister.Api/Articles/Models/ArticleDto.cs` defines the detail DTO separately from the paginated `ArticleListItemDto`, even though the fields currently match.

## Decision Log

- Decision: Use `/articles?articleId=<id>` as the selected-article state, preserving `until` when present.
  Rationale: This matches the order overview master-detail behavior, keeps reload and browser history meaningful, and avoids global state or a separate detail route.
  Date/Author: 2026-04-26 / Codex

- Decision: Keep article creation as the existing modal flow.
  Rationale: The request targets list-to-detail editing and explicitly says not to redesign the whole application. Creation is not row selection, and the existing modal is scoped and already tested.
  Date/Author: 2026-04-26 / Codex

- Decision: Build an article-specific detail panel that reuses `ArticleForm` rather than creating a new form implementation.
  Rationale: `ArticleForm` already owns money input conversion, fetcher submission, pending state, and success/error callbacks. Reusing it keeps behavior consistent and avoids duplicating money-entry code.
  Date/Author: 2026-04-26 / Codex

- Decision: Remove the table actions column as part of this feature.
  Rationale: Editing moves to row selection, and deletion belongs to the currently inspected detail panel rather than an always-visible row action.
  Date/Author: 2026-04-26 / Codex

- Decision: Fetch selected article details as `ArticleDto` through `GET /articles/{id}` instead of reusing `ArticleListItemDto`.
  Rationale: This matches the backend API signature and preserves the same list/detail boundary used by the orders master-detail implementation.
  Date/Author: 2026-04-27 / Codex

- Decision: Keep article deletion in scope for the detail panel, but implement it as a panel-local API call rather than an `ArticleForm` submit intent.
  Rationale: `ArticleForm` should stay responsible for create/edit fields. A route-action delete would revalidate `/articles?articleId=<deleted>` before the panel closes, causing an avoidable selected-detail 404. The panel can call `DELETE /articles/{id}`, navigate to the close URL after success, and report failures locally.
  Date/Author: 2026-04-27 / Codex

## Outcomes & Retrospective

Implemented the articles route as a close counterpart to the orders overview route. The loader now fetches the page and optional selected article independently, the selected row is URL-backed through `articleId`, and the right-side panel reuses the existing `ArticleForm` for edits. The selected detail is typed as `ArticleDto` to match `GET /articles/{id}`. The table is now a pure selection surface with no actions column. Article creation remains modal-based. Article deletion lives in the detail panel as a panel-local API call and closes the panel after successful deletion. Targeted article tests, full frontend verification, and full backend verification all pass.

## Context and Orientation

The frontend lives under `ui/` and uses React Router v7, React 19, TypeScript, Tailwind CSS, and Vitest. Routes are registered in `ui/app/routes.ts`. The articles page is implemented in `ui/app/routes/articles/articles.tsx`.

The current articles route fetches a cursor page from `GET /articles`. A cursor is an opaque id-like string used to ask the backend for the next page. In this route, the URL parameter `until` carries that cursor. The route renders a header, action nav, table, loading spinner, footer "Load More" form, and two modals. One modal creates an article and the other edits an article selected through the row edit button.

The table is split across `ui/app/routes/articles/components/articles-table.tsx` and `ui/app/routes/articles/components/article-row.tsx`. `ArticlesTable` receives `ArticleListItemDto[]` and an `onEdit` callback. `ArticleRow` renders description, price, an edit button, and a disabled delete button. The edit button calls `onEdit(article)`, which opens the edit modal in `articles.tsx`.

`ui/app/routes/articles/components/article-form.tsx` is the existing article form. It uses React Router `useFetcher` to submit to `/articles`, sends an `intent` field of either `create` or `edit`, includes `articleId` for edit, and uses `MoneyInput` so users type decimal money while the submitted form field remains integer cents.

The orders page already demonstrates the intended master-detail mechanism. `ui/app/routes/order-overview/order-overview.tsx` parses both `until` and `orderId`, returns the selected id from the loader, lays out a left list and right detail panel, preserves `orderId` in the "Load More" form, and closes the panel by removing only the selected id. `ui/app/routes/order-overview/url.ts` contains small URL helper functions for selection and close links.

The backend article API already supports the operations needed by the current form. `be/Cashregister.Api/Articles/Endpoints.cs` maps `GET /articles`, `POST /articles`, `GET /articles/{id}`, `POST /articles/{id}`, and `DELETE /articles/{id}`. This task should not require backend contract changes or database migrations.

## Plan of Work

Start by adding URL helpers for the articles page in a new file `ui/app/routes/articles/url.ts`. Define `buildArticlesCloseLink(until: string | null): string` and `buildArticlesSelectionLink(articleId: string, until: string | null): string`. These functions should mirror the order overview helpers: preserve `until` when it exists, set or remove only `articleId`, and return `/articles` when no query string remains.

Next, update `ui/app/routes/articles/articles.tsx`. The loader should parse both `until` and `articleId`. It should continue calling `deps.apiClient.get<ArticlesPageDto>("/articles", until ? { until } : undefined)` for the page. When `articleId` is present, it should also fetch `deps.apiClient.get<ArticleDto>(`/articles/${articleId}`)` and return that result separately from the paginated list result. In the component, derive `page` from the page result and derive `selectedArticle` from the selected-detail result. If selected-detail loading fails, render no panel and let `useLoaderError` surface the failure, while preserving URL selection state.

Then replace the page layout with the same high-level structure used by the orders page: a fixed header and action nav, then a flexing main area. The left side contains the existing table, spinner, and footer. The right side renders an `aside` only when `selectedArticle` exists. Keep widths and classes close to the orders panel, for example `w-[24rem] min-w-[24rem] border-l bg-white`, unless the articles form requires minor spacing adjustments.

Update pagination in `articles.tsx`. The "Load More" form must include `until=page.next` as it does today. When `selectedArticleId` exists, it must also include a hidden `articleId` input so loading more does not close the panel. The loading state should distinguish pagination from row selection in the same spirit as orders: the footer button is disabled and says "Loading..." only when the active navigation is for the current load-more cursor, not merely because the user selected a row.

Remove edit-modal state from `articles.tsx`. Delete `isEditOpen`, `openEdit`, `closeEdit`, `editKey`, `editingArticle`, and `openEditModal`. Keep create-modal state and the create `ArticleForm` because creation remains modal-based. Keep `useErrorMessages` for create errors and for the detail panel if it needs a callback.

Change `ArticlesTable` in `ui/app/routes/articles/components/articles-table.tsx` so it receives `selectedArticleId: string | null` and `until: string | null` instead of `onEdit`. Pass `selected={selectedArticleId === article.id}` and `until` to each row. Keep the empty state.

Change `ArticleRow` in `ui/app/routes/articles/components/article-row.tsx` so the row is a selection surface. It should compute `to = buildArticlesSelectionLink(article.id, until)`, render description and price as block links to that URL, and apply a selected background class such as `bg-blue-100`, matching the order row. Remove the edit button. Keep the disabled delete button only if its presence does not make row selection awkward; if kept, it should remain disabled and should not claim to perform this feature's edit behavior. If the actions column becomes meaningless after removing the edit button, remove the actions column and update table tests accordingly. Prefer the simpler table if tests and layout support it.

Add `ui/app/routes/articles/components/article-detail-panel.tsx`. This component should accept `article: ArticleDto` and `closeTo: string`. Its header should show the article description, a small article id line, and a close link with `aria-label="Close article details"`. Its body should render `ArticleForm` with `intent="edit"`, `articleId={article.id}`, and initial description and price. On successful edit submit, keep the panel open; the loader revalidation should refresh the list. On edit error, use the existing error-message system. Deletion belongs to the panel, not the form: the panel should call `deps.apiClient.del("/articles/{id}")`, disable only the delete button while pending, navigate to `closeTo` after success, and report failures through the error-message system. Avoid a nested modal-specific close button in this panel; if `ArticleForm` currently requires modal behavior for its Cancel button, adjust `ArticleForm` minimally to support an optional non-modal cancel target or hide cancel when embedded.

If `ArticleForm` needs adjustment, keep it general but narrow. A reasonable change is to add optional props such as `cancelTo?: string` or `showCancel?: boolean`. In modal usage, retain the current button with `command="close"` and `commandfor={modalId}`. In panel usage, render a `Link` or ordinary button that closes via the supplied URL. Do not duplicate the form just to change the cancel control.

Update tests. `ui/app/routes/articles/articles.test.tsx` should cover loader parsing for `until` and `articleId`, rendering the detail panel when a selected article is in the page, preserving `articleId` in the load-more form, closing the panel through a link that preserves `until`, and retaining create modal behavior. `article-row.test.tsx` should cover selection links, selected styling, formatted price, and the disabled delete behavior if the delete button remains. `articles-table.test.tsx` should cover forwarding selected state and empty state. Add `article-detail-panel.test.tsx` for panel rendering and edit form submission behavior if that behavior is not already covered through the route test.

After source changes, update `docs/DIARY.md` with a concise entry. Include an `ExecPlan` section referencing `plans/article-master-detail.md`, because this task uses an ExecPlan. Do not update `docs/ARCH.md` or `docs/ESCPOS.md`; this is a route interaction change and does not alter high-level architecture or printing behavior.

## Concrete Steps

Implement in small slices and run targeted tests while working:

    cd ui
    npm run test -- app/routes/articles

After the feature works locally, run the required project verification:

    cd be
    dotnet format
    dotnet build
    dotnet test

    cd ../ui
    npm run lint
    npm run build
    npm run test

For manual inspection, run the backend and frontend development servers using the existing project commands, navigate to `/articles`, click an article row, confirm the URL becomes `/articles?articleId=<id>`, edit the article from the right panel, and confirm the list updates without navigating to a separate page. With a selected article open and a next cursor available, click "Load More" and confirm the URL still contains `articleId=<id>`.

## Validation and Acceptance

The behavior is accepted when `/articles` still shows the paged article list and create actions, clicking an article row opens a right-side detail panel without leaving `/articles`, the panel can be closed, and the selected article is represented by `articleId` in the URL.

Editing is accepted when changing description or price in the detail panel submits the existing edit action to `POST /articles/{id}`, uses the existing money input behavior, reports errors through the existing error-message system, and refreshes the visible list after success.

Pagination is accepted when "Load More" still submits the cursor-based `until` parameter and preserves `articleId` while a panel is open. Loading more must not accidentally close the panel or disable the button during unrelated row-selection navigation.

Automated acceptance requires the updated articles route and component tests to pass. Full task acceptance requires all backend and frontend verification commands listed above to pass.

## Idempotence and Recovery

All edits are ordinary source and documentation changes. No database migrations, destructive commands, or backend contract changes are expected. Re-running tests and build commands is safe. If a partial implementation leaves the edit form tied too tightly to modal behavior, recover by extending `ArticleForm` with a minimal optional cancel behavior rather than creating a second form implementation.

If preserving selection from the list DTO proves insufficient because the selected article can disappear from the loaded page, switch the loader to fetch `GET /articles/{articleId}` in parallel with the page request, mirroring orders. If this change is made, update the loader shape, the plan, and tests to cover failed selected-detail loads through `useLoaderError`.

## Artifacts and Notes

The plan was created after inspecting these files:

    docs/ARCH.md
    docs/CONVENTIONS.md
    docs/PLANS.md
    docs/plans/order-master-detail.md
    ui/app/routes/articles/articles.tsx
    ui/app/routes/articles/components/articles-table.tsx
    ui/app/routes/articles/components/article-row.tsx
    ui/app/routes/articles/components/article-form.tsx
    ui/app/routes/order-overview/order-overview.tsx
    ui/app/routes/order-overview/url.ts
    be/Cashregister.Api/Articles/Endpoints.cs
    be/Cashregister.Api/Articles/Handlers.cs

Expected implementation remains frontend-only. If backend changes become necessary, revise this plan before making them and add backend tests under `be/Cashregister.Tests.Integration`.

## Interfaces and Dependencies

No public backend API changes are required for the initial implementation.

In `ui/app/routes/articles/url.ts`, define:

    export function buildArticlesCloseLink(until: string | null): string

    export function buildArticlesSelectionLink(
      articleId: string,
      until: string | null,
    ): string

In `ui/app/model.ts`, define an article detail DTO matching the backend detail response:

    export interface ArticleDto {
      id: string;
      description: string;
      priceInCents: number;
    }

In `ui/app/routes/articles/articles.tsx`, the loader should return a route-local shape equivalent to:

    {
      articlesPage: Result<ArticlesPageDto>,
      selectedArticle: Result<ArticleDto> | null,
      selectedArticleId: string | null,
      until: string | null
    }

In `ui/app/routes/articles/components/article-detail-panel.tsx`, define a component that accepts:

    article: ArticleDto
    closeTo: string

The component should reuse `ArticleForm` for edit submission. If `ArticleForm` is extended, keep existing modal call sites source-compatible or update them narrowly in `articles.tsx`.

Revision note: 2026-04-26. Initial ExecPlan created from the user request and repository inspection. The main design choice is to mirror the existing orders master-detail pattern while keeping article creation modal-based and article editing inline in the detail panel.
