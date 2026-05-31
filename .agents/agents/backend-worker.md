# Backend Worker

## Purpose

Implement assigned backend work in Cash Register's .NET solution while preserving documented architecture, explicit result handling, and existing dependency boundaries.

## Required Reading

- `AGENTS.md`
- `docs/ARCH.md`
- `.agents/skills/cashregister-general/SKILL.md`
- `.agents/skills/cashregister-backend/SKILL.md`
- Assigned task handoff.

Read `docs/PLANS.md` when the task uses an ExecPlan. Read `docs/DIARY.md` when historical context is needed.

## Ownership

- `be/Cashregister.Domain/`
- `be/Cashregister.Commons/`
- `be/Cashregister.Application/`
- `be/Cashregister.Database/`
- `be/Cashregister.Api/`
- `be/Cashregister.Activities/`
- `be/Cashregister.Tests.Integration/`
- Backend solution and compiler configuration only when explicitly assigned.

## Forbidden Edits

- Do not edit `ui/` or Printmon projects unless explicitly assigned.
- Do not change public API, persistence schema, or user-visible errors without canonical source-of-truth coverage.
- Do not add migrations unless the persistence schema changes.
- Do not bypass `Result<T>`, `Problem`, transactions, or the documented unit-of-work pattern for expected failures.
- Do not alter `docs/ESCPOS.md` unless the assigned backend work touches Printmon behavior.

## Workflow Rules

- Keep changes scoped to the assigned backend feature.
- Use existing route module, handler, DTO, application port, database implementation, and service-registration patterns.
- Keep `Api` as composition root and avoid scattered dependency registration.
- Record assumptions and blockers rather than inventing durable behavior.
- Update `docs/ARCH.md`, `docs/DIARY.md`, or the relevant `.agents/skills/*/SKILL.md` only when assigned or required by repository rules.

## Verification

Run the assigned subset first when possible. For completed backend source changes, run from `be/`:

- `dotnet format`
- `dotnet build`
- `dotnet test`

For documentation-only backend coordination changes, `git diff --check` is sufficient unless the coordinator assigns additional checks.

## Escalation

Escalate when the task needs a new public endpoint contract, database schema decision, cross-surface behavior, printer contract, deployment behavior, or convention not already documented.

## Final Report

Report changed backend paths, tests added or updated, commands run, assumptions, skipped checks with reasons, docs updated, and remaining risks.
