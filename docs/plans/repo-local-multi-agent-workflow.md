# Add repo-local multi-agent workflow tooling

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This document follows `docs/PLANS.md`. It describes a documentation and workflow-tooling change only. It must not modify runtime product code.

## Purpose / Big Picture

Cash Register needs repo-local agent profiles, skills, and workflow templates so a coordinating agent can split feature work across backend, Printmon, and UI surfaces, route each surface through matching review, and then integrate approved work. The result is observable by inspecting the `.agents` tree and reading the root instructions: `.agents` becomes available as workflow tooling and the home for agent-facing implementation practice, while `AGENTS.md`, `docs/ARCH.md`, `docs/ESCPOS.md`, `docs/PLANS.md`, and `docs/DIARY.md` remain the canonical project sources of truth for product behavior, runtime contracts, planning, and bookkeeping.

## Progress

- [x] (2026-05-31 20:46Z) Inspected `PROMPT.md`, root `AGENTS.md`, architecture, ESC/POS, conventions, Docker, planning, package, and toolchain files.
- [x] (2026-05-31 20:46Z) Chose three implementation surfaces: backend, Printmon, and UI.
- [x] (2026-05-31 20:46Z) Created this ExecPlan at `docs/plans/repo-local-multi-agent-workflow.md`.
- [x] (2026-05-31 20:46Z) Added `.agents` role profiles, skills, and workflow templates.
- [x] (2026-05-31 20:46Z) Added root and convention pointers and appended one diary entry.
- [x] (2026-05-31 20:46Z) Ran structural and diff validation.
- [x] (2026-05-31 21:01Z) Migrated active standalone convention guidance into `.agents/skills/*`.
- [x] (2026-05-31 21:01Z) Removed active references to the deleted standalone convention documents.

## Surprises & Discoveries

- Observation: The working tree already had modified `docs/DIARY.md` and `docs/v2.MD` before this task.
  Evidence: `git status --short --branch` showed `M docs/DIARY.md` and `M docs/v2.MD`.

## Decision Log

- Decision: Use backend, Printmon, and UI as the only worker/reviewer role pairs.
  Rationale: These match the repository's independently assignable implementation surfaces without overfitting workflow tooling to DevOps-only maintenance tasks.
  Date/Author: 2026-05-31 / Codex

- Decision: Keep `.agents` explicitly non-canonical.
  Rationale: Cash Register already has canonical architecture, ESC/POS, convention, planning, and diary documents. Workflow tooling must not replace durable product contracts.
  Date/Author: 2026-05-31 / Codex

- Decision: Run documentation and structure validation only.
  Rationale: The implementation changes no runtime code, package manifests, migrations, or configuration consumed by the application.
  Date/Author: 2026-05-31 / Codex

- Decision: Move agent-facing implementation practice from standalone convention documents into `.agents/skills/*`.
  Rationale: Keeping the same guidance in canonical docs and skills creates drift. Product behavior and runtime contracts remain in canonical docs; implementation practice for agents belongs in skills.
  Date/Author: 2026-05-31 / Codex

## Outcomes & Retrospective

The repository now contains local multi-agent workflow tooling for significant parallel feature work. The tooling points agents back to canonical project documents for durable behavior and contracts. Agent-facing backend, frontend, Printmon, documentation-only, Docker, and shell-script practices live in `.agents/skills/*`. Runtime verification was intentionally skipped because this task only added Markdown workflow files and documentation pointers.

## Context and Orientation

The repository root is Cash Register. Runtime code is split between `be/`, a .NET 10 backend solution, and `ui/`, a React 19 / React Router v7 frontend. Printmon code under `be/Cashregister.Printmon*` and `be/Cashregister.Cli` implements ESC/POS printer programs, encoding, emulation, and development CLI tooling.

Canonical repository instructions live in `AGENTS.md`. High-level application architecture lives in `docs/ARCH.md`. ESC/POS and Printmon contracts live in `docs/ESCPOS.md`. ExecPlan rules live in `docs/PLANS.md`, and task history lives in `docs/DIARY.md`. Agent-facing implementation practice lives in `.agents/skills/*`.

The `.agents` directory created by this plan is workflow tooling and implementation-practice guidance. It can coordinate agent work, but it is not source of truth for product behavior, public interfaces, persistence, deployment behavior, or rebuild-relevant decisions.

