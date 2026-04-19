# Build Receipt PrintProgram Template Service

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document must be maintained according to `docs/PLANS.md`.

## Purpose / Big Picture

After this change, the backend Application layer can build a receipt `PrintProgram` for an existing order without depending on Domain aggregate models. This is the template that later order-submission printing will use. The receipt shows the order number, item quantities and descriptions, order id, and date. It deliberately excludes prices and totals.

The behavior is observable by creating an order in integration tests, resolving the receipt service, building a `PrintProgram`, encoding and emulating it, then rendering the emulated receipt to Markdown and asserting the expected receipt text exists.

## Progress

- [x] (2026-04-16) Create this ExecPlan at `docs/plans/receipt-print-program.md`.
- [x] (2026-04-16) Add `Cashregister.Printmon` as a project reference from `be/Cashregister.Application/Cashregister.Application.csproj`.
- [x] (2026-04-16) Replace the current `OrderPrintData` shape with receipt-specific projection data.
- [x] (2026-04-16) Add a receipt-specific `Problem` for missing order print data.
- [x] (2026-04-16) Add a default receipt `PrintProgram` service under `Cashregister.Application.Receipts`.
- [x] (2026-04-16) Add a database query that projects order receipt data without materializing Domain `Order`.
- [x] (2026-04-16) Register the receipt service and query in DI.
- [x] (2026-04-16) Add integration tests for the projection and receipt template.
- [x] (2026-04-16) Run backend verification: `dotnet format`, `dotnet build`, `dotnet test`.
- [x] (2026-04-16) Add a `docs/DIARY.md` entry summarizing the implementation decisions.

## Surprises & Discoveries

- Observation: `dotnet format` could not start because restore failed on a vulnerable transitive `System.Security.Cryptography.Xml` 9.0.0 package.
  Evidence: `dotnet restore` reported NU1903 for `Cashregister.Database` and advisories GHSA-37gx-xxp4-5rgx and GHSA-w3x6-4m5h-cxqf.

- Observation: `System.Security.Cryptography.Xml` 10.0.0 is also vulnerable according to NuGet audit.
  Evidence: `dotnet restore` reported the same NU1903 advisories after pinning 10.0.0, while `dotnet package search System.Security.Cryptography.Xml --exact-match --format json` showed 10.0.6 is available.

## Decision Log

- Decision: Build a pure receipt program service and avoid actual printer I/O in this task.
  Rationale: The user requested the template and service now; actual submission-time printing is later work. Keeping this service pure makes it testable and prevents accidental writes to a printer device.
  Date/Author: 2026-04-16 / Codex

- Decision: The service returns `Result<PrintProgram>` and uses a `Problem` when order print data is missing.
  Rationale: Project convention is explicit `Result<T>` plus `Problem`, not exceptions for expected business/application failures.
  Date/Author: 2026-04-16 / Codex

- Decision: The Application projection may use Domain value objects but not Domain aggregate/entity models.
  Rationale: Existing Application output models already use `Identifier`, `OrderNumber`, `TimeStamp`, and `Cents`; the requested boundary is specifically to avoid passing Domain `Order`/`Item` into receipt construction.
  Date/Author: 2026-04-16 / Codex

- Decision: Use UTC date formatting from `TimeStamp`.
  Rationale: `TimeStamp` stores Unix seconds generated from `DateTimeOffset.UtcNow`; the backend has no user-local timezone concept.
  Date/Author: 2026-04-16 / Codex

- Decision: Add a direct `System.Security.Cryptography.Xml` 10.0.6 package reference to `Cashregister.Database`.
  Rationale: The verification sequence treats NuGet audit warnings as errors. A direct patched package reference fixes the restore failure without suppressing security warnings.
  Date/Author: 2026-04-16 / Codex

## Outcomes & Retrospective

Implemented. The Application layer now exposes a pure `IReceiptPrintProgramService` that returns `Result<PrintProgram>`, uses `NoSuchOrderPrintDataProblem` for missing orders, and builds the receipt template from `OrderPrintData`. The database layer provides a direct EF projection for receipt data. Integration tests verify the projection, missing-order failure, and rendered receipt content through the print emulator. Actual printer I/O remains future orchestration work.

Verification completed from `be/` on 2026-04-16:

    dotnet format
    dotnet build
    dotnet test

## Context and Orientation

`Cashregister.Printmon` contains the printer abstraction. `PrintProgram` is an immutable list of ESC/POS print instructions. `PrintProgramBuilder` is the intended construction API and automatically adds initialization and final feed/cut instructions when `Build()` is called.

