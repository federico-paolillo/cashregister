# Codebase Review

Last review: 2026-03-16

This is a living document. It will contain past review items, any action taken to address one and the current status. 

Each item has a severity of HIGH, MEDIUM and LOW in parenthesis after the title. Each item has a suggested `**Fix:**` and a `**Status:**`. Status is TODO, DONE, WONT. If a status is either DONE or WONT a `**Decision:**` explains briefly how an item was addressed or why it will not be done

As new reviews of the codebase come in, they must take into account existing items and their status and append any new findings to the appropriate list.

At the bottom there is a summary of the review on important items to address and an overall (100-200 words) evaluation of the architecture.

## Review prompt

You need to perform a thorough review of the codebase. 

The codebase has two main components: backend (`be/` folder) and frontend (`ui/` folder). Review AGENTS.md for further context.

You have to evaluate the codebase for the following criterias:

- Consistency within the codebase implementations
- Idiomacy to the respective framework and "tribal knowledge"
- Modern syntax choices
- Bugs
- Smells
- Cleanliness
- Application of pattern and practices
- Point out inconsistencies between the code and AGENTS.md

The application will be used in the following context:

> It is a town fair cash register deployed on a local machine and used by a single user

The backend architecture design decisions where guided by the following line of reasoning:

> The backend is to be considered a playground to try out patterns and practices for enterprise applications and more. There are some over-engineered implementations that are due to the nature of the exercise. The goal is to write a complete, enterprise-grade and reliable backend that can be showed off as a portfolio project.

## 1. Bugs

### 1.1 `OrderListItemMapper` ignores `TotalOverride` (HIGH)

`be/Cashregister.Database/Mappers/OrderListItemMapper.cs:18` computes the total as `entity.Items.Sum(i => i.Quantity * i.Price)` and never checks `entity.TotalOverride`. The sibling query `FetchOrdersListQuery.cs:27-29` correctly uses a ternary on `TotalOverride`. The full-entity mapper `OrderEntityMapper.cs:32` also handles it. This means the same order can report different totals depending on which code path fetches it.

**Fix:** Add the `TotalOverride` check to `OrderListItemMapper.FromEntity`, mirroring the projection in `FetchOrdersListQuery`.
**Status:** DONE
**Decision:** Added the `TotalOverride` check to `OrderListItemMapper.FromEntity`.

### 1.2 Frontend edit action uses POST instead of PUT (MEDIUM)

`ui/app/routes/articles/articles.tsx:54` sends the edit request as `deps.apiClient.post(/articles/${articleId}, body)`. On the backend, `POST /{id}` is mapped to `ChangeArticle`, so it works. But `POST` to a resource URL idiomatically means "create a subordinate" in REST. Both the frontend and backend cooperate on this non-standard verb.

**Fix:** Change the backend endpoint to `MapPut("/{id}", Handlers.ChangeArticle)` and add a `put` method to `ApiClient`, or accept this deviation and document it in AGENTS.md.
**Status:** WONT
**Decision**: Adhesion to the RESTful is out of scope. We will use only the HTTP methods GET, UPDATE and DELETE. This is to keep the API simple and not wade into RESTful philosophic discussions.

### 1.3 Frontend price mismatch on article edit (MEDIUM)

`ui/app/routes/articles/articles.tsx:150` passes `priceInCents: editingArticle.price * 100` as initial data to the edit form. But `editingArticle.price` is already the decimal value returned by `Cents.AsPayableMoney()` (e.g. `3.50`), so `* 100` converts it back to cents. However, `AsPayableMoney()` rounds to the nearest 0.05 CHF. If the original stored value was `349` cents, the API returns `3.45` (rounded to 0.05), and the edit form pre-fills `345` - losing 4 cents. This is only a problem if the rounding in `AsPayableMoney` actually changes the value and the user submits the edit without correcting it.

**Fix:** The cashier is aware that whatever price in cents it enters it will be rounded to 0.05. We should ensure that this rounding is enforced by the domain and application layer before persistence. Do not make the database aware of this concern.
**Status:** DONE
**Decision:** Enforced 5-cent rounding in the `Cents.From` factory method. All `Cents` instances are now automatically rounded upon creation.

