# Printmon Worker

## Purpose

Implement assigned ESC/POS, receipt printing, emulator, file-device, or CLI work while keeping Printmon behavior aligned with `docs/ESCPOS.md`.

## Required Reading

- `AGENTS.md`
- `docs/ARCH.md`
- `docs/ESCPOS.md`
- `.agents/skills/cashregister-general/SKILL.md`
- `.agents/skills/cashregister-backend/SKILL.md`
- `.agents/skills/cashregister-printmon/SKILL.md`
- Assigned task handoff.

Read `docs/MANUAL.md` and `docs/PRINTER.md` only when command-level printer behavior requires them.

## Ownership

- `be/Cashregister.Printmon/`
- `be/Cashregister.Printmon.Emulator/`
- `be/Cashregister.Printmon.Tests/`
- `be/Cashregister.Printmon.Emulator.Tests/`
- `be/Cashregister.Cli/`
- Print-related application or API adapters only when explicitly assigned.
- `docs/ESCPOS.md` when Printmon behavior changes.

## Forbidden Edits

- Do not edit UI or non-printing backend surfaces unless explicitly assigned.
- Do not add ESC/POS instruction records without updating builder, encoders, decoder or executor where applicable, tests, and `docs/ESCPOS.md`.
- Do not bypass `PrintProgramBuilder` in application code unless the assignment is a low-level test.
- Do not introduce hardware assumptions not documented in canonical docs.

## Workflow Rules

- Keep command-level behavior in `docs/ESCPOS.md`.
- Keep high-level receipt orchestration aligned with `docs/ARCH.md`.
- Preserve the immutable `PrintProgram` and pure emulator model.
- Keep encoder mappings deterministic and covered by tests.
- Record assumptions and unsupported hardware behavior rather than adding speculative support.

## Verification

Run focused Printmon tests first. For completed Printmon source changes, run from `be/`:

- `dotnet format`
- `dotnet build`
- `dotnet test`

For documentation-only Printmon coordination changes, `git diff --check` is sufficient unless the coordinator assigns additional checks.

## Escalation

Escalate when printer manual behavior is unclear, command semantics conflict with existing encoder or emulator behavior, or changes affect receipt API behavior outside Printmon.

## Final Report

Report changed Printmon paths, `docs/ESCPOS.md` updates, tests added or updated, commands run, assumptions, skipped checks with reasons, and remaining risks.
