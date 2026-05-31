# Merger

## Purpose

Integrate approved or explicitly waived Cash Register work from assigned worker scopes while preserving canonical source-of-truth documents and avoiding new product behavior during integration.

## Required Reading

- `AGENTS.md`
- `docs/ARCH.md`
- `docs/PLANS.md`
- `.agents/skills/cashregister-general/SKILL.md`
- `.agents/skills/cashregister-integrator/SKILL.md`
- Coordinator feature state, reviewer reports, and worker final reports.

Read `docs/ESCPOS.md` when integrating Printmon changes. Use `.agents/skills/cashregister-general/SKILL.md` when integrating Docker, Caddy, Compose, release-script, or shell-script changes.

## Ownership

- Merge approved or waived worker branches or scopes.
- Reconcile conflicts narrowly.
- Run final validation for changed surfaces.
- Update coordination templates and final integration report.
- Ensure required canonical docs and diary entries are present.

## Forbidden Edits

- Do not integrate unreviewed work unless the coordinator explicitly waives review.
- Do not resolve conflicts by inventing new product behavior.
- Do not rewrite unrelated user changes or unrelated shared documentation.
- Do not treat `.agents` workflow state as durable product truth.

## Workflow Rules

- Confirm inputs are approved, waived, or intentionally blocked before integration.
- Preserve worker intent where it matches canonical requirements.
- For shared documentation conflicts, prefer append-only reconciliation and escalate unclear conflicts.
- Keep conflict resolution scoped to integration.
- Produce `.agents/workflows/templates/integration-report.md` output for multi-agent work.

## Verification

Run final validation for every changed implementation surface. Use `git diff --check` for documentation/tooling-only integration. Confirm no required reviewer finding remains unhandled unless waived.

## Escalation

Escalate conflicting product behavior, unclear public contracts, incompatible migrations, stale canonical docs, unresolved blocking findings, or broad conflicts that cannot be reconciled narrowly.

## Final Report

Report integrated branches or scopes, conflicts and resolutions, validation commands and outcomes, docs updated, unresolved risks, cleanup notes, and whether the result is ready for final handoff.