### 1.4 `Cents` allows negative values (LOW)

`be/Cashregister.Domain/Cents.cs` accepts any `long`, including negatives. `Identifier.From` validates non-null/whitespace, `TimeStamp.From` validates positive - but `Cents.From` has no guard. Negative prices would propagate silently.

**Fix:** Add `ArgumentOutOfRangeException.ThrowIfNegative(total)` in `Cents.From`.
**Status:** DONE
**Decision:** Added `ArgumentOutOfRangeException.ThrowIfNegative` to the `Cents.From` method.

## 2. Inconsistencies with AGENTS.md

### 2.1 AGENTS.md references `fe/` folder; actual frontend is `ui/` (LOW)

The user's task description says "frontend (`fe/` folder)" but the project structure in AGENTS.md and on disk uses `ui/`. If external documentation or onboarding material says `fe/`, newcomers will be confused.

**Fix:** Ensure all documentation consistently refers to `ui/`.
**Status:** WONT
**Decision:** AGENTS.md already uses `ui/` consistently. The `fe/` reference was in the review prompt, not in AGENTS.md.

### 2.2 AGENTS.md says `api-client.ts` is in `app/api/` (LOW)

AGENTS.md section "API Client" says "located in `app/api/`" but the actual file is `app/api-client.ts` (root of `app/`). There is no `app/api/` directory.

**Fix:** Correct AGENTS.md to say `app/api-client.ts`.
**Status:** DONE
**Decision:** Changed `app/api/` to `app/api-client.ts` in AGENTS.md.

### 2.3 AGENTS.md mentions `use-articles-page.ts` hook; it does not exist (LOW)

The AGENTS.md file references `ui/app/components/use-articles-page.ts` as an "infinite scroll hook". This file does not exist in the codebase. Pagination is handled inline in the articles route.

**Fix:** Remove the stale reference from AGENTS.md.
**Status:** WONT
**Decision:** AGENTS.md does not reference `use-articles-page.ts`. The stale reference is in MEMORY.md, not AGENTS.md.

### 2.4 Receipts module registered but not wired (MEDIUM)

`Cashregister.Application.Receipts.Extensions.ServiceCollectionExtensions` defines `AddCashregisterReceipts()`, but `Program.cs` never calls it. The `PlaceOrderActivity` depends on `IPrintReceiptTransaction` via `Scoped<>`, but `PlaceOrderActivity` is also never registered or used from the API. AGENTS.md lists `escpos/` as a project but no ESC/POS library exists in the repo.

**Fix:** Either wire up the receipts/activity module end-to-end or remove the dead code. Update AGENTS.md to reflect the actual state.
**Status:** WONT
**Decision:** We will implement this feature later

### 2.5 AGENTS.md project structure omits `Cashregister.Commons` and `Cashregister.Activities` (LOW)

The "Project Structure" tree in AGENTS.md lists `Cashregister.Api/`, `Cashregister.Application/`, `Cashregister.Database/`, `Cashregister.Domain/`, and `Cashregister.Tests.*/` but does not mention `Cashregister.Commons` or `Cashregister.Activities`. Both are real projects in the `be/` folder. A reader relying on AGENTS.md to understand the backend would not know these exist.

**Fix:** Add both projects to the AGENTS.md tree. Describe `Cashregister.Commons` as the cross-cutting infrastructure (Result, Transaction, UoW, Scoped) and `Cashregister.Activities` as the orchestration layer (pending implementation).
**Status:** DONE
**Decision:** Added both projects to the AGENTS.md project structure tree. Also removed the stale `escpos/` entry which no longer exists on disk.

### 2.6 AGENTS.md frontend directory tree is incomplete (LOW)

The tree in AGENTS.md omits several files that exist on disk: `money.ts`, `settings.ts`, `routes/order/components/article-selector.tsx`, and `routes/order/components/order-summary.tsx`. The tree implies the order route has no sub-components.

**Fix:** Add the missing files to the AGENTS.md directory tree.
**Status:** DONE
**Decision:** Added `money.ts`, `settings.ts`, `routes/order/components/article-selector.tsx`, and `routes/order/components/order-summary.tsx` to the AGENTS.md frontend directory tree.

