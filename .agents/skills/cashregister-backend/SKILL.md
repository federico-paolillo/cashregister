---
name: cashregister-backend
description: Use for Cash Register backend work in the .NET domain, application, database, API, activities, and integration-test projects.
---

# Cash Register Backend Skill

## Owned Paths

Backend work normally lives under:

- `be/Cashregister.Domain/`
- `be/Cashregister.Commons/`
- `be/Cashregister.Application/`
- `be/Cashregister.Database/`
- `be/Cashregister.Api/`
- `be/Cashregister.Activities/`
- `be/Cashregister.Tests.Integration/`

Backend runtime contracts and architecture are documented in `docs/ARCH.md`. Backend implementation practice is documented in this skill.

## Conventions

- Target the repository's configured .NET and C# versions.
- Use nullable reference types, implicit usings, latest analysis, and warnings-as-errors expectations from `be/Directory.Build.props`.
- Do not add unnecessary using directives; implicit usings are enabled.
- Add XML summary comments to classes and interfaces.
- Use `Result<T>` plus `Problem` for expected business or application failures.
- Use exceptions only for invalid programmer input, impossible states, or unexpected infrastructure failures.
- Use `Transaction<TInput, TOutput>` for business operations that need unit-of-work behavior.
- Put default implementations of interfaces under a `Defaults/` folder in the parent feature folder.
- Keep interfaces lightweight and test-driven. Do not introduce abstractions for one-off code unless they isolate an external dependency or make meaningful tests possible.
- Keep application features organized by domain area, for example `Articles`, `Orders`, and `Receipts`.
- Treat `*Activity` classes as emulators of out-of-process sagas where each step executes in an independent `Scope<T>`.
- Keep application ports in Application and EF Core implementations in Database.
- Keep API route groups under `Cashregister.Api/<Feature>/` with `Endpoints.cs`, `Handlers.cs`, and `Models/`.
- Prefer `TypedResults` high-level Minimal API methods and typed result types from `Microsoft.AspNetCore.Http.HttpResults`, such as `InternalServerError`, instead of generic status-code results.
- Register dependencies through feature-specific `ServiceCollectionExtensions`.

## Persistence

- Use EF Core SQLite patterns already present in `Cashregister.Database`.
- Keep EF Core entities under `Cashregister.Database/Entities`.
- Keep persistence-to-domain mapping under `Mappers`.
- Keep queries under `Queries` and commands under `Commands`.
- Use `AsNoTracking()` for read-only projections.
- Add migrations only when the schema changes.
- Preserve the unit-of-work behavior where successful transactions save and failures roll back or discard changes.

## Testing

- Add or update tests for backend behavior changes.
- Cover at least the happy path and main failure path when a change adds behavior.
- Prefer integration tests when the real DI graph, EF Core behavior, or API boundary matters.
- Prefer focused unit tests only for pure helpers, value objects, builders, encoders, and renderers.
- Use typed request DTOs and typed HTTP helpers in API tests unless testing malformed JSON or serialization boundaries.

## Validation

For backend source changes, run from `be/`:

    dotnet format
    dotnet build
    dotnet test

For documentation-only backend coordination changes, run `git diff --check` from the repository root.

## Common Pitfalls

- Do not add a one-off abstraction unless it isolates an external dependency or enables meaningful tests.
- Do not scatter service registration in unrelated projects.
- Do not map backend routes under `/api`; `/api` is a frontend/proxy convention.
- Do not leave expected application problems unmapped at the API boundary.
- Do not change cents-only API money contracts without canonical documentation and matching tests.
