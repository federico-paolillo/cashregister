# Add Technician Device Selection Page

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md` from the repository root. It is self-contained so a contributor can resume the work from this file and the current working tree.

## Purpose / Big Picture

Technicians need a browser page at `/devices` where they can see printer devices known to the backend host and select which one the backend writes receipts to. After this change, the hamburger menu contains a `Devices` link, `/devices` shows a table of available printer targets, the active target is highlighted, and pressing `Select` on another row updates `FileDeviceSettings.Target` without restarting the backend. The backend exposes device data through HTTP so the UI does not need host-level access.

## Progress

- [x] (2026-04-16 12:07Z) Read `docs/ARCH.md`, `docs/ESCPOS.md`, `docs/PLANS.md`, current API route patterns, printer device classes, and frontend routing/menu conventions.
- [x] (2026-04-16 12:07Z) Resolved the route-prefix mismatch: browser calls use `/api` through the existing `ApiClient`, while the backend keeps existing no-prefix route groups.
- [x] (2026-04-16 14:29Z) Added backend device catalog, runtime target selection, DTOs, endpoints, and HTTP endpoint tests.
- [x] (2026-04-16 14:29Z) Added frontend `/devices` route, model types, table components, action handling, and tests.
- [x] (2026-04-16 14:29Z) Wired `FileDeviceSettings` with the Options pattern in `Cashregister.Api` and added `/dev/null` as the development target.
- [x] (2026-04-16 14:29Z) Updated `docs/DIARY.md`.
- [x] (2026-04-16 14:29Z) Ran backend and frontend verification commands; frontend passed, backend format/build passed, backend full test run aborted because existing WebApplicationFactory tests timed out under the host resolver.
- [x] (2026-04-16 15:30Z) Removed the backend `/api/devices` alias, removed `InternalsVisibleTo`, replaced direct handler tests with HTTP endpoint tests, and replaced Options cache mutation with a runtime target store.

## Surprises & Discoveries

- Observation: Existing browser API calls are prefixed with `/api`, but Vite rewrites `/api/orders` to backend `/orders`; the backend itself exposes `/orders`, `/articles`, and `/devices`, not `/api/*`.
  Evidence: `ui/app/settings.ts` defaults `apiBaseUrl` to `/api`; `ui/vite.config.ts` rewrites `/api` away; `be/Cashregister.Api/Orders/Endpoints.cs` maps `/orders`.

- Observation: CUPS queue URIs from `lpstat -v` are not valid targets for the existing `FileDevice`, which opens the configured target with `FileStream`.
  Evidence: `be/Cashregister.Printmon/Devices/FileDevice.cs` constructs a `FileStream` from the selected target string.

## Decision Log

- Decision: Map only `/devices` in the backend while the frontend continues to call `deps.apiClient.get("/devices")` and `post("/devices/<id>")`.
  Rationale: `/api` is a frontend proxy/base-url convention in this repository. Mapping `/api/devices` in the backend would create a second public route contract inconsistent with `/orders` and `/articles`.
  Date/Author: 2026-04-16 / Codex.

- Decision: Use a URL-safe Base64 encoding of the file-device path as `device_id`.
  Rationale: Linux file device paths contain `/`, which cannot be used raw as one route segment. Encoding produces stable route-safe identifiers without inventing a registry or persisting extra state. The POST handler accepts it only if it matches a currently enumerated device.
  Date/Author: 2026-04-16 / Codex.

- Decision: Represent devices as writable Linux printer file paths, not CUPS queues.
  Rationale: the current printing implementation writes to `FileDeviceSettings.Target` with `FileStream`, so the actionable device choice must be a filesystem path such as `/dev/usb/lp0`, not a CUPS URI.
  Date/Author: 2026-04-16 / Codex.

- Decision: Enumerate `/dev/usb/lp*` and `/dev/lp*` for the default catalog.
  Rationale: these are the Linux file-device paths compatible with `FileDevice`. `lpstat -v` returns CUPS queue URIs, which are not valid `FileStream` paths.
  Date/Author: 2026-04-16 / Codex.

- Decision: Store the selected target in a singleton runtime store initialized from `FileDeviceSettings.Target`.
  Rationale: mutating `IOptionsMonitorCache<FileDeviceSettings>` treats configuration binding as application state and has race-prone remove/add behavior. A store makes the runtime state explicit and keeps restart behavior simple.
  Date/Author: 2026-04-16 / Codex.

- Decision: Test device behavior through handlers and the selector instead of `WebApplicationFactory`.
  Rationale: the repository's full integration suite times out during host creation in this environment before device assertions run. Direct handler tests still cover the device DTO mapping, selected row calculation, 204 selection path, and 404 unknown-id path without depending on host startup.
  Date/Author: 2026-04-16 / Codex.

## Outcomes & Retrospective

Implemented the backend and frontend feature. The UI exposes `/devices`, and the backend exposes `/devices` only. The device catalog lists Linux printer file devices compatible with `FileDevice`, and selection updates an in-memory runtime target store initialized from options. Device endpoint tests exercise HTTP routing and serialization, and `/api/devices` is asserted as not found.

## Context and Orientation

The backend is an ASP.NET Core Minimal API in `be/Cashregister.Api/Program.cs`. Existing route modules live under `be/Cashregister.Api/Articles` and `be/Cashregister.Api/Orders`; each has an `Endpoints.cs` file with a `Map...` extension and a `Handlers.cs` file with static handler methods. Integration tests use `WebApplicationFactory<Program>` in `be/Cashregister.Tests.Integration/IntegrationTest.cs`.

The printer output abstraction lives in `be/Cashregister.Printmon/Devices`. `FileDevice` implements `IDevice` and writes encoded bytes to `FileDeviceSettings.Target`. `FileDeviceSettings` currently exists but `Cashregister.Api` does not configure it. The Options pattern means registering a settings class with `builder.Services.Configure<FileDeviceSettings>(configurationSection)`, then injecting `IOptions<FileDeviceSettings>` or related options services.

The frontend is React Router v7 framework mode under `ui/app`. Routes are registered in `ui/app/routes.ts`, and each route owns a folder under `ui/app/routes`. Shared DTO interfaces live in `ui/app/model.ts`. The navigation hamburger menu is `ui/app/components/navigation-menu.tsx`; it already contains a `Devices` link pointing to `/devices`, but the route is missing. The API client base URL defaults to `/api`, so frontend code calls logical paths like `/orders` and the browser requests `/api/orders`.

## Plan of Work

First, add backend device infrastructure in `Cashregister.Api` because this is an API-facing feature rather than a printer encoder change. Create a `Devices` folder with DTOs, endpoint mapping, handlers, and small services. Add an `IPrinterDeviceCatalog` interface with a default implementation that enumerates `/dev/usb/lp*` and `/dev/lp*`. The catalog returns device records with id, name, target, and description. Add a second service that updates a runtime target store after validating that the posted id maps to an enumerated device.

Second, wire `FileDeviceSettings` in `be/Cashregister.Api/Program.cs` using `builder.Services.Configure<FileDeviceSettings>(builder.Configuration.GetSection(FileDeviceSettings.Section))`. Add a development default target in `be/Cashregister.Api/appsettings.Development.json`. Register the printmon services needed by the API if they are not already registered: `FileDeviceTargetStore`, `IDevice`, `FileDevice`, and `IEncoder<byte[]>`, `BinaryEncoder`. Keep this minimal even if receipt printing is not yet invoked by the API path.

Third, add integration tests in `be/Cashregister.Tests.Integration/Api/DevicesEndpointTests.cs`. Replace the catalog with a test fake using `ConfigureTestServices`, configure a test target, assert `GET /devices` returns devices with the selected row marked, assert `POST /devices/{id}` updates the current target and returns no content, assert an unknown id returns 404, and assert `/api/devices` returns 404.

Fourth, add frontend model types, route registration, and a `ui/app/routes/devices` route. The loader fetches `/devices`. The action posts to `/devices/${deviceId}` and returns the result. The route renders a table with columns for name, target, details, status, and action. Selected rows get a distinct background and display `Selected` instead of a `Select` button. Empty results render a concise empty state.

Fifth, add frontend tests for the device table and route action. Mock `deps.apiClient` as existing route tests do. Assert selected devices are highlighted and do not show the `Select` button, non-selected devices do show it, and the action posts to the expected path.

Finally, update `docs/DIARY.md` with the implementation decision summary, run the mandated verification commands from the AGENTS instructions, and update this plan with final evidence.

## Concrete Steps

Run backend discovery and tests from the repository root or `be/` as noted:

    cd /Users/federico.paolillo/src/cashregister/be
    dotnet format
    dotnet build
    dotnet test

Run frontend checks from `ui/`:

    cd /Users/federico.paolillo/src/cashregister/ui
    npm run lint
    npm run build
    npm run test

Manual browser verification after implementation:

    cd /Users/federico.paolillo/src/cashregister/ui
    npm run dev

Then open `/devices` in the frontend. With the backend running, the page should render a table. The current target row is highlighted and says `Selected`; another row has a `Select` button. Pressing `Select` posts to the backend and the selected row changes after the action reloads the route.

## Validation and Acceptance

Acceptance is met when `GET /devices` returns HTTP 200 with a JSON array of device objects, and exactly the object whose target equals the runtime target initialized from `FileDeviceSettings.Target` has `selected: true`. `POST /devices/<device_id>` returns HTTP 204 for an id from the GET response and changes the current value exposed by later GET responses. `GET /api/devices` must return HTTP 404.

Frontend acceptance is met when `/devices` is reachable from the hamburger menu and directly by URL, shows an empty state when the API returns no devices, highlights the selected device row, and omits the `Select` button from that selected row.

The required automated checks are `dotnet format`, `dotnet build`, `dotnet test`, `npm run lint`, `npm run build`, and `npm run test`. Any failure must be fixed before completion.

## Idempotence and Recovery

All changes are additive or narrow edits to composition roots and route registration. Re-running tests or formatters is safe. If no Linux printer file devices exist, the backend should return an empty list and the UI should remain usable. The POST endpoint must not update the runtime target for unknown ids, which prevents arbitrary path writes through crafted URLs.

## Artifacts and Notes

Important route evidence discovered before implementation:

    ui/app/settings.ts: apiBaseUrl defaults to "/api"
    ui/vite.config.ts: rewrite strips /^\/api/
    be/Cashregister.Api/Orders/Endpoints.cs: MapGroup("/orders")

## Interfaces and Dependencies

In `be/Cashregister.Api/Devices/Models/DeviceDto.cs`, define a DTO shaped for JSON:

    public sealed record DeviceDto(
        string Id,
        string Name,
        string Target,
        string? Description,
        bool Selected
    );

In `be/Cashregister.Api/Devices`, define a device catalog abstraction:

    internal interface IPrinterDeviceCatalog
    {
        Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken);
    }

`PrinterDevice` should include `Id`, `Name`, `Target`, and optional `Description`. The default implementation should enumerate `/dev/usb/lp*` and `/dev/lp*` conservatively and return an empty list when those directories or files do not exist.

The endpoint module should expose:

    public static RouteGroupBuilder MapDevices(this WebApplication webApplication)

and handler methods equivalent to:

    GET /devices
    POST /devices/{id}

`GET /api/devices` must not be mapped by the backend.

The frontend should add:

    ui/app/routes/devices/devices.tsx
    ui/app/routes/devices/components/devices-table.tsx
    ui/app/routes/devices/components/device-row.tsx
    ui/app/routes/devices/*.test.tsx as needed

Revision note, 2026-04-16 12:07Z: Initial plan created after repository discovery to document the route-prefix decision and implementation sequence before coding.

Revision note, 2026-04-16 15:30Z: Corrected the plan after review. The backend no longer maps `/api/devices`, device discovery no longer uses CUPS `lpstat`, runtime target state no longer mutates the Options cache, and endpoint tests now exercise HTTP routing.