`Cashregister.Application.Receipts` already exists but is incomplete. It currently has `OrderPrintData` with only `Id` and `Total`, `IFetchOrderPrintDataQuery`, and a stub `PrintReceiptTransaction`. The current projection is wrong for this feature because the receipt must not expose prices or totals and must include order number, date, and items.

`Cashregister.Commons/Result.cs` defines `Result<TValue>` and `Problem`. Application services should return `Result<T>` for expected failures. Missing order print data is expected application failure, so the receipt program service must return a failed result, not throw.

`Cashregister.Database` owns EF Core queries and maps persistence entities. The new receipt query belongs there because it reads `OrderEntity` and `OrderItemEntity`. It must project directly into Application receipt output models instead of returning Domain `Order`.

`Cashregister.Printmon.Emulator` can emulate encoded printer bytes and render the final receipt to Markdown. Integration tests can use `BinaryEncoder`, `PrinterEmulator`, and `MarkdownRenderer` to verify the receipt content without a physical printer.

## Plan of Work

First, add `Cashregister.Printmon` as a reference to `be/Cashregister.Application/Cashregister.Application.csproj` so Application can expose `PrintProgram`.

Next, reshape `be/Cashregister.Application/Receipts/Models/Output/OrderPrintData.cs`. The model should contain `Identifier Id`, `OrderNumber Number`, `TimeStamp Date`, and an immutable item list. Add a separate `OrderPrintDataItem` type in the same Receipts output models area with `string Description` and `uint Quantity`. Do not include price, total, article id, or item id.

Update `be/Cashregister.Application/Receipts/Data/IFetchOrderPrintDataQuery.cs` so `Fetch` becomes `FetchAsync` and returns `Task<OrderPrintData?>`. Include a `CancellationToken cancellationToken = default` parameter. This keeps the query as a nullable data lookup while the service translates null into an Application problem.

Add `NoSuchOrderPrintDataProblem` under `be/Cashregister.Application/Receipts/Problems/`. It should derive from `Problem` and carry the missing `Identifier`, or an immutable collection if matching nearby problem style makes that clearer. This problem represents "the requested order cannot be converted to receipt print data because it does not exist."

Add `IReceiptPrintProgramService` under `be/Cashregister.Application/Receipts/Services/`. It must expose:

    Task<Result<PrintProgram>> BuildAsync(Identifier orderId, CancellationToken cancellationToken = default);

Add `ReceiptPrintProgramService` under `be/Cashregister.Application/Receipts/Services/Defaults/`. It depends on `IFetchOrderPrintDataQuery`. `BuildAsync` validates `orderId`, fetches print data, returns `Result.Error<PrintProgram>(new NoSuchOrderPrintDataProblem(orderId))` when the result is null, then builds and returns `Result.Ok(printProgram)` using only `PrintProgramBuilder`.

The receipt template should be simple and stable:

    ORDER <order number>

    <quantity>x <description>
    <quantity>x <description>

    Order ID: <order id>
    Date: <yyyy-MM-dd HH:mm:ss UTC>

Use centered bold formatting for the top order number, left alignment for item/footer lines, and no price-related text. Use `DateTimeOffset.FromUnixTimeSeconds(order.Date.Value).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)` or the equivalent invariant formatting.

Add `FetchOrderPrintDataQuery` under `be/Cashregister.Database/Queries/`. It depends on `IApplicationDbContext` and uses EF Core `AsNoTracking()`. It must filter by `OrderEntity.Id`, project `Id`, `RowId`, `Date`, and `Items.Select(i => new OrderPrintDataItem { Description = i.Description, Quantity = i.Quantity })`. It must not use `OrderEntityMapper` and must not construct Domain `Order`.

Register the service in `be/Cashregister.Application/Receipts/Extensions/ServiceCollectionExtensions.cs`:

    serviceCollection.AddScoped<IReceiptPrintProgramService, ReceiptPrintProgramService>();

Keep the existing `IPrintReceiptTransaction` registration unchanged for now.

Register the query in `be/Cashregister.Database/Extensions/ServiceCollectionExtensions.cs`:

    services.AddScoped<IFetchOrderPrintDataQuery, FetchOrderPrintDataQuery>();

Add `using Cashregister.Application.Receipts.Extensions;` to `be/Cashregister.Api/Program.cs` and include `.AddCashregisterReceipts()` in the existing Application service registration chain.

