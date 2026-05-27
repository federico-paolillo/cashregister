## Executive Summary

Overall score: 7/10.

Cash Register is a well-structured small application with an intentionally enterprise-heavy backend. The codebase generally respects its documented boundaries: domain objects are isolated, application ports separate persistence from use cases, API handlers use typed results in most places, frontend DTOs and API access are centralized, and tests cover a meaningful amount of behavior. The current uncommitted article-list change is consistent with the architecture: `printDetailReceipt` flows from database projection to application output model to API DTO, with integration coverage.

The main quality risks are not the enterprise patterns themselves; they are places where the implementation undercuts the guarantees those patterns imply. The biggest examples are post-commit print failure being reported as order-creation failure, EF rollback being a no-op behind a unit-of-work abstraction, and API boundary code allowing user input to throw before typed bad-request mapping. Frontend quality is solid, but the order and bulk-create flows inherit backend ambiguity around partial success, and a few accessibility and error-message paths are weaker than the rest of the application.

## Backend review findings

**High severity**

- `be/Cashregister.Activities/PlaceOrderActivity.cs:19` commits the order in `IPlaceOrderTransaction` before `be/Cashregister.Activities/PlaceOrderActivity.cs:29` prints the receipt, while `be/Cashregister.Api/Orders/Handlers.cs:92` maps any later failure to the order action result. If printing fails, the API can return failure even though the order already exists, making client retries likely to duplicate orders. Remediation: split order creation from printing, or return an explicit created-but-print-failed outcome that includes the created order identity and routes recovery through reprint.
- `be/Cashregister.Api/Articles/Handlers.cs:78`, `be/Cashregister.Api/Articles/Handlers.cs:139`, and `be/Cashregister.Api/Orders/Handlers.cs:86` construct `Cents` directly from request values. `be/Cashregister.Domain/Cents.cs:7` throws for negative input, so normal client validation failures can bypass typed HTTP results and become server errors. Remediation: validate DTO primitives at the API boundary or add non-throwing value-object parsing that returns `Result<T>`/`Problem`, then add negative-price and negative-total-override API tests.

**Medium severity**

- `be/Cashregister.Database/Entities/OrderEntity.cs:32` and `be/Cashregister.Database/Entities/OrderItemEntity.cs:40` configure two relationships between orders and order items. This creates divergent semantics between navigation-based reads such as `FetchOrderQuery`/`FetchOrdersListQuery` and explicit `OrderId` queries such as statistics. Remediation: map exactly one relationship, preferably `OrderItemEntity.OrderId` as the FK for `OrderEntity.Items`, migrate away the duplicate FK/index, and add a persistence-model test that prevents shadow relationship drift.
- `be/Cashregister.Commons/Transaction.cs:24` promises rollback on failed results, but `be/Cashregister.Database/ApplicationDbContext.cs:26` implements rollback as a no-op. That is only safe when the scoped context is immediately disposed and never later saved. Remediation: implement real EF transactions or clear the change tracker on rollback, then test a failed mutation followed by a successful save in the same scope.
- `be/Cashregister.Commons/Transaction.cs:31` catches every exception and returns `UnhandledExceptionProblem`. That turns programmer errors and infrastructure failures into application flow, contrary to the convention that `Result<Problem>` represents expected failures. Remediation: rollback and rethrow unexpected exceptions, or log centrally before rethrowing; reserve `Problem` for known domain/application outcomes.
- `be/Cashregister.Printmon/Devices/FileDevice.cs:27` lets production device I/O exceptions escape even though `DeviceIoProblem` exists and `MarkdownDevice` models I/O as `Result<Unit>`. Remediation: wrap open/write failures in a device problem and cover disappearing or unwritable targets in device tests.
- `be/Cashregister.Printmon/PrintProgramBuilder.cs:325` and `be/Cashregister.Printmon/PrintProgramBuilder.cs:369` cast duration units to `byte` before validating upper bounds. Large drawer-pulse durations can wrap into valid-looking ESC/POS values. Remediation: compute in `int`/`double`, validate the full range, then cast; add boundary tests.
- `be/Cashregister.Database/Extensions/ServiceCollectionExtensions.cs:66` enables detailed EF errors and sensitive data logging unconditionally. That is development-only behavior in a runtime registration path and can leak persisted values in logs. Remediation: gate these options on development environment or remove them from production service registration.

