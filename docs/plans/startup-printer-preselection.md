# Preselect the first discovered printer at API startup

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This plan follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register currently asks technicians to select a printer target before physical receipt output can use an enumerated Linux printer file device. The API should remove that step for the common one-printer case by selecting the first discovered printer during startup. The startup choice must be explicit runtime state: it should not silently depend on a configured target, and attempts to print without any selected target must fail instead of throwing away a receipt.

## Progress

- [x] (2026-05-22) Reviewed the existing startup change, device-selection services, printer device behavior, tests, and documentation.
- [x] (2026-05-22) Implemented runtime printer target state with API startup preselection and CLI direct target selection.
- [x] (2026-05-22) Updated tests for startup preselection and no-selected-target print failure.
- [x] (2026-05-22) Updated printer-selection documentation and diary bookkeeping, then ran backend verification.

## Surprises & Discoveries

- Observation: The first startup hook already changes existing behavior because it selects the first catalog printer even when tests configured a different `FileDeviceSettings.Target`.
  Evidence: `dotnet test Cashregister.Tests.Integration/Cashregister.Tests.Integration.csproj --filter FullyQualifiedName~DevicesEndpointTests` fails `GetDevices_ReturnsDevicesWithSelectedDevice` after the startup hook selects `/dev/usb/lp0`.

## Decision Log

- Decision: API printer selection will no longer use `FileDeviceSettings.Target` as startup input.
  Rationale: The requested improvement should use discovered printers at startup so the user does not select the active printer for the common case.
  Date/Author: 2026-05-22 / Codex
- Decision: Printing with no selected file target returns an explicit device failure.
  Rationale: Missing printer output must not be silently discarded or skipped by order workflows.
  Date/Author: 2026-05-22 / Codex

## Outcomes & Retrospective

API startup now preselects the first discovered printer, file-device target state is runtime-only, and CLI printing still sets its explicit target before output. Printmon reports a dedicated problem when no physical file target is selected. Backend formatting, build, and tests complete successfully.

## Context and Orientation

Backend printer selection is split across Application and Printmon code. `be/Cashregister.Application/Devices/Services/Defaults/FilePrinterDeviceCatalog.cs` discovers writable Linux printer file paths. `FileDeviceTargetSelector` selects a catalog item and writes its target path into the singleton `be/Cashregister.Printmon/Devices/FileDeviceTargetStore.cs`. `be/Cashregister.Api/Devices/Handlers.cs` compares each discovered target with the current runtime target to expose the selected row through `/devices`.

`be/Cashregister.Printmon/Devices/FileDevice.cs` writes encoded receipt bytes to the active file target. The API starts in `be/Cashregister.Api/Program.cs`, where it already performs database migrations before serving requests. The CLI also uses `FileDevice` from `be/Cashregister.Cli`, but it receives an explicit `--device` option and should keep that direct workflow.

## Plan of Work

First, replace the configured startup target model with an initially empty runtime target store. Keep catalog validation in `FileDeviceTargetSelector`, run API preselection after migrations, and let `/devices` report no selected row when no target has been selected.

Second, update `FileDevice` to return a Printmon device problem when the runtime store has no target. Remove the now-unused file-device target settings from API registration and development settings. Set the CLI runtime target from its existing `--device` argument before invoking the print tool.

Third, extend backend tests where the behavior belongs. Device API integration tests should prove startup preselection and empty-catalog startup state. Printmon tests should prove an unselected `FileDevice` fails before file I/O. Then update `docs/ESCPOS.md`, any high-level architecture note that changed, and append the diary entry for the implementation decision.

## Concrete Steps

Work from the repository root unless a command names another directory.

1. Edit the printer target store, file device problem path, API startup registration, CLI setup, and affected tests.
2. From `be/`, run:

       dotnet format
       dotnet build
       dotnet test

3. Review the diff and ensure unrelated worktree changes were not rewritten.

Completed verification:

       dotnet format
       dotnet build
       dotnet test

## Validation and Acceptance

Acceptance is met when API startup with a catalog containing multiple printers marks only the first catalog item as selected without an HTTP selection call. Startup with no discovered printers still completes and exposes no selected device. Manual `POST /devices/{id}` still changes selection. A physical `FileDevice` print attempt with no selected target returns the dedicated problem instead of opening a file, while the CLI still accepts its explicit `--device` argument.

## Idempotence and Recovery

The edits are source changes and tests only. Verification commands may rebuild output and caches but do not require database or printer cleanup. If a focused test fails, keep the printer state contract intact and adjust the failing boundary rather than restoring the old configured startup target.

## Artifacts and Notes

The focused pre-implementation failure is:

    Failed Cashregister.Tests.Integration.Api.DevicesEndpointTests.GetDevices_ReturnsDevicesWithSelectedDevice
    Assert.False() Failure
    Expected: False
    Actual:   True

## Interfaces and Dependencies

`FileDeviceTargetStore.CurrentTarget` and `FileDeviceTargetSelector.CurrentTarget` must permit no selected target. `FileDeviceTargetStore.Select(string)` remains the state mutation used by validated API selections and by the CLI explicit device option. Printmon owns the new device `Problem` for missing target state so application and API receipt workflows can propagate it through their existing result handling.
