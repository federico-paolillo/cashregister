# Refresh Architecture, ESC/POS, and Conventions Documentation

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md` from the repository root. It is self-contained so a contributor can resume the work from this file and the current working tree.

## Purpose / Big Picture

The project documentation currently mixes architecture, frontend conventions, and stale ESC/POS printer details. After this change, `docs/ARCH.md` describes the current application architecture at a high level, `docs/ESCPOS.md` is the dedicated source of truth for `Cashregister.Printmon.*`, and root `CONVENTIONS.md` captures backend and frontend development conventions that can later be folded into `AGENTS.md`. A contributor can verify the result by comparing the docs to the current backend projects, frontend routes, and Printmon implementation without reading unrelated docs.

## Progress

- [x] (2026-04-19) Read the current `docs/ARCH.md`, `docs/ESCPOS.md`, backend project layout, API composition, frontend routing/configuration, Printmon builder, encoders, emulator, CLI tools, and diary format.
- [x] (2026-04-19) Confirmed the worktree already has an unrelated `AGENTS.md` modification that must not be edited.
- [x] (2026-04-19) Created this ExecPlan before changing documentation content.
- [x] (2026-04-19) Rewrite `docs/ARCH.md` as high-level architecture only, with a short Printmon note and a link to `docs/ESCPOS.md`.
- [x] (2026-04-19) Update `docs/ESCPOS.md` to match the current Printmon, emulator, and CLI code.
- [x] (2026-04-19) Add root `CONVENTIONS.md` for backend and frontend development rules.
- [x] (2026-04-19) Append a concise `docs/DIARY.md` entry.
- [x] (2026-04-19) Run documentation verification and record the outcome here.

## Surprises & Discoveries

- Observation: `docs/ARCH.md` describes `ui/` under the backend project tree, but the actual repository has sibling `be/` and `ui/` directories.
  Evidence: `rg --files` lists `be/Cashregister.slnx` and `ui/package.json` at sibling paths.

- Observation: `docs/ESCPOS.md` names stale builder APIs such as `EmphasizeOn`, `Justify`, and `Cut`, while the current builder exposes `BoldOn`, `Align`, `PartialCut`, `FeedAndCut`, and other renamed methods.
  Evidence: `be/Cashregister.Printmon/PrintProgramBuilder.cs` contains the current method names and no `EmphasizeOn`, `Justify`, or `Cut` methods.

- Observation: The current decoder and executor use explicit `Result<T>` plus `Problem` values, not exceptions for expected decode/execution failures.
  Evidence: `InstructionDecoder.Decode` returns `Result<PrintProgram>` and `InstructionExecutor.Execute` returns `Result<Printer>`.

## Decision Log

- Decision: Keep detailed ESC/POS instruction, encoder, emulator, CLI, and printer-device rules in `docs/ESCPOS.md`, not `docs/ARCH.md`.
  Rationale: Printmon is a large subsystem with command-level rules. Duplicating that detail in architecture documentation would create drift.
  Date/Author: 2026-04-19 / Codex.

- Decision: Put `CONVENTIONS.md` at repository root.
  Rationale: The user intends to use it to update `AGENTS.md`, and root placement makes it independent from architecture-specific docs.
  Date/Author: 2026-04-19 / Codex.

- Decision: Treat `Printom` in the request as `Printmon`.
  Rationale: The repository projects and namespaces consistently use `Cashregister.Printmon`.
  Date/Author: 2026-04-19 / Codex.

## Outcomes & Retrospective

Completed. `docs/ARCH.md` now covers high-level backend/frontend architecture and links to `docs/ESCPOS.md` for Printmon details. `docs/ESCPOS.md` now matches the current Printmon builder, encoders, emulator, CLI, device model, and test layout. Root `CONVENTIONS.md` captures backend and frontend development conventions for future `AGENTS.md` updates. `docs/DIARY.md` records the documentation split and the decision not to touch source code or the unrelated `AGENTS.md` modification.

Verification completed with `git diff --check`. Full backend/frontend builds and tests were not run because this task intentionally changed documentation only and source formatters were out of scope.

## Context and Orientation

The repository root is `/Users/federico.paolillo/src/cashregister`. Backend code lives under `be/` and frontend code under `ui/`. Backend solution membership is defined in `be/Cashregister.slnx`. Shared backend settings are in `be/Directory.Build.props`, which sets .NET 10, C# 14, nullable reference types, implicit usings, latest analysis, and warnings as errors.

The backend entrypoint is `be/Cashregister.Api/Program.cs`. It configures logging, environment variables with the `CASHREGISTER_` prefix, database/application/receipt services, file printer devices, and maps `/articles`, `/orders`, and `/devices`. The frontend default API base URL is `/api`; during development `ui/vite.config.ts` proxies `/api/*` to `http://localhost:5122` and strips the prefix. The backend itself does not map `/api/*`.

The Printmon subsystem lives under `be/Cashregister.Printmon`, `be/Cashregister.Printmon.Emulator`, and `be/Cashregister.Cli`, with tests in `be/Cashregister.Printmon.Tests` and `be/Cashregister.Printmon.Emulator.Tests`. `PrintProgramBuilder` constructs immutable `PrintProgram` values. Encoders transform programs into bytes or string tokens. `FileDevice` writes encoded bytes to a selected Linux file device. The emulator decodes bytes into instructions, executes them into immutable printer states and receipt elements, then renders Markdown.

## Plan of Work

First, replace `docs/ARCH.md` with a concise architecture document. It must describe repository shape, backend layers and dependency direction, API and runtime composition, persistence, frontend architecture, testing, and operational constraints. Its Printmon section must be brief and must direct readers to `docs/ESCPOS.md` for command-level details.

Second, replace `docs/ESCPOS.md` with a current Printmon reference. It must document project layout, invariants, builder behavior, device model, encoders, implemented instructions, emulator behavior, CLI usage, and tests. The instruction table must match the current instruction records, builder methods, binary encoding, and string encoding.

Third, add `CONVENTIONS.md` at repository root. It must capture backend and frontend coding conventions for future development, without duplicating the full architecture or ESC/POS command table.

Fourth, append a `docs/DIARY.md` entry titled `Architecture, ESC/POS, and conventions documentation refresh`. The entry must reference `plans/arch-escpos-conventions-refresh.md` relative to `docs/` and summarize the key documentation separation decisions.

Finally, run `git diff --check`. Because this is documentation-only, full backend/frontend verification is optional confidence checking; if run and it fails without source edits, record the existing failure instead of changing code.

## Concrete Steps

Work from repository root:

    cd /Users/federico.paolillo/src/cashregister
    git status --short

Create and maintain this ExecPlan:

    docs/plans/arch-escpos-conventions-refresh.md

Edit:

    docs/ARCH.md
    docs/ESCPOS.md
    CONVENTIONS.md
    docs/DIARY.md

Verify:

    git diff --check

Optional confidence checks:

    cd /Users/federico.paolillo/src/cashregister/be
    dotnet build
    dotnet test

    cd /Users/federico.paolillo/src/cashregister/ui
    npm run lint
    npm run build
    npm run test

## Validation and Acceptance

Acceptance is met when `docs/ARCH.md` no longer duplicates ESC/POS instruction details and accurately describes the current backend/frontend architecture. `docs/ESCPOS.md` must match the current Printmon builder, encoders, emulator, CLI, and test projects. `CONVENTIONS.md` must be present at repository root and contain practical backend/frontend conventions. `docs/DIARY.md` must include the completed task entry. `git diff --check` must exit successfully.

## Idempotence and Recovery

The task edits Markdown only. Re-running `git diff --check` is safe. If a documentation edit accidentally touches `AGENTS.md` or source code, revert only those accidental edits, not the pre-existing `AGENTS.md` change. If optional code verification fails, document the failure and do not broaden the task into source fixes.

## Artifacts and Notes

Initial worktree status before edits:

    M AGENTS.md

Important architecture evidence:

    be/Cashregister.Api/Program.cs maps Articles, Orders, and Devices.
    ui/app/settings.ts defaults apiBaseUrl to "/api".
    ui/vite.config.ts proxies "/api" to "http://localhost:5122" and rewrites the prefix away.

Important Printmon evidence:

    PrintProgramBuilder.Build() appends LineFeedInstruction and CutAfterInstruction(1).
    BinaryEncoder and StringEncoder switch over instruction types and throw NotSupportedException only for unsupported instruction types.
    InstructionDecoder.Decode returns Result<PrintProgram>.
    InstructionExecutor.Execute returns Result<Printer>.

Verification evidence:

    git diff --check
    # exited successfully with no output

## Interfaces and Dependencies

No runtime interfaces or dependencies change. The documentation must refer to existing public surfaces:

    Cashregister.Printmon.PrintProgram
    Cashregister.Printmon.PrintProgramBuilder
    Cashregister.Printmon.Encoders.IEncoder<TOutput>
    Cashregister.Printmon.Devices.IDevice
    Cashregister.Printmon.Devices.FileDeviceTargetStore
    Cashregister.Printmon.Emulator.IInstructionDecoder
    Cashregister.Printmon.Emulator.IInstructionExecutor
    Cashregister.Printmon.Emulator.IPrinterEmulator
    Cashregister.Printmon.Emulator.IMarkdownRenderer

Revision note, 2026-04-19: Initial ExecPlan created from the accepted implementation plan and current repository discovery.

Revision note, 2026-04-19: Completed the documentation rewrite, added conventions and diary entry, and recorded verification.
