# Cash Register 

> The most simplestest cash register application. It _does not_ track money, just orders.

## Mindset

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## General

- **Be idiomatic and consistent at all costs !**
- Backend code might feel over-engineered, complex and "enterprise-grade". That is **on purpose**. 
- Backend code is a style exercise in enterprise applications. Follow same style.
- Keep changes scoped to the task. Do not refactor adjacent code unless the requested change requires it.
- Prefer the existing project style over introducing a new local pattern.
- Use conventional commits when committing, for example `feat:`, `fix:`, `docs:`, `test:`, or `chore:`.
- Avoid repetitions. If you see or make a code block twice, refactor.

## Documentation

`docs/` folder has all the documentation, context and information you will need.

### Must reads

- docs/ARCH.md - Main architecture and design choices. Read this to get your bearings.
- docs/ESCPOS.md - Architecture and design choices for the ESC/POS implementation. Read this to understand how printing works.
- docs/CONVENTIONS.md - How to work on the project. This is fundamental

### Optional reads

- docs/DIARY.md - Log of implementation decisions and choices. Read it if you need to understand how the code evolved.
- docs/PLANS.md - "ExecPlan" definition and template. Use it when generating an implementation plan.
- docs/MANUAL.md - Manual of the target printer we use when printing receipts. Read this if you need to know something about the printer.
- docs/PRINTER.md - List of all ESC/POS commmands supported by the target printer. Reader this if you need to review how a command works.

`plans/` folder has all the past implementation plans. You can ignore this folder completely. It's just to have some history.

### Ignore

- README.md - It's barren. Save tokens and skip it.
- docs/REVIEW.md - Past reviews findings. Ignore this as it is outdated and needs human review.

## Verification

When you finish a task always run verification steps. Verification steps are described in `docs/ARCH.md` under section "Operational Notes". If any verification step reports errors or failures you **must** address them before considering the task done.

## Bookkeeping

When writing complex features or significant refactors, use an "ExecPlan" (as described in `docs/PLANS.md`) from design to implementation. Emit the "ExecPlan" in `docs/plans/` folder.

When you complete a task you have to document in docs/DIARY.md implementation decisions, design choices, and strategies. The goal is to avoid re-deriving the same conclusions when picking up work later and keep you consistent. There is no need to list changed files.

Follow this format for an entry of the diary:

```markdown
## Short task description

More detailed description (max. 200 words)

### ExecPlan

`<path_to_exec_plan>`

### Key decisions

- We did this because of that. We did not do something else because of another reason.
```

Include an `ExecPlan` section with `path_to_exec_plan` relative to `docs/` folder only if you used an ExecPlan to develop. If you did not use an ExecPlan skip this section.

If you make changes that alter what is referenced in `ARCH.md` or `ESCPOS.md` you have to update those files as well. `ARCH.md` and `ESCPOS.md` must not get out of sync. There is no need to document every tiny little detail in `ARCH.md` or `ESCPOS.md` as they document the high-level architecture of Cashregister.

`CONVENTIONS.md` must be updated if you introduce new development conventions or techniques.
