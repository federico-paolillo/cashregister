---
name: cashregister-agentic-feature
description: Use when coordinating a significant Cash Register feature across multiple repo-local agents.
---

# Cash Register Agentic Feature Skill

## Coordinator Loop

Use this loop for significant parallel feature work:

1. Inspect git status and relevant canonical docs.
2. Identify the feature, affected modules, durable surfaces, and acceptance criteria.
3. Run a clarification gate before worker dispatch when durable behavior is missing.
4. Select only the needed roles.
5. Assign disjoint write scopes and explicit read-only scopes.
6. Create worker handoffs using `.agents/workflows/templates/task-handoff.md`.
7. Dispatch workers.
8. Collect worker reports and changed-scope summaries.
9. Dispatch matching reviewers.
10. Route reviewer findings back to workers.
11. Track fixed, waived, approved, and blocked findings.
12. Merge only approved or explicitly waived work.
13. Run final validation for changed surfaces.
14. Update canonical source-of-truth, task, status, and diary files only where repository rules require it.
15. Produce final report and cleanup notes.

## Source-of-Truth Discipline

Use `.agents/workflows/templates/feature-state.md` for coordination state. Do not treat it as product truth. Durable behavior belongs in `AGENTS.md`, `docs/ARCH.md`, `docs/ESCPOS.md`, `docs/PLANS.md`, `docs/DIARY.md`, active ExecPlans, or the relevant source code and tests. Agent-facing implementation practice belongs in `.agents/skills/*/SKILL.md`.

## Role Selection

Use backend roles for domain, application, database, API, activities, and integration tests. Use Printmon roles for ESC/POS, printer model, encoders, emulator, file devices, receipt printing internals, and CLI tools. Use UI roles for React routes, components, API client behavior, styling, and frontend tests.

Do not create extra workers for small changes that a single agent can safely complete.

## Handoff Requirements

Each handoff must name:

- Source-of-truth context.
- Assigned write scope.
- Read-only scope.
- Required skills.
- Acceptance criteria.
- Validation commands.
- Escalation triggers.
