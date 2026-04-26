# Cash Register Development Conventions

> Use this file as the compact development convention reference for backend and frontend work. Architecture belongs in `docs/ARCH.md`; ESC/POS command, encoder, emulator, CLI, and printer-device details belong in `docs/ESCPOS.md`.

## Backend

Backend code lives under `be/` and targets .NET 10 with C# 14. `be/Directory.Build.props` enables nullable reference types, implicit usings, latest analysis, and warnings as errors.

Conventions:

- Do not add unnecessary `using` directives; implicit usings are enabled.
- Add `/// <summary>` comments on classes and interfaces stating their purpose.
- Use explicit `Result<T>` plus `Problem` for expected business or application failures. Do not use exceptions for normal control flow.
- Use exceptions for invalid programmer input, impossible states, and infrastructure failures that are not part of the expected application contract.
- Use `Transaction<TInput, TOutput>` for business operations that need unit-of-work behavior.
- Default implementations of interfaces go under a `Defaults/` folder of the parent feature folder.
- Keep interfaces lightweight and test-driven. Do not introduce abstractions for one-off code unless they isolate an external dependency or make meaningful tests possible.
- Keep application features organized by domain area, for example `Articles`, `Orders`, and `Receipts`.
- Register feature services through `ServiceCollectionExtensions` methods instead of scattering registrations in unrelated projects.
- `*Activity` classes are emulators of out-of-process sagas where each step executes in a indipendent `Scope<T>` 
- **Always prefer** using `TypeResults` high-level methods from ASP .NET Minimal APIs. **Do** `Task<Results<BadRequest, InternalServerError, Created<EntityPointerDto>>>` instead of`Task<Results<BadRequest, StatusCodeHttpResult, Created<EntityPointerDto>>>`. **Do** `TypedResults.InternalServerError()` instead of `TypedResults.StatusCode(StatusCodes.Status500InternalServerError)`
- Make ad-hoc `ServiceCollectionExtensions` extensions and provide `AddXxx()` methods to register dependencies. Don't scatter around the codebase dependencies registration code.

Minimal API conventions:

- Route modules live under `Cashregister.Api/<Feature>/`.
- Use `Endpoints.cs` for route-group mapping and `Handlers.cs` for static handler methods.
- Put HTTP DTOs under `Models/`.
- Keep backend routes unprefixed: `/articles`, `/orders`, and `/devices`. `/api` is a frontend/proxy convention.
- Prefer typed results from `Microsoft.AspNetCore.Http.HttpResults`.
- Map expected application problems to appropriate HTTP responses at the API boundary.

Persistence conventions:

- Application defines query and command interfaces; Database implements them.
- EF Core entity classes live under `Cashregister.Database/Entities`.
- Persistence-to-domain mapping lives under `Mappers`.
- Queries belong under `Queries`; commands belong under `Commands`.
- Use `AsNoTracking()` for read-only EF projections.
- Do not add migrations unless the persistence schema changes.
- Preserve the explicit unit-of-work pattern: successful transactions save, failed transactions roll back or discard changes.

Testing conventions:

- Tests are mandatory for backend code changes. Cover at least the happy path and the main failure path when the change adds behavior.
- Use integration tests for application/database/API behavior when the real DI graph matters.
- Use focused unit tests for pure helpers, value objects, builders, encoders, and renderers.
- In API integration tests, prefer proper request DTO types and typed HTTP helpers such as `PostAsJsonAsync`. Use raw JSON or `StringContent` only for malformed payloads, unknown fields, or explicit serialization-boundary tests.
- Run backend verification from `be/`:

```bash
dotnet format
dotnet build
dotnet test
```

## Frontend

Frontend code lives under `ui/` and uses React 19, React Router v7 framework mode with `ssr: false`, TypeScript, Vite, Tailwind CSS v4, and Vitest.

Conventions:

- Routes are registered in `ui/app/routes.ts`.
- Each route has its own folder under `ui/app/routes/`; the route component file name matches the folder name.
- Components used by only one route live under that route's `components/` folder.
- Components shared by multiple routes or by the root layout live under `ui/app/components/`.
- Keep one React component per `.tsx` file, except route files may also export React Router loaders, actions, and boundaries.
- In route components, rely on React Router generated `Route.ComponentProps` typing for `loaderData`; do not add local `LoaderData` interfaces or cast `loaderData` to a manually maintained type.
- Use `<>...</>` fragments, not `<Fragment>`, unless an explicit keyed fragment is required.
- Use `@cashregister/*` imports for app modules instead of deep relative paths, except for local sibling route components where existing code already uses relative imports.
- Use `ui/app/deps.ts` as the composition root for root-level dependencies.
- Use `deps.apiClient` in loaders, actions, and modules that call the backend. Do not create ad hoc fetch wrappers.
- Keep DTO interfaces in `ui/app/model.ts`.
- Client API calls return `Result<T>`. Handle loader/action errors explicitly and surface user-facing failures through the existing error-message system.
- Keep money formatting and parsing in `ui/app/money.ts`.
- Use `ui/app/components/money-input.tsx` for user-entered money amounts. Users see decimal strings, while forms submit hidden integer cent fields such as `priceInCents` and `totalOverrideInCents`.

Styling conventions:

- Tailwind CSS v4 is configured through the Vite plugin and `ui/app/app.css`.
- Put shared component utility classes in `@layer components` in `app.css`.
- Reuse existing classes such as `btn-primary`, `btn-secondary`, `btn-outline`, and `input-field` instead of repeating large class strings.
- Keep layouts stable at kiosk resolution; avoid hover/state changes that resize tables, controls, or summary panels.

Testing conventions:

- Frontend tests are colocated as `ui/app/**/*.test.{ts,tsx}`.
- Use Vitest, jsdom, React Testing Library, and `@testing-library/user-event`.
- Mock `deps.apiClient` for route loader/action tests when backend behavior is not the subject.
- Prefer assertions on user-visible behavior over component internals.
- Run frontend verification from `ui/`:

```bash
npm run lint
npm run build
npm run test
```

## Printmon

Keep generic backend conventions here. Keep Printmon-specific rules in `docs/ESCPOS.md`, including:

- instruction record requirements;
- builder behavior;
- binary and string encoder mappings;
- decoder, executor, emulator, and renderer behavior;
- file-device and CLI details;
- instruction tables and ESC/POS byte sequences.

When changing `Cashregister.Printmon.*`, update `docs/ESCPOS.md` in the same task.

## Documentation-Only Changes

For documentation-only tasks:

- Run `git diff --check`.
- Do not run formatters that rewrite code.
- Full backend/frontend verification is optional confidence checking unless source code changed.
- If optional code verification fails without source edits, report it as existing repository state rather than broadening the task into a code fix.