### 2.7 AGENTS.md code example uses `~/deps` import path (LOW)

The API Client usage example in AGENTS.md shows `import { deps } from "~/deps"`, but the actual codebase uses the `@cashregister/*` path alias configured in `tsconfig.json` (e.g., `import { deps } from "@cashregister/deps"`). The `~` alias is not configured.

**Fix:** Change the example to `import { deps } from "@cashregister/deps"`.
**Status:** DONE
**Decision:** Updated the import path in the AGENTS.md code example.

### 2.8 `PageRequest.After` is `required` but nullable (LOW)

`be/Cashregister.Application/Pagination/PageRequest.cs:7` declares `public required Identifier? After`. The `required` keyword forces callers to set the property, but it can be set to `null`. This is valid C# but semantically odd - `required` suggests the value matters, yet `null` is the common case (first page). `Until` does not use `required`, creating an inconsistency between the two optional cursor fields.

**Fix:** Remove `required` from `After` to match `Until`, or add `required` to `Until` for consistency. As both cannot be specified there must be an application layer validation that forbids this. To do that we might need to move away from `init` properties.
**Status:** TODO

## 3. Smells

### 3.1 Blanket `BadRequest` on all pagination handler errors (MEDIUM)

`be/Cashregister.Api/Articles/Handlers.cs:44-46` and `be/Cashregister.Api/Orders/Handlers.cs:43-45` return `TypedResults.BadRequest()` for any `Result.NotOk` from the pagination handlers. The `ChangeArticle` and `DeleteArticle` handlers correctly distinguish `NoSuchArticleProblem -> 404` from other errors `-> 500`. The pagination handlers don't follow this pattern.

**Fix:** Apply the same `result.Error switch { ... }` pattern used in `ChangeArticle` to the pagination handlers, or at minimum return 500 for unexpected errors.
**Status:** TODO

### 3.2 Blanket `BadRequest` on order creation errors (MEDIUM)

`be/Cashregister.Api/Orders/Handlers.cs:89-91` returns `BadRequest` when order creation fails. The `PlaceOrderTransaction` can fail with `OrderRequestIsMissingSomeArticles` (a client error - 400 is correct) but could also fail with `UnhandledExceptionProblem` from the base `Transaction` class (which should be 500). All are returned as 400.

**Fix:** Pattern match on the error type: `OrderRequestIsMissingSomeArticles -> 400`, `_ -> 500`.
**Status:** TODO

### 3.3 `RegisterArticle` handler returns `BadRequest` for all errors (MEDIUM)

`be/Cashregister.Api/Articles/Handlers.cs:78-80` returns `BadRequest` when registration fails. Registration currently has no expected application-level error - the only failures would be infrastructure (DB errors). These would be wrapped as `UnhandledExceptionProblem` by the `Transaction` base class and returned as 400, masking a 500-level issue.

**Fix:** Return `StatusCode(500)` for unexpected errors, same as `ChangeArticle` does.
**Status:** TODO

### 3.4 `PlaceOrderDto.cs` filename vs content (LOW)

`be/Cashregister.Api/Orders/Models/PlaceOrderDto.cs` defines `OrderRequestDto` and `OrderRequestItemDto`, not `PlaceOrderDto`. The filename doesn't match its content.

**Fix:** Rename the file to `OrderRequestDto.cs` or rename the types to `PlaceOrderDto`/`PlaceOrderItemDto`.
**Status:** TODO

### 3.5 `OrderNumber` pads to 20 digits (LOW)

`be/Cashregister.Domain/OrderNumber.cs:7` formats the number as `D20`, producing strings like `00000000000000000001`. For a town fair register this is excessive; 6-8 digits would be more readable on receipts.

**Fix:** Reduce the padding to a practical width (e.g. `D6`). 6 digits is sufficient.
**Status:** TODO

### 3.6 Order route fetches all articles with `pageSize: "500"` magic number (MEDIUM)

`ui/app/routes/order/order.tsx:17` hardcodes `pageSize: "500"`. If the catalog grows beyond 500 articles some will be invisible in the order screen with no indication. There's also no pagination or search for the article selector.

