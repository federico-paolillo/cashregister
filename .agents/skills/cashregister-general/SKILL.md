---
name: cashregister-general
description: Use for all Cash Register development work to preserve source-of-truth discipline, scope control, validation, and repository-wide conventions.
---

# Cash Register General Skill

## Source of Truth

Treat `AGENTS.md`, `docs/ARCH.md`, `docs/ESCPOS.md`, `docs/PLANS.md`, and `docs/DIARY.md` as canonical project documents. `.agents` is non-canonical for product behavior and runtime contracts. Agent workflow and implementation-practice guidance lives in `.agents/agents/`, `.agents/skills/`, and `.agents/workflows/templates/`.

If a durable product, deployment, security, persistence, public-interface, or rebuild-relevant decision is missing or ambiguous, stop and escalate instead of placing the decision only in `.agents`.

## Development Practice

- Keep changes surgical and tied to the assigned task.
- Prefer existing project style over new local patterns.
- Do not add speculative abstractions, configuration, or dependencies.
- Preserve explicit module boundaries between backend, Printmon, and UI unless the assignment requires a cross-surface change.
- Update canonical docs when runtime architecture, ESC/POS behavior, or bookkeeping rules change.
- Update the relevant `.agents/skills/*/SKILL.md` file when agent-facing implementation practice changes.
- Append to `docs/DIARY.md` at task completion when implementation changes are made.

## Security and Configuration

- Do not commit secrets.
- Keep environment-variable and deployment behavior aligned with canonical docs.
- Do not introduce new network, filesystem, authentication, or persistence behavior without source-of-truth coverage.

## DevOps Practice

- Prefer hardened Docker runtime images: official distroless or chiseled bases when the application can run on them.
- Run runtime containers as a non-root user.
- Keep application files read-only at runtime.
- Put writable application state, caches, and logs in explicit writable directories separate from application binaries.
- Use multi-stage builds so SDKs, compilers, package managers, and build-only tools do not ship in runtime images.
- Prefer BuildKit cache mounts for dependency restoration and other repeatable expensive build steps.
- Use `#!/usr/bin/env sh` for shell scripts unless Bash-specific behavior is required.
- Use `set -e` so scripts stop after failed commands.
- Use `set -u` so unset variables are treated as errors.
- Keep scripts focused on one operational task.

## Validation

Use validation commands from `docs/ARCH.md` and the relevant implementation skill.

For documentation-only changes, run:

    git diff --check

Do not run formatters that rewrite code for documentation-only changes. Full backend or frontend verification is optional confidence checking unless source code changed. If optional code verification fails without source edits, report it as existing repository state rather than broadening the task into a code fix.

For backend source changes, run from `be/`:

    dotnet format
    dotnet build
    dotnet test

For frontend source changes, run from `ui/`:

    npm run lint
    npm run build
    npm run test

## Missing Decisions

Record assumptions in handoffs and final reports. Escalate decisions that affect public contracts, persistence, printer behavior, deployment, security, user-visible errors, or artifact layout.
