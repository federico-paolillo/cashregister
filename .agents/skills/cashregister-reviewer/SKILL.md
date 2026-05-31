---
name: cashregister-reviewer
description: Use for Cash Register reviewer agents checking source-of-truth compliance, correctness, security, tests, and integration risk.
---

# Cash Register Reviewer Skill

## Review Stance

Prioritize bugs, behavioral regressions, source-of-truth conflicts, missing tests, security issues, persistence risk, and public contract drift. Findings come first, ordered by severity.

## Skill Parity

A reviewer must load every implementation skill loaded by the matching worker, plus this reviewer skill.

- Backend reviewer: `cashregister-general`, `cashregister-backend`, `cashregister-reviewer`.
- Printmon reviewer: `cashregister-general`, `cashregister-backend`, `cashregister-printmon`, `cashregister-reviewer`.
- UI reviewer: `cashregister-general`, `cashregister-ui`, `cashregister-reviewer`.

## Checks

- Confirm the change matches canonical docs and the assigned handoff.
- Check public API, DTO, persistence, printer, deployment, and user-visible error behavior for undocumented drift.
- Check security boundaries, input handling, filesystem behavior, and configuration changes.
- Check concurrency and integration risk when multiple surfaces change.
- Check that tests cover the main success path and relevant failure paths.
- Check that validation commands were run or that skipped commands have a valid reason.

## Report Format

Use this order:

1. Findings, ordered by severity, with file and line references where applicable.
2. Open questions or assumptions.
3. Validation reviewed or run.
4. Approval status.

If there are no findings, state that directly and identify any remaining test gaps or residual risk.

## Waivers

Only the coordinator or user can waive a valid finding. A waiver must name the risk and why it is acceptable for this task.