**Fix:** Implement proper pagination handling with "infinite scroll" like `/articles` route
**Status:** TODO

### 3.7 The `Activities` project is orphaned (LOW)

`be/Cashregister.Activities/PlaceOrderActivity.cs` orchestrates order placement + receipt printing, but is never referenced from the API project or registered in DI. It exists as dead code.

**Fix:** Integrate it into the API workflow or remove it until needed.
**Status:** WONT
**Decision:** We will implement this feature later

### 3.8 `EnableSensitiveDataLogging()` is always on (LOW)

`be/Cashregister.Database/Extensions/ServiceCollectionExtensions.cs:65` enables sensitive data logging unconditionally. In production this would leak field values (prices, descriptions) into logs.

**Fix:** Guard with `if (builder.Environment.IsDevelopment())` or make it configurable.
**Status:** WONT
**Decision:**: The application runs locally to the machine. It it not a concern and helps debugging

### 3.9 `SaveArticleCommand.SaveAsync(RetiredArticle)` silently no-ops on missing article (LOW)

`be/Cashregister.Database/Commands/SaveArticleCommand.cs:39-42`: if `FindAsync` returns null, the method returns without error. The `RetireArticleTransaction` already checks existence before calling save, so this is defense-in-depth. However, if the two checks diverge (e.g., concurrent deletion), the retire silently succeeds while the article was already gone.

**Fix:** Log a warning when the article is not found, or throw to surface the inconsistency.
**Status:** TODO

### 3.10 `OrderItemEntity` relationship configured from both sides (MEDIUM)

The `OrderId` FK between `OrderItemEntity` and `OrderEntity` is configured twice: once in `OrderEntity.OrderEntityTypeConfiguration` as `HasMany(p => p.Items).WithOne().HasForeignKey(p => p.OrderId)` and again in `OrderItemEntity.OrderItemEntityTypeConfiguration` as `HasOne<OrderEntity>().WithMany().HasForeignKey(p => p.OrderId)`. The same duplication exists for the `ArticleId` FK between `ArticleEntity` and `OrderItemEntity`. EF Core can unify these by FK name, but the redundancy is confusing and fragile - a change to one side without the other could create a second shadow relationship.

**Fix:** Configure each relationship in one place only. The owning side (the entity with the collection navigation, e.g. `OrderEntity`) is the natural home.
**Status:** TODO

## 4. Consistency

### 4.1 Error handling pattern varies across API handlers (MEDIUM)

`ChangeArticle` and `DeleteArticle` use `result.Error switch { ... }` to map specific problems to specific HTTP status codes. `GetArticlesPage`, `RegisterArticle`, `GetOrdersPage`, and `CreateOrder` use a flat `return TypedResults.BadRequest()`. The codebase should pick one approach and apply it everywhere.

**Fix:** Adopt the `switch` pattern in all handlers. Even if there is only one error type today, the structure is in place for future growth and avoids masking 500s as 400s.
**Status:** TODO

### 4.2 DTO naming convention split (LOW)

Article DTOs use the file-per-type approach (`ArticleDto.cs`, `RegisterArticleRequestDto.cs`, `ChangeArticleRequestDto.cs`) except for the page DTO which bundles `ArticlesPageDto` and `ArticleListItemDto` in one file (`ArticlePageDto.cs`). Order DTOs follow the same pattern (`OrderDto.cs` bundles `OrderDto` and `OrderItemDto`, `PlaceOrderDto.cs` bundles request types). The rule "one type per file vs. bundled" is inconsistent.

**Fix:** Pick one one type per file. It is cleaner and easier to navigate.
**Status:** TODO

### 4.3 Handler naming inconsistency (LOW)

Article operations use `IFetchArticlesPageHandler` (a "handler" wrapping a `Paginator` call) while the actual mutation logic uses `IRegisterArticleTransaction`, `IChangeArticleTransaction`. Orders follow the same split. But the `Handlers` in the API layer are static methods, not the same concept as `IFetchArticlesPageHandler` in the Application layer. The word "Handler" is overloaded.

**Fix:** Rename the Application-layer read-only wrappers (those that dont need a database transaction) to something like `IFetchArticlesPageUseCase` to avoid confusion with the API handler methods.
**Status:** TODO

