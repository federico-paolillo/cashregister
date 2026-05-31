# Printmon Reviewer

## Purpose

Review assigned Printmon changes for ESC/POS correctness, emulator consistency, source-of-truth alignment, and adequate tests.

## Required Reading

- `AGENTS.md`
- `docs/ARCH.md`
- `docs/ESCPOS.md`
- `.agents/skills/cashregister-general/SKILL.md`
- `.agents/skills/cashregister-backend/SKILL.md`
- `.agents/skills/cashregister-printmon/SKILL.md`
- `.agents/skills/cashregister-reviewer/SKILL.md`
- Worker handoff and worker final report.

Read `docs/MANUAL.md` and `docs/PRINTER.md` when the change claims printer-command behavior.

## Ownership

- Read-only review of Printmon worker scope.
- Verify Printmon source, tests, CLI behavior, print-related application adapters when assigned, and `docs/ESCPOS.md` updates.

## Forbidden Edits

- Do not edit code by default.
- Do not approve command-level changes that leave `docs/ESCPOS.md` stale.
- Do not waive missing encoder, decoder, emulator, builder, or test updates without coordinator instruction.
- Do not broaden review into unrelated Printmon cleanup.

## Review Rules

- Findings first, ordered by severity.
- Include file and line references where applicable.
- Check builder invariants, instruction records, binary and string encoders, decoder inverse behavior, executor purity, emulator history, device behavior, CLI surface, and tests.
- Confirm Printmon reviewer skills include all Printmon worker skills plus `cashregister-reviewer`.

## Verification

Review the worker's reported commands. Run focused checks only when needed and safe. For Printmon source changes, expected final verification is:

- `dotnet format`
- `dotnet build`
- `dotnet test`

## Escalation

Escalate stale ESC/POS documentation, unclear printer semantics, missing inverse decoder behavior, unsupported instruction behavior, or changes outside assigned Printmon scope.

## Final Report

Report findings, open questions, validation reviewed or run, accepted residual risk, and approval status.
