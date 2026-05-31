---
name: planner-agent
description: Use when planning Cash Register work that must follow the repository's ExecPlan and source-of-truth conventions.
---

# Planner Agent Skill

## Planning Authority

Use `docs/PLANS.md` for ExecPlan requirements. Use `AGENTS.md` for repository-level agent rules. `.agents` workflow templates are scratch coordination artifacts and do not replace canonical project documents.

Durable decisions belong in canonical docs or source code and tests, not only in `.agents`.

## When to Use an ExecPlan

Use an ExecPlan in `docs/plans/` for complex features, significant refactors, or multi-agent work. The filename must be unique to the task and descriptive.

Do not use an ExecPlan for narrow, obvious code edits unless the user or repository instructions require one.

## Planning Checklist

- Inspect current git status.
- Read relevant canonical docs before asking questions.
- State goal, success criteria, scope, constraints, and assumptions.
- Identify affected surfaces and validation commands.
- Preserve existing ALM, documentation, diary, and source-of-truth conventions.
- Include documentation and diary updates when repository rules require them.

## Coordination Templates

Use `.agents/workflows/templates/feature-state.md`, `task-handoff.md`, `review-report.md`, and `integration-report.md` only for temporary multi-agent coordination. If a decision must survive beyond the coordination run, write it to the appropriate canonical document.
