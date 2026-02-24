# Backend Code Review

Comprehensive review of the `be/` backend codebase covering architecture, .NET/C# idioms, refactoring opportunities, code smells, and long-term maintainability. Evaluated in the context of a local-network, single-user cash register for a town fair, while acknowledging the deliberate enterprise-style exercise (DDD, Saga pattern, `Scoped<T>`, etc.).

---

## Table of Contents

1. [Overall Architecture Assessment](#1-overall-architecture-assessment)
2. [Strengths](#2-strengths)
3. [Bugs and Correctness Issues](#3-bugs-and-correctness-issues)
4. [Code Smells and Design Issues](#4-code-smells-and-design-issues)
5. [.NET / C# Idiom Compliance](#5-net--c-idiom-compliance)
6. [Refactoring Opportunities](#6-refactoring-opportunities)
7. [Testing Assessment](#7-testing-assessment)
8. [Maintainability and Housekeeping](#8-maintainability-and-housekeeping)
9. [Summary Matrix](#9-summary-matrix)

---

## 1. Overall Architecture Assessment

The layered Clean Architecture is sound and well-executed for an enterprise-style exercise:

```
Domain  →  Application  →  Database  →  API
                ↑
           Commons (Result, Transaction, Scoped, UoW)
                ↑
           Activities (Saga orchestration)
```

Layer boundaries are respected: the Domain project has zero framework dependencies (only the `Ulid` package), Application depends only on Domain + Commons, and Database depends on Application + EF Core. The API wires everything together. This is textbook onion/clean architecture and done correctly.

The `Transaction<TInput, TOutput>` base class provides a consistent Unit of Work envelope, and the `Result<T>` pattern avoids exceptions for business-level flow control. The `Scoped<T>` utility enables the Saga-style `PlaceOrderActivity` to orchestrate multiple transactions across independent DI scopes — a well-motivated pattern for in-process multi-step workflows.

---

## 2. Strengths

### 2.1 Value Objects are well-designed
`Identifier`, `Cents`, `TimeStamp`, and `OrderNumber` are clean, immutable value objects. Using `record` for those that need structural equality is correct. The `Cents.AsPayableMoney()` rounding to CHF 0.05 is a domain-specific rule properly encapsulated in the value object.

### 2.2 Consistent use of `required` and `init`
Domain entities and DTOs consistently use `required init` properties, enforcing complete object initialization at construction time. This is modern C# at its best.

### 2.3 Clean separation of read and write concerns
The Query/Command split (`IFetchArticleQuery`, `ISaveArticleCommand`, etc.) at the Application layer is clean. Each interface has a narrow responsibility.

### 2.4 Cursor-based pagination
The ULID-based cursor pagination is a good choice. ULIDs sort lexicographically and temporally, making `CompareTo` comparisons in SQL reliable. The "fetch N+1" strategy for detecting `HasNext` is efficient and avoids a separate COUNT query.

### 2.5 SQLite pragmas interceptor
The `SqlitePragmasDbConnectionInterceptor` sets WAL mode, appropriate cache sizes, and foreign key enforcement — all important for SQLite correctness and performance. Using a `DbConnectionInterceptor` is the right EF Core extension point.

### 2.6 Soft-delete via global query filter
The `ArticleEntity.Retired` flag with a global query filter (`HasQueryFilter(x => x.Retired == false)`) is idiomatic EF Core. Soft-delete makes sense here because historical orders reference articles by ID.

### 2.7 Integration test infrastructure
The `IntegrationTest` base class with per-test SQLite databases, `WebApplicationFactory`, and helper methods (`RunScoped`, `CreateHttpClient`) is well-structured. Test isolation is strong.

### 2.8 Build strictness
`Directory.Build.props` enables `TreatWarningsAsErrors`, `Nullable`, and `AnalysisMode=All`. This is good hygiene.

---

## 3. Bugs and Correctness Issues

### 3.1 [BUG] `ChangeArticle` handler silently returns `NoContent` on unknown errors

**File:** `Cashregister.Api/Articles/Handlers.cs:113-137`

```csharp
public static async Task<Results<NotFound, NoContent>> ChangeArticle(...)
{
    var result = await changeArticleTransaction.ExecuteAsync(articleChange);

    if (result.NotOk)
    {
        if (result.Error is NoSuchArticleProblem)
        {
            return TypedResults.NotFound();
        }
    }

    return TypedResults.NoContent();  // ← reached even when result.NotOk with a non-NotFound error
}
```

When `result.NotOk` is `true` and the error is *not* `NoSuchArticleProblem` (e.g., `UnhandledExceptionProblem`), the handler falls through and returns `204 NoContent` — a success status. The same issue exists in `DeleteArticle` at line 139. Both handlers should return a `500` or at minimum a `BadRequest` for unrecognised problems.

**Suggested fix:** add an `else` branch or a catch-all after the pattern match:

```csharp
if (result.NotOk)
{
    return result.Error switch
    {
        NoSuchArticleProblem => TypedResults.NotFound(),
        _ => TypedResults.BadRequest()  // or InternalServerError
    };
}

return TypedResults.NoContent();
```

### 3.2 [BUG] `CreateOrder` returns the request DTO instead of an `EntityPointerDto`

**File:** `Cashregister.Api/Orders/Handlers.cs:48`

```csharp
return TypedResults.Created(getOrderUrl, orderRequestDto);  // ← returns the input, not a pointer
```

The `RegisterArticle` handler correctly returns an `EntityPointerDto` with `Id` and `Location`. The `CreateOrder` handler instead echoes back the incoming `OrderRequestDto`. This is inconsistent and means the client receives no order ID in the response body. It should return an `EntityPointerDto` (or a similar DTO containing the new order's `Id`), consistent with the articles endpoint.

### 3.3 [BUG] `SqlitePragmasDbConnectionInterceptor` does not dispose the `DbCommand`

**File:** `Cashregister.Database/Interceptors/SqlitePragmasDbConnectionInterceptor.cs:24`

```csharp
DbCommand? pragmaCommand = connection.CreateCommand();
// ... command is used but never disposed
```

`DbCommand` implements `IDisposable`. The command should be wrapped in `await using`:

```csharp
await using var pragmaCommand = connection.CreateCommand();
```

### 3.4 [ISSUE] `FetchArticlesListQuery` inclusive cursor semantics may duplicate items

**File:** `Cashregister.Database/Queries/FetchArticlesListQuery.cs:21`

```csharp
.Where(a => afterValue == null || a.Id.CompareTo(afterValue) >= 0)
```

The `>= 0` (inclusive) means the cursor item itself is included in the results. In `FetchArticlesPageTransaction`, the `Next` cursor is set to the last element of the over-fetched list (`articleListItemPlusOne[^1]`), which then becomes the first element of the *next* page — duplicating it across pages.

The test `FetchAsync_WithAfter_ShouldReturnNextPage` in `FetchArticlesPageTransactionTests.cs` actually passes because the `Next` cursor is set to the *extra* (N+1th) element, but this behaviour is fragile and counter to standard cursor-pagination convention where `after` means "strictly after." This cursor is inclusive, which is unusual and could surprise future maintainers. The naming `After` suggests exclusive semantics. Consider renaming to `From` or switching to strictly `> 0`.

### 3.5 [ISSUE] `Result.Void()` initialises `Value` with `default` (null for reference check)

**File:** `Cashregister.Commons/Result.cs:47-49`

```csharp
public static Result<Unit> Void()
{
    return new Result<Unit>(value: default);
}
```

`Unit` is a `readonly struct`, so `default` is a valid zero-initialised instance. However, `Result<TValue>` stores `Value` as `TValue?`. For value types this means `Nullable<Unit>`, so `Value` will be `null` (since the `Result(TValue value)` constructor sets `Value = value` which for `default(Unit)` gets boxed to `null` on nullable value types). The `[MemberNotNullWhen(true, nameof(Value))]` attribute on `Ok` then claims `Value` is not null when `Ok == true`, but it *is* null. This is technically a nullability contract violation, though it has no practical effect since nobody reads `Value` from a `Result<Unit>`. Still, it would be cleaner to pass `new Unit()` or define a `Unit.Value` constant.

---

## 4. Code Smells and Design Issues

### 4.1 Namespace mismatch: `Cashregister.Commons` project uses `Cashregister.Factories`

**File:** `Cashregister.Commons/Cashregister.Commons.csproj:3`

```xml
<RootNamespace>Cashregister.Factories</RootNamespace>
```

The project is named `Cashregister.Commons` but its root namespace is `Cashregister.Factories`. This is confusing. `Result<T>`, `Transaction<T>`, `IUnitOfWork`, `Scoped<T>`, and `Unit` are not "factories" — they're foundational abstractions. The namespace should match the project name or at least convey the correct semantics (e.g., `Cashregister.Commons` or `Cashregister.Foundation`).

### 4.2 `Transaction` does not propagate `CancellationToken` to `InternalExecuteAsync`

**File:** `Cashregister.Commons/Transaction.cs:18`

```csharp
var result = await InternalExecuteAsync(input);
```

The `ExecuteAsync` method accepts a `CancellationToken`, passes it to `StartAsync` and `SaveChangesAsync`/`RollbackAsync`, but *does not* pass it to `InternalExecuteAsync`. This means the actual business logic inside transactions cannot observe cancellation. The abstract method signature should include `CancellationToken`:

```csharp
protected abstract Task<Result<TOutput>> InternalExecuteAsync(TInput input, CancellationToken cancellationToken);
```

### 4.3 `FetchArticlesPageTransaction` does not extend `Transaction<,>` and is not a transaction

**File:** `Cashregister.Application/Articles/Transactions/Defaults/FetchArticlesPageTransaction.cs`

This class implements `IFetchArticlesPageTransaction` directly (without inheriting from `Transaction<,>`). It's a read-only query operation — not a transaction. Placing it in the `Transactions` folder and naming it a "Transaction" is misleading. Consider:
- Moving it to a `Queries` or `UseCases` folder in the Application layer
- Renaming the interface to `IFetchArticlesPageQuery` or `IFetchArticlesPageUseCase`

The naming inconsistency is compounded by `IFetchArticlesPageTransaction.ExecuteAsync` not accepting a `CancellationToken`, unlike all the real Transaction-based interfaces.

### 4.4 `PrintReceiptTransaction` starts a UoW for a no-op

**File:** `Cashregister.Application/Receipts/Transactions/Defaults/PrintReceiptTransaction.cs`

This class extends `Transaction<Identifier, Unit>` (so it starts/commits a Unit of Work) but currently does nothing. Even when the receipt printing is implemented, it's unlikely to need a database transaction since printing is a side-effect, not a data mutation. Consider whether this should be a Transaction at all, or just a plain service.

### 4.5 Duplicate relationship configuration for `OrderItemEntity`

**Files:**
- `Cashregister.Database/Entities/OrderEntity.cs:30-32` — configures `OrderEntity → OrderItemEntity` (one-to-many)
- `Cashregister.Database/Entities/OrderItemEntity.cs:40-42` — configures `OrderItemEntity → OrderEntity` (many-to-one)

Both sides of the same relationship are configured independently. EF Core will detect them as the same relationship, but it's redundant and risks divergence. Pick one authoritative side (conventionally the principal/owner, `OrderEntity`) and remove the duplicate from `OrderItemEntity`.

Similarly, `ArticleEntity` configures `HasMany<OrderItemEntity>` while `OrderItemEntity` configures `HasOne<ArticleEntity>`. Same issue.

### 4.6 Inconsistent visibility of API DTOs

- `ArticleDto` is `internal sealed record` (`Cashregister.Api/Articles/Models/ArticleDto.cs:5`)
- `ArticlesPageDto`, `ArticleListItemDto` are `public sealed record` (`ArticlePageDto.cs:7,13`)
- `EntityPointerDto` is `public sealed class` (`EntityPointerDto.cs:3`)

Since the test project references the API project, the `public` visibility is needed for deserialization in tests. But `ArticleDto` is `internal`, meaning tests can't deserialize single-article responses. This should be made consistent — either all `public` (for testability) or use `InternalsVisibleTo`.

### 4.7 The `Cashregister.Activities` project is referenced but not wired up

**File:** `Cashregister.Api/Cashregister.Api.csproj` references `Cashregister.Activities`, but `Program.cs` never registers `PlaceOrderActivity` or `Scoped<T>` in DI, and the Orders handler calls `IPlaceOrderTransaction` directly rather than going through `PlaceOrderActivity`. The Activity/Saga layer is dead code from the API's perspective.

This also means `AddCashregisterReceipts()` is never called in `Program.cs`, even though the Application layer defines `IPrintReceiptTransaction`. The Saga can't work at runtime.

### 4.8 `IFetchOrderPrintDataQuery` is defined but never implemented

**File:** `Cashregister.Application/Receipts/Data/IFetchOrderPrintDataQuery.cs`

No class in `Cashregister.Database` implements this interface. It's not registered in DI. This is orphaned code.

---

## 5. .NET / C# Idiom Compliance

### 5.1 Use `AddAsync` vs `Add` for EF Core

**File:** `Cashregister.Database/Commands/SaveArticleCommand.cs:23`

```csharp
await applicationDbContext.Articles.AddAsync(articleEntity);
```

`DbSet.AddAsync` is only needed when using value generators that require async I/O (e.g., HiLo sequences). For SQLite with client-generated ULIDs, synchronous `Add()` is preferred per [EF Core guidance](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.addasync). Same applies in `SaveOrderCommand.cs:14`.

### 5.2 `Identifier.New()` uses unnecessary nullable local

**File:** `Cashregister.Domain/Identifier.cs:8`

```csharp
string? ulidString = ulid.ToString();
```

`Ulid.ToString()` never returns `null` — `string?` is misleading. Use `string`.

### 5.3 Consider using `IAsyncDisposable` and `GC.SuppressFinalize`

**File:** `Cashregister.Tests.Integration/IntegrationTest.cs:25`

The `Dispose()` method disposes `_webApplicationFactory` (which is `IDisposable`). Since `IntegrationTest` is only `IDisposable` (not `IAsyncDisposable`), there's no issue per se, but `WebApplicationFactory` does support `IAsyncDisposable`. More importantly, the `Dispose()` call should include `GC.SuppressFinalize(this)` per CA1816, since warnings are errors.

### 5.4 `Scoped<T>` nullable annotations on non-nullable locals

**File:** `Cashregister.Commons/Scoped.cs:11-13`

```csharp
using IServiceScope? scope = serviceProvider.CreateScope();
TService? service = scope.ServiceProvider.GetRequiredService<TService>();
```

`CreateScope()` never returns null, and `GetRequiredService<T>()` never returns null (it throws). The `?` annotations are unnecessary noise.

### 5.5 `using` statement for `ApplicationDbContext` in tests

**File:** `Cashregister.Tests.Integration/IntegrationTest.cs:66`

```csharp
await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
```

When `ApplicationDbContext` is resolved from a scoped `IServiceScope`, the scope owns the lifetime of the context. Explicitly disposing it with `await using` means it gets disposed *twice* (once by the using statement, once when the scope disposes). This is harmless for EF Core's `DbContext` (it's idempotent) but is incorrect usage. Remove the explicit `await using` and let the scope manage lifetime, or remove the scope and manage the context directly.

### 5.6 `OrderEntity.RowId` uses `.IsRowVersion()` for SQLite rowid mapping

**File:** `Cashregister.Database/Entities/OrderEntity.cs:34-37`

```csharp
builder.Property(x => x.RowId)
    .HasColumnName("_rowid_")
    .IsRowVersion()
    .IsRequired();
```

`IsRowVersion()` is meant for concurrency tokens (e.g., SQL Server `rowversion`/`timestamp`). Using it to map SQLite's `_rowid_` is a semantic mismatch. This works because SQLite's `_rowid_` is auto-assigned and monotonically increasing, but it means EF Core will add a `WHERE RowId = @old_value` clause to any UPDATE statement on `OrderEntity`, which is unnecessary and could cause phantom concurrency conflicts. Consider using `.ValueGeneratedOnAdd()` and `.HasColumnName("rowid")` instead.

---

## 6. Refactoring Opportunities

### 6.1 Unify the error-mapping logic in API handlers

The pattern of checking `result.NotOk` + pattern-matching `result.Error` is repeated across all handlers. Consider a small extension method or static helper:

```csharp
static IResult? ToProblemResult(Problem error) => error switch
{
    NoSuchArticleProblem => TypedResults.NotFound(),
    OrderRequestIsMissingSomeArticles => TypedResults.BadRequest(),
    UnhandledExceptionProblem => TypedResults.StatusCode(500),
    _ => TypedResults.BadRequest()
};
```

This centralises the mapping and avoids the fall-through bug described in 3.1.

### 6.2 Make `Result<T>` support `Map` / `Bind` combinators

Given the enterprise exercise context, `Result<T>` would benefit from monadic combinators:

```csharp
public Result<TOut> Map<TOut>(Func<TValue, TOut> mapper) =>
    Ok ? Result.Ok(mapper(Value)) : Result.Error<TOut>(Error);

public async Task<Result<TOut>> BindAsync<TOut>(Func<TValue, Task<Result<TOut>>> binder) =>
    Ok ? await binder(Value) : Result.Error<TOut>(Error);
```

`PlaceOrderActivity` would become more readable with `BindAsync` chains instead of the repetitive `if (result.NotOk) return Error` blocks.

### 6.3 Consolidate `ArticleChange` and `ArticleDefinition`

**Files:** `Application/Articles/Models/Input/ArticleChange.cs`, `ArticleDefinition.cs`

`ArticleChange` has `Id + Description + Price`. `ArticleDefinition` has `Description + Price`. They share two of three properties. You could use inheritance or just accept the duplication given the small size, but if more fields are added to articles later, consider a shared base.

### 6.4 Extract cursor pagination into a reusable pattern

`FetchArticlesPageTransaction` contains generic pagination logic (fetch N+1, detect `HasNext`, slice). If more entities need pagination (e.g., orders), consider extracting a `CursorPage<T>` helper.

### 6.5 Consider replacing `IApplicationDbContext` with direct constructor injection

`IApplicationDbContext` exposes raw `DbSet<T>` properties, which means queries and commands directly depend on EF Core types anyway. The interface adds a layer of indirection without meaningful abstraction — you can't substitute a non-EF implementation since the consumers use `DbSet<T>` LINQ. For this codebase size, injecting `ApplicationDbContext` directly would be simpler. However, the interface does have test-double value and fits the enterprise exercise, so this is a minor point.

### 6.6 Wire up the Activities layer or remove the dead code

As noted in 4.7, `PlaceOrderActivity` is unreachable from the API. Either:
- Wire it up: register `Scoped<IPlaceOrderTransaction>`, `Scoped<IPrintReceiptTransaction>`, `Scoped<IFetchOrderSummaryQuery>`, `PlaceOrderActivity`, and call `AddCashregisterReceipts()` in `Program.cs`, then route the Orders handler through the Activity.
- Or, if the Saga pattern is aspirational/not yet needed, remove `Cashregister.Activities` from the API project reference to keep the dependency graph honest.

---

## 7. Testing Assessment

### 7.1 Good coverage breadth
The integration tests cover:
- CRUD lifecycle for articles (create, read, paginate, soft-delete)
- Order creation and retrieval
- Edge cases: empty data, pagination boundaries, invalid requests
- Entity-level read/write smoke tests

### 7.2 Missing coverage
- **`ChangeArticle` endpoint** — no HTTP-level test for `POST /articles/{id}` (update).
- **`PlaceOrderActivity`** — the Saga orchestration has zero tests.
- **Error paths** — no tests verify that `UnhandledExceptionProblem` is properly surfaced (or that it isn't accidentally returned as 204, per bug 3.1).
- **`ArticleDto` visibility** — since `ArticleDto` is `internal`, the test project can't deserialize `GET /articles/{id}` responses, which may explain why there's no test for the single-article endpoint response body.
- **Concurrent writes** — not relevant for a single-user app, noted for completeness.

### 7.3 Stub test
`ListArticlesByPageQueryTests.cs` contains a single empty `[Fact]` with a `// TODO: Implement test` comment. This should either be implemented or removed to avoid green test counts obscuring real coverage.

### 7.4 `RunScoped` lifetime risk

**File:** `IntegrationTest.cs:81-88`

```csharp
protected Task<TResult> RunScoped<TService, TResult>(Func<TService, Task<TResult>> action)
    where TService : notnull
{
    using var serviceScope = NewServiceScope();
    var service = serviceScope.ServiceProvider.GetRequiredService<TService>();
    return action(service);
}
```

The `using var serviceScope` disposes the scope when `RunScoped` returns. But `action(service)` returns a `Task<TResult>` that may not have completed yet — the scope (and its scoped services, including `DbContext`) gets disposed *before* the task completes. This is a race condition. The fix is to `await` the action:

```csharp
protected async Task<TResult> RunScoped<TService, TResult>(Func<TService, Task<TResult>> action)
    where TService : notnull
{
    using var serviceScope = NewServiceScope();
    var service = serviceScope.ServiceProvider.GetRequiredService<TService>();
    return await action(service);
}
```

This is likely the most impactful bug in the test infrastructure — it may pass today because SQLite is fast enough that the task completes synchronously in practice, but it's fundamentally unsafe.

---

## 8. Maintainability and Housekeeping

### 8.1 `EnableDetailedErrors` and `EnableSensitiveDataLogging` in production

**File:** `Cashregister.Database/Extensions/ServiceCollectionExtensions.cs:57-60`

```csharp
opts.UseSqlite(connectionString)
    .AddInterceptors(...)
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
```

`EnableSensitiveDataLogging()` includes parameter values in EF Core logs and exception messages. While acceptable for a local-network app, it's unconditionally enabled. If the app ever moves beyond the fair kiosk, this should be environment-gated (e.g., `if (env.IsDevelopment())`).

### 8.2 Error message string in `ServiceCollectionExtensions`

**File:** `Cashregister.Database/Extensions/ServiceCollectionExtensions.cs:51-52`

```csharp
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("The database connection string 'CashRegister' is missing.");
}
```

This check will never trigger — `SqliteConnectionStringBuilder.ToString()` always returns a non-empty string if `DataSource` was set. And the error message references `'CashRegister'`, which is not the actual configuration key (the real key is `'DataSource'`). Either remove this dead check or fix the message.

### 8.3 Logging verbosity of PRAGMA commands

**File:** `Cashregister.Database/Extensions/LoggerMessages.cs`

The full PRAGMA command text is logged at `LogLevel.Information` every time a connection is opened. Since SQLite with connection pooling will open/reuse connections frequently, this pollutes logs. Consider `LogLevel.Debug`.

### 8.4 `using Cashregister.Application;` in `ApplicationDbContext.cs`

**File:** `Cashregister.Database/ApplicationDbContext.cs:1`

This `using` directive is unused — `ApplicationDbContext` doesn't reference anything from `Cashregister.Application` namespace directly. It may be a leftover.

### 8.5 `Order` class is not `sealed`

**File:** `Cashregister.Domain/Order.cs:6`

```csharp
public class Order  // not sealed
```

Every other domain class (`Article`, `Item`, `PendingOrder`, `RetiredArticle`) is `sealed`. `Order` should be too for consistency and to signal that it's not designed for inheritance.

### 8.6 `ArticleDto.From` uses `ToString()` for `Id`

**File:** `Cashregister.Api/Articles/Models/ArticleDto.cs:13`

```csharp
article.Id.ToString()
```

`Identifier` is a `record(string Value)`, so `ToString()` returns `Identifier { Value = ... }` (the record's default `ToString`), not the raw ULID string. Every other DTO uses `.Value` directly. This should be `article.Id.Value`.

---

## 9. Summary Matrix

| # | Category | Severity | Item |
|---|----------|----------|------|
| 3.1 | Bug | **High** | `ChangeArticle`/`DeleteArticle` return 204 on unrecognised errors |
| 3.2 | Bug | **High** | `CreateOrder` returns request DTO instead of entity pointer |
| 3.3 | Bug | Medium | `DbCommand` not disposed in pragma interceptor |
| 3.6 | Bug | **High** | `ArticleDto.From` uses `ToString()` instead of `.Value` for Id (8.6) |
| 7.4 | Bug | **High** | `RunScoped` disposes scope before async task completes |
| 3.4 | Design | Medium | Inclusive cursor semantics may surprise maintainers |
| 3.5 | Design | Low | `Result.Void()` nullability contract mismatch |
| 4.1 | Smell | Medium | `Cashregister.Factories` namespace mismatch |
| 4.2 | Design | Medium | `CancellationToken` not forwarded to `InternalExecuteAsync` |
| 4.3 | Smell | Low | Read-only query named "Transaction" |
| 4.4 | Smell | Low | `PrintReceiptTransaction` starts UoW for a no-op |
| 4.5 | Smell | Low | Duplicate EF relationship configuration |
| 4.6 | Smell | Low | Inconsistent DTO visibility (`internal` vs `public`) |
| 4.7 | Smell | Medium | `PlaceOrderActivity` / Receipts layer not wired into DI |
| 4.8 | Smell | Low | `IFetchOrderPrintDataQuery` orphaned interface |
| 5.1 | Idiom | Low | `AddAsync` vs `Add` |
| 5.2 | Idiom | Low | Unnecessary `string?` on `Ulid.ToString()` |
| 5.4 | Idiom | Low | Unnecessary nullable on `CreateScope()`/`GetRequiredService()` |
| 5.6 | Idiom | Medium | `IsRowVersion()` misuse for SQLite rowid |
| 6.1 | Refactor | Medium | Centralise error-to-HTTP-status mapping |
| 6.6 | Refactor | Medium | Wire up or remove Activities layer |
| 7.3 | Testing | Low | Stub test `ListArticlesByPageQueryTests` |
| 8.1 | Maint. | Low | `EnableSensitiveDataLogging` unconditional |
| 8.2 | Maint. | Low | Dead check with wrong error message |
| 8.3 | Maint. | Low | PRAGMA log at Info instead of Debug |
| 8.5 | Maint. | Low | `Order` not sealed |