### 4.4 Mapper registration: singleton vs scoped (LOW)

`be/Cashregister.Database/Extensions/ServiceCollectionExtensions.cs:85-86` registers `OrderEntityMapper` and `ArticleEntityMapper` as singletons. All other database services (queries, commands, interceptor) are scoped. Mappers are stateless so singleton is correct, but the mixed lifetime is surprising in a layer where everything else is scoped.

**Fix:** Document the rationale in a comment, or make them scoped for uniformity (the perf difference is negligible).
**Status:** TODO

### 4.5 Frontend `ArticleRow` uses emoji buttons; rest of UI uses text buttons (LOW)

Edit (`✎`) and delete (`✕`) in `article-row.tsx:28,36` use Unicode symbols, while all other buttons use text ("New Article", "Load More", "Submit Order"). This is a visual inconsistency and an accessibility concern (screen readers will read the raw Unicode code point name).

**Fix:** Use text labels (e.g. "Edit", "Delete") only
**Status:** TODO

### 4.6 Domain model immutability: `class` vs `record` (LOW)

`Article`, `Order`, `PendingOrder` are `sealed class` with `init`-only properties. `Identifier`, `Cents`, `TimeStamp`, `OrderNumber` are `sealed record`. The choice is deliberate (value semantics for value objects, reference semantics for entities), but `PendingOrder` is conceptually a value/event - it's created once, never mutated, and has no identity lifecycle beyond the transaction.

**Fix:** Consider making `PendingOrder` a record for consistency with how it's used.
**Status:** TODO

### 4.7 Two different price formatting strategies on the frontend (MEDIUM)