## Plan of Work

Create role profiles under `.agents/agents/` for one coordinator, one merger, and three worker/reviewer pairs: backend, Printmon, and UI. Keep role profiles focused on ownership, allowed and forbidden edits, workflow, verification, escalation, and final reporting.

Create skills under `.agents/skills/` for shared Cash Register practice, each implementation surface, reviewing, integrating, agentic feature coordination, and planning. Each skill must have lowercase hyphenated frontmatter with `name` and `description`. Skills can restate implementation guidance but must point back to canonical docs for durable contracts.

Create workflow templates under `.agents/workflows/templates/` for feature state, task handoff, review reports, and integration reports. Each template must state that it is scratch coordination state and does not replace canonical repository docs.

Add a short `.agents` pointer to `AGENTS.md`. Append diary entries to `docs/DIARY.md`. Remove obsolete active convention documents after migrating their guidance into skills. Do not touch `docs/v2.MD`, runtime code, migrations, package files, `docs/ARCH.md`, or `docs/ESCPOS.md`.

## Concrete Steps

From the repository root, create the required directories:

    mkdir -p .agents/agents .agents/skills/cashregister-agentic-feature .agents/skills/cashregister-general .agents/skills/cashregister-backend .agents/skills/cashregister-printmon .agents/skills/cashregister-ui .agents/skills/cashregister-reviewer .agents/skills/cashregister-integrator .agents/skills/planner-agent .agents/workflows/templates

Add the role, skill, and template Markdown files. Then add the root documentation pointer and diary entry. When removing old convention documents, migrate all active guidance into the relevant skills before deletion.

## Validation and Acceptance

Run these commands from the repository root:

    find .agents/agents -maxdepth 1 -type f | sort
    find .agents/skills -maxdepth 2 -name SKILL.md | sort
    find .agents/workflows/templates -maxdepth 1 -type f | sort
    rg -n 'archivist|go-developer-agent|developer-agent.md|canonical product truth|canonical rebuild' .agents AGENTS.md
    rg -n '<deleted-convention-doc-paths-or-filenames>' AGENTS.md .agents docs/plans/repo-local-multi-agent-workflow.md
    git diff --check

Accept the change when all expected files are listed, stale-reference search reports no copied example references, removed-doc search reports no active references, and `git diff --check` reports no whitespace errors. Manually verify that reviewer skill parity holds: each reviewer loads the same worker skills as its matching worker and also loads `.agents/skills/cashregister-reviewer/SKILL.md`.

## Idempotence and Recovery

The workflow-tooling change is additive except for deliberate deletion of obsolete convention documents after migration. Re-running the structural `find` and `rg` checks is safe. If a role or skill file is missing, recreate only that file. If `docs/DIARY.md` gains duplicate entries while retrying, keep only the newest entry created by this task and do not alter unrelated existing diary content. Do not reset or rewrite pre-existing modifications to `docs/DIARY.md` or `docs/v2.MD`.

## Artifacts and Notes

Expected role files:

    .agents/agents/backend-reviewer.md
    .agents/agents/backend-worker.md
    .agents/agents/coordinator.md
    .agents/agents/merger.md
    .agents/agents/printmon-reviewer.md
    .agents/agents/printmon-worker.md
    .agents/agents/ui-reviewer.md
    .agents/agents/ui-worker.md

Expected skill files:

    .agents/skills/cashregister-agentic-feature/SKILL.md
    .agents/skills/cashregister-backend/SKILL.md
    .agents/skills/cashregister-general/SKILL.md
    .agents/skills/cashregister-integrator/SKILL.md
    .agents/skills/cashregister-printmon/SKILL.md
    .agents/skills/cashregister-reviewer/SKILL.md
    .agents/skills/cashregister-ui/SKILL.md
    .agents/skills/planner-agent/SKILL.md

Expected template files:

    .agents/workflows/templates/feature-state.md
    .agents/workflows/templates/integration-report.md
    .agents/workflows/templates/review-report.md
    .agents/workflows/templates/task-handoff.md

## Interfaces and Dependencies

This plan introduces no runtime interfaces or dependencies. The only new interface is a documentation convention: agents may read `.agents/agents/*.md`, `.agents/skills/*/SKILL.md`, and `.agents/workflows/templates/*.md` when coordinating significant parallel work. Durable runtime behavior remains in the existing canonical project documents.