Add tests under `be/Cashregister.Tests.Integration/Receipts/`. Create `FetchOrderPrintDataQueryTests` for missing and existing orders. Create `ReceiptPrintProgramServiceTests` that creates articles and an order, resolves `IReceiptPrintProgramService`, builds the program, encodes it with `BinaryEncoder`, emulates with `PrinterEmulator`, renders with `MarkdownRenderer`, and asserts content.

Finally, add a concise diary entry to `docs/DIARY.md` with the task title "Receipt PrintProgram template service". Mention that receipt construction is pure Application behavior, the projection excludes prices/totals by design, missing orders are represented as `Result<PrintProgram>` failures, and actual printer I/O remains future work.

## Concrete Steps

Work from repository root `/Users/federico.paolillo/src/cashregister`.

Create this plan file first:

    docs/plans/receipt-print-program.md

Then implement the Application model, problem, and service changes, then the database query and DI registrations, then tests.

Run backend verification from `be/` in this exact order:

    dotnet format
    dotnet build
    dotnet test

Expected successful outcome: all three commands complete with exit code 0. `dotnet test` should include the new receipt tests and report all tests passed.

## Validation and Acceptance

The feature is accepted when an integration test can create an order with at least two items and build a receipt program whose rendered content contains:

    ORDER <order number>
    2x <first item description>
    3x <second item description>
    Order ID: <order id>
    Date: <formatted UTC date>

The rendered receipt must not contain item prices, total, subtotal, tax, or payment text.

The missing-order service test must assert that `IReceiptPrintProgramService.BuildAsync` returns `Result<PrintProgram>` with `NotOk == true` and an error of type `NoSuchOrderPrintDataProblem`.

The projection test must prove `IFetchOrderPrintDataQuery` returns null for a missing id and returns the correct order number, id, date, descriptions, and quantities for an existing order.

## Idempotence and Recovery

The implementation is additive except for reshaping `OrderPrintData`, which is currently only a stub for receipt printing. If a verification command fails, fix the reported error and rerun the same command sequence from `be/`.

Do not modify migrations because the feature only reads existing order data and adds no persistence schema.

Do not wire physical printing into order submission in this task. That is separate orchestration work.

## Artifacts and Notes

The rendered Markdown output will include formatting wrappers from `MarkdownRenderer`, such as centered paragraph tags and bold markers. Tests should assert meaningful substrings rather than exact full-document equality unless the exact output is intentionally stabilized.

Example assertions:

    Assert.True(result.Ok);
    var program = result.Value;
    Assert.Contains(order.Number.Value, markdown, StringComparison.Ordinal);
    Assert.Contains("2x Coffee", markdown, StringComparison.Ordinal);
    Assert.Contains(orderId.Value, markdown, StringComparison.Ordinal);
    Assert.DoesNotContain("100", markdown, StringComparison.Ordinal);
    Assert.DoesNotContain("Total", markdown, StringComparison.OrdinalIgnoreCase);

Example missing-order assertion:

    Assert.True(result.NotOk);
    Assert.IsType<NoSuchOrderPrintDataProblem>(result.Error);

## Interfaces and Dependencies

At completion, these interfaces and types must exist:

    namespace Cashregister.Application.Receipts.Services;

    public interface IReceiptPrintProgramService
    {
        Task<Result<PrintProgram>> BuildAsync(Identifier orderId, CancellationToken cancellationToken = default);
    }

    namespace Cashregister.Application.Receipts.Models.Output;

    public sealed class OrderPrintData
    {
        public required Identifier Id { get; init; }
        public required OrderNumber Number { get; init; }
        public required TimeStamp Date { get; init; }
        public required ImmutableArray<OrderPrintDataItem> Items { get; init; }
    }

    public sealed class OrderPrintDataItem
    {
        public required string Description { get; init; }
        public required uint Quantity { get; init; }
    }

    namespace Cashregister.Application.Receipts.Data;

    public interface IFetchOrderPrintDataQuery
    {
        Task<OrderPrintData?> FetchAsync(Identifier orderId, CancellationToken cancellationToken = default);
    }

    namespace Cashregister.Application.Receipts.Problems;

    public sealed record NoSuchOrderPrintDataProblem(Identifier OrderId) : Problem;

The Application project depends on `Cashregister.Printmon` only to expose and build `PrintProgram`. It must not depend on database or API projects. The Database project depends on Application as it already does and provides the concrete query implementation.