`ui/app/money.ts:formatPrice` uses `toFixed(2)` (locale-unaware, always uses `.` as separator). `ui/app/routes/articles/components/article-row.tsx:16` uses `toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })` (locale-aware, uses the browser's locale separator). The same price will display as `3.50` in one place and `3,50` in another depending on locale. For a Swiss town fair, the locale separator matters.

**Fix:** Use `formatPrice` everywhere, or switch `formatPrice` to use `toLocaleString` and use it consistently. One function, one behavior.
**Status:** DONE
**Decision:** Centralised all price formatting to `formatPrice()` in `money.ts`. All inline `toLocaleString` calls in components were replaced with `formatPrice`. `formatPrice` uses `toFixed(2)` for locale-independent output.

### 4.8 `IFetchArticleQuery` vs `IFetchArticlesQuery` naming (LOW)

`Cashregister.Application.Articles.Data.IFetchArticleQuery` (singular, fetches one article by ID) and `Cashregister.Application.Orders.Data.IFetchArticlesQuery` (plural, fetches multiple articles by IDs for order placement) live in different namespaces but have near-identical names. The plural one is a port owned by the Orders module but operates on articles.

**Fix:** Rename the plural one to something that reflects its order-specific purpose, e.g. `IFetchOrderArticlesQuery` or `IResolveArticlesForOrderQuery`.
**Status:** TODO

### 4.9 Multiple types per file in application layer input models (LOW)

`be/Cashregister.Application/Orders/Models/Input/OrderRequest.cs` defines both `OrderRequest` and `OrderRequestItem` in the same file. This is the same pattern flagged in 4.2 for API DTOs. The "one type per file" convention should extend to the application layer as well.

**Fix:** Move `OrderRequestItem` to its own file.
**Status:** TODO

## 5. Idiomacy / Modern Syntax

### 5.1 `ChangeArticle` uses POST instead of PUT/PATCH (LOW)

`be/Cashregister.Api/Articles/Endpoints.cs:19` maps `ChangeArticle` to `MapPost("/{id}", ...)`. The HTTP convention for full replacement is `PUT`; for partial update, `PATCH`. Using `POST` for updates is non-standard.

**Fix:** Change to `MapPut("/{id}", ...)`.
**Status:** WONT
**Decision**: Adhesion to the RESTful is out of scope. We will use only the HTTP methods GET, UPDATE and DELETE. This is to keep the API simple and not wade into RESTful philosophic discussions.

### 5.2 API returns no error body (MEDIUM)

All `BadRequest()`, `NotFound()` calls return empty bodies. ASP.NET Core's `ProblemDetails` is the idiomatic way to communicate error details to clients. The `Problem` hierarchy exists in the domain but never reaches the wire.

**Fix:** Use a custom middleware that maps `Problem` types to RFC 9457 ProblemDetails. Leverage ASP .NET Core built-ins as much as possible
**Status:** TODO

### 5.3 No CORS configuration (LOW)

The SPA and API may run on different origins during development (`localhost:5173` vs `localhost:5122`). The Vite proxy handles this, but if the app is ever served from a different origin there's no CORS policy. For a portfolio project this is worth demonstrating.

**Fix:** Add `builder.Services.AddCors()` with a sensible policy.
**Status:** WONT
**Decision**: The application will be deployed locally and will use a local reverse-proxy.

### 5.4 Frontend `ApiClient` error message is just the URL (LOW)

`ui/app/api-client.ts:52` sets `message: url` on HTTP errors. This makes toast messages show raw URLs to the user (e.g. "http://localhost:5122/api/articles/123") instead of a human-readable message.

**Fix:** Include the status text or a generic message, e.g. `message: \`${response.status} ${response.statusText}\``. This can be done when 5.2 is complete
**Status:** TODO

### 5.5 `Result.Void()` returns `default(Unit)` as value, which is `null`-ish (LOW)

`be/Cashregister.Commons/Result.cs:47-49` creates `new Result<Unit>(value: default)`. Since `Unit` is a struct, `default` produces `default(Unit)` which is valid. But the `Result<T>` stores `Value` as `TValue?` (nullable), and `MemberNotNullWhen(true, "Value")` on `Ok` asserts it's non-null. With `default(Unit)`, `Value` is non-null because `Unit` is a value type - this works but relies on value-type semantics. If `Unit` were ever changed to a class, this would break silently.

**Fix:** Add a comment explaining the reliance on value-type behavior.
**Status:** TODO

### 5.6 No input validation on API DTOs (MEDIUM)

The backend has no validation attributes or `FluentValidation` on request DTOs (`RegisterArticleRequestDto`, `ChangeArticleRequestDto`, `OrderRequestDto`). Invalid input (empty description, zero quantity, negative price) is caught downstream or not at all.

**Fix:** Add `[Required]`, `[Range]`, `[MinLength]` attributes on DTOs.
**Status:** TODO

### 5.7 `closedby`, `command`, and `commandfor` are experimental HTML attributes (LOW)

`ui/app/components/modal.tsx:31` uses `closedby="none"` and `ui/app/routes/articles/components/article-form.tsx:75-76` uses `command="close"` and `commandfor={modalId}`. These are part of the Invoker Commands API, currently only supported in Chrome 134+. The `env.d.ts` augments React's type definitions to allow them. Firefox and Safari do not support these yet, and the attributes will be silently ignored there - meaning the modal close button in the article form won't work in non-Chrome browsers.

**Fix:** Add a fallback `onClick` handler to the Cancel button that calls `dialog.close()` or the modal's `onClose` callback, so the button works regardless of Invoker Commands support.
**Status:** TODO

### 5.8 `Spinner` has no accessible label (LOW)

`ui/app/components/spinner.tsx` renders a spinning `div` with no `role="status"`, no `aria-label`, and no visually hidden text. Screen readers cannot communicate that content is loading.

**Fix:** Add `role="status"` and an `aria-label="Loading"` to the outer `div`, or include a visually hidden `<span>` with "Loading..." text.
**Status:** TODO

## 6. Application of Patterns and Practices

### 6.1 Transaction base class swallows all exceptions (MEDIUM)

`be/Cashregister.Commons/Transaction.cs:33` catches `Exception` and wraps it in `UnhandledExceptionProblem`. This includes catastrophic exceptions (`OutOfMemoryException`, `StackOverflowException`). The pragma suppression acknowledges CA1031, but the behavior is still risky.

**Fix:** Re-throw fatal exceptions: `catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)`.
**Status:** TODO

### 6.2 `Scoped<T>` creates independent scopes, breaking the UoW boundary (MEDIUM)

`be/Cashregister.Commons/Scoped.cs` creates a new `IServiceScope` per call. In `PlaceOrderActivity`, each `Scoped<IPlaceOrderTransaction>.ExecuteAsync(...)` gets its own `DbContext` and `IUnitOfWork`. If `PlaceOrderTransaction` succeeds but `PrintReceiptTransaction` fails, the order is already committed - there is no cross-scope rollback. This is by design (each transaction is independent) but means the "activity" is not truly atomic.

**Fix:** Document this explicitly in `PlaceOrderActivity`. If atomicity is needed, use a single scope wrapping all steps.
**Status:** WONT
**Decision:** This is an in-process saga simulation. There are no compensating actions in case of failure by design. The cashier can retry printing manually.

### 6.3 `IUnitOfWork.StartAsync` and `RollbackAsync` are no-ops (LOW)

`be/Cashregister.Database/ApplicationDbContext.cs:18-31` implements both as `Task.CompletedTask`. The `Transaction` base class faithfully calls them. This is the Null Object pattern applied to EF Core's implicit UoW, which is fine, but it means the interface over-specifies the contract for this implementation.

**Fix:** This is acceptable as-is given the "enterprise playground" goal. Add a brief XML doc on the interface explaining that implementations may no-op these methods.
**Status:** TODO

### 6.4 `FetchUntilAsync` has no upper bound (MEDIUM)

`be/Cashregister.Database/Queries/PaginationQuery.cs:31-44` fetches all items with `Id <= until` with no `Take` limit. Combined with `Paginator.FetchUntilPageAsync`, this loads the entire history into memory when the `until` cursor is at the end of the dataset. For a town fair this is fine, but for a portfolio project it contradicts the enterprise-grade aspiration.

**Fix:** Add a hard cap (e.g. 10,000 items) or paginate the historical portion as well. At minimum, document the limitation.
**Status:** TODO

### 6.5 Pagination is exclusive on `after` but documentation says inclusive (LOW)

The actual query `PaginationQuery.cs:22` uses `CompareTo(afterValue) > 0` (exclusive - items strictly after the cursor). The `AGENTS.md` file does not contain pagination documentation. The `WHERE id >= after` (inclusive) claim is in `MEMORY.md`, not AGENTS.md.

**Fix:** Correct the MEMORY.md entry to say `WHERE id > after`.
**Status:** DONE
**Decision:** AGENTS.md does not contain this claim. Updated MEMORY.md reference instead.

## 7. Cleanliness

### 7.1 `Article` doesn't carry `Retired` state (LOW)

`be/Cashregister.Domain/Article.cs` has no `Retired` property. `Article.Retire()` returns a `RetiredArticle` (which only carries an `Id`). The `SaveArticleCommand` presumably handles the transition, but the domain model discards all information except the ID when retiring. This means the retire operation cannot be logged or audited with the article's description/price at retirement time.

**Fix:** Consider having `RetiredArticle` carry the full article data, or log the details before retirement.
**Status:** WONT
**Decision:** This is not needed for this type of application. We will not feature an audit log.

### 7.2 Delete button is present but disabled with no explanation (LOW)

`ui/app/routes/articles/components/article-row.tsx:30-37` renders a permanently disabled delete button. There's no tooltip or UI indication of why it's disabled or when it will be enabled.

**Fix:** Either hide the button until delete is implemented, or add a `title="Coming soon"` tooltip.
**Status:** WONT
**Decision:** We will implement this feature later

### 7.3 The `Cashregister.Commons` namespace is `Cashregister.Factories` (MEDIUM)

The project is named `Cashregister.Commons` but the root namespace is `Cashregister.Factories` (e.g. `Result.cs`, `Transaction.cs`, `Scoped.cs` all use `namespace Cashregister.Factories`). This is confusing - "Factories" implies a creational pattern, but the namespace contains `Result`, `Transaction`, `IUnitOfWork`, etc.

**Fix:** Remove the root namespace declaration in the .csproj file
**Status:** TODO

### 7.4 `ArticleDto.Price` is `decimal` but `ArticleListItemDto.price` (frontend) is `number` (MEDIUM)

The backend sends prices as `decimal` via JSON (e.g. `3.50`). The frontend `model.ts:10` types this as `number`. JavaScript `number` is IEEE 754 double, which cannot represent all decimals exactly. For a cash register, `3.50` is fine, but `0.1 + 0.2 !== 0.3` in JS. The `money.ts` utility uses `Math.round` to paper over this.

**Fix:** I did a very dumb thing with this. We should always move cents back and forth and let the frontend handle presentation. Backend should still apply payble money coercion to the amounts coming in and the frontend should just present them in decimal notation
**Status:** DONE
**Decision:** All API DTOs updated to use `long` cents instead of `decimal`. Frontend updated to use these cent values and format them for display only.

### 7.5 `ChangeArticleTransaction` fetches article only to check existence (LOW)

`be/Cashregister.Application/Articles/Transactions/Defaults/ChangeArticleTransaction.cs:22-27` fetches the full article via `fetchArticleQuery.FetchAsync(change.Id)`, uses it only to check for null, then discards it and creates a brand new `Article` from the change input. The fetched data is never read. This is an unnecessary database round-trip when a lightweight existence check would suffice.

**Fix:** Introduce a `bool ExistsAsync(Identifier id)` method on the query interface, or accept the current behavior as a deliberate pattern (fetch-before-write ensures the entity is tracked for EF Core's change tracker, which `SaveArticleCommand` relies on via `FindAsync`). If keeping it, rename the variable to `existingArticle` to signal intent.
**Status:** TODO

### 7.6 `Fragment` import in order route (LOW)

`ui/app/routes/order/order.tsx:1` imports `Fragment` from React. It is used on line 129 as `<Fragment key={entry.article.id}>` to wrap hidden inputs. The explicit `Fragment` import is necessary because the JSX shorthand `<>` does not support the `key` prop. However, the `Fragment` wrapper exists only to group two `<input type="hidden">` elements per cart entry - the same effect can be achieved by wrapping them in a plain `<div>` with `hidden` or by restructuring the form data serialization.

**Fix:** Replace `Fragment` keyed usage with a wrapper `<div>` or restructure the hidden inputs to avoid needing a keyed wrapper.
**Status:** TODO

## Summary

### Priority TODO items

1. **3.1/3.2/3.3/4.1 Blanket `BadRequest` in API handlers** (MEDIUM) - masks 500s as 400s, apply `switch` pattern everywhere
2. **5.2 API returns no error body** (MEDIUM) - wire `Problem` types to RFC 9457 ProblemDetails
3. **5.6 No input validation on API DTOs** (MEDIUM) - empty descriptions and negative prices pass through
4. **3.10 EF Core relationships configured from both sides** (MEDIUM) - redundant and fragile
5. **4.7 Two different price formatting strategies** (MEDIUM) - `toFixed` vs `toLocaleString` inconsistency
6. **5.7 Experimental `command`/`commandfor` attributes** (LOW) - modal Cancel button broken in non-Chrome browsers

### Architecture evaluation

The codebase demonstrates strong architectural discipline. The layered hexagonal design enforces clean dependency flow: Domain has no dependencies, Application defines ports, Database and API implement them. The Transaction base class with automatic UoW management, the Result monad for error propagation, and the cursor-based pagination with lookahead are all well-executed patterns appropriate for a portfolio project.

The frontend mirrors this quality with a clean composition root, consistent Result pattern, proper route/component ownership, and thorough test coverage. The main structural weakness is the price data contract - the backend converts cents to decimal before sending them over the wire, then the frontend has to convert back, introducing rounding ambiguity. Fixing this (item 7.4) will cascade into resolving 1.3 and simplifying the frontend money utilities.

The second cluster of issues is around error handling: the `Problem` type hierarchy exists in the domain but never surfaces to the API consumer. Wiring ProblemDetails (5.2) and adopting the error switch pattern consistently (4.1) would close this gap and make the backend genuinely enterprise-grade. AGENTS.md had several stale entries that have been corrected: API client path (2.2), project structure tree (2.5), frontend directory tree (2.6), and import path example (2.7). Items 2.1 and 2.3 were false positives — the issues were in the review prompt and MEMORY.md respectively, not in AGENTS.md.