**Low severity**

- `be/Cashregister.Api/Articles/Handlers.cs:158` and `be/Cashregister.Api/Articles/Handlers.cs:172` still use `StatusCodeHttpResult` and `TypedResults.StatusCode(...)`, directly violating the Minimal API convention. Remediation: use `Results<NotFound, InternalServerError, NoContent>` and `TypedResults.InternalServerError()`.
- `be/Cashregister.Database/Queries/PaginationQuery.cs:21`, `be/Cashregister.Database/Queries/FetchArticlesListQuery.cs:13`, and `be/Cashregister.Database/Queries/FetchOrdersListQuery.cs:47` omit explicit `AsNoTracking()` on read-only query roots, unlike nearby query implementations and the persistence convention. Remediation: apply no-tracking consistently to read-only query paths.
- `be/Cashregister.Cli/Tools/TestTool.cs:36` ignores the `IDevice.PrintAsync` result and always exits `0`. Remediation: check `NotOk`, write a concise error, and return nonzero.

## Frontend review findings

**High severity**

- `ui/app/routes/order/order.tsx:37` and `ui/app/routes/order/order.tsx:111` treat any order action failure as not-created and keep the cart intact. Because the backend can commit an order before print failure, retrying can duplicate orders. Remediation: consume an explicit backend outcome that distinguishes not-created from created-but-print-failed; clear the cart for committed outcomes and send print recovery through reprint.
- `ui/app/routes/articles-bulk/articles-bulk.tsx:18` fires independent create requests, while `ui/app/routes/articles-bulk/articles-bulk.tsx:33` and `ui/app/routes/articles-bulk/articles-bulk.tsx:99` only report a generic failure count. Successful rows remain retryable, so retry can duplicate already-created articles. Remediation: add an atomic bulk-create API or return row-level outcomes and remove/lock successful rows before retry.

**Medium severity**

- `ui/app/api-client.ts:66` discards response bodies on HTTP failure and exposes the URL as the user-facing message; `ui/app/api-client.test.ts:99` locks in that behavior. Remediation: parse problem JSON or text when present, fall back to `HTTP <status> <statusText>` plus path, and keep the numeric status for callers.
- `ui/app/routes/order/order.tsx:18` hard-caps the order article list at `pageSize: 500`, and `ui/app/routes/order/order.tsx:90` ignores `hasNext`. More than 500 orderable articles become invisible with no warning. Remediation: add search/pagination to the selector, introduce a dedicated bounded orderable-articles contract, or surface overflow explicitly.
- `ui/app/routes/order/components/article-selector.tsx:21` sets `aria-label` to only the article description, so status icons inside the button are not announced. Remediation: expose low-stock/detail-receipt status through `aria-describedby` or deliberately include status text in the accessible name.
- `ui/app/routes/articles/components/article-row.tsx:20` and `ui/app/routes/order-overview/components/order-row.tsx:20` repeat the same row destination link in every cell, creating multiple tab stops and ambiguous link names such as prices, dates, and `-`. Remediation: link only the identifying cell or provide one explicit row action with a descriptive accessible name.

**Low severity**

- `ui/app/components/spinner.tsx:1` renders visual loading state without `role="status"` or an accessible label. Remediation: add a status role and hidden/loading label, and mark the animated circle itself as decorative.
- `ui/app/routes/articles-bulk/components/bulk-row.tsx:20` repeats identical labels such as `Description`, `Price`, and `Remove` across rows. Remediation: use a `fieldset`/legend or row-specific accessible names so form navigation and removal actions are unambiguous.
