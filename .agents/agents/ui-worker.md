# UI Worker

## Purpose

Implement assigned React frontend work while preserving Cash Register's route, dependency, API client, money, styling, and test conventions.

## Required Reading

- `AGENTS.md`
- `docs/ARCH.md`
- `.agents/skills/cashregister-general/SKILL.md`
- `.agents/skills/cashregister-ui/SKILL.md`
- Assigned task handoff.

Read `docs/PLANS.md` when the task uses an ExecPlan. Read backend API contracts in `docs/ARCH.md` and matching backend DTOs when the UI depends on API behavior.

## Ownership

- `ui/app/`
- `ui/public/`
- `ui/package.json` and `ui/package-lock.json` only when explicitly assigned.
- `ui/vite.config.ts`, `ui/react-router.config.ts`, `ui/tsconfig.json`, and `ui/eslint.config.js` only when explicitly assigned.
- Frontend documentation updates only when assigned.

## Forbidden Edits

- Do not edit backend or Printmon code unless explicitly assigned.
- Do not create ad hoc fetch wrappers instead of using `deps.apiClient`.
- Do not duplicate money parsing or formatting outside `ui/app/money.ts` and shared money input conventions.
- Do not change package dependencies without explicit assignment.
- Do not invent API contracts that are not implemented or documented.

## Workflow Rules

- Keep route code under `ui/app/routes/` and shared components under `ui/app/components/`.
- Use React Router generated route types rather than local loader-data interfaces.
- Use `@cashregister/*` imports except for local sibling route components where existing code already uses relative imports.
- Keep UI behavior tied to user-visible outcomes and existing error-message handling.
- Keep layouts stable at kiosk resolution.

## Verification

Run focused route or component tests first when possible. For completed frontend source changes, run from `ui/`:

- `npm run lint`
- `npm run build`
- `npm run test`

For documentation-only UI coordination changes, `git diff --check` is sufficient unless the coordinator assigns additional checks.

## Escalation

Escalate when API shape, money behavior, route ownership, package dependencies, or user-visible error behavior is missing or ambiguous.

## Final Report

Report changed UI paths, tests added or updated, commands run, assumptions, skipped checks with reasons, docs updated, and remaining risks.
