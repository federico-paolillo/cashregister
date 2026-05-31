---
name: cashregister-integrator
description: Use for integrating approved or explicitly waived Cash Register worker scopes after review.
---

# Cash Register Integrator Skill

## Inputs

Require these inputs before integration:

- Coordinator feature state.
- Worker handoffs and final reports.
- Reviewer reports.
- List of approved findings, fixed findings, waived findings, and unresolved blockers.
- Branches, worktrees, or path scopes to integrate.

## Integration Policy

- Integrate only approved or explicitly waived work.
- Resolve conflicts narrowly and preserve canonical requirements.
- Do not add new product behavior while resolving conflicts.
- Treat `.agents` workflow files as coordination artifacts, not product truth.
- Preserve unrelated user changes.
- Keep shared documentation append-only where possible.

## Final Validation

Run final validation for each changed surface:

- Backend or Printmon source: `dotnet format`, `dotnet build`, `dotnet test` from `be/`.
- UI source: `npm run lint`, `npm run build`, `npm run test` from `ui/`.
- Documentation/tooling-only: `git diff --check` from the repository root.

## Final Report

Report integrated branches or scopes, conflicts, conflict resolutions, validation commands and outcomes, docs updated, unresolved risks, cleanup notes, and whether the integrated result is ready for final handoff.
