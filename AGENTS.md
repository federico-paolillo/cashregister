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

## In general

- **Be idiomatic and consistent at all costs !**
- Backend code might feel over-engineered, complex and "enterprise-grade". That is **on purpose**. 
- Backend code is a style exercise in enterprise applications. Follow same style.
- Follow conventional commit format (e.g., `feat:`, `fix:`, `chore:`)

## When you plan

- Make sure your plan output does not contradict `docs/ARCH.md` and `docs/ESCPOS.md`.
- Do not assume. If you are not sure about something ask me.
- Use an "ExecPlan" (as described in `docs/PLANS.md`) from design to implementation. 
- Emit the "ExecPlan" in `docs/plans/` folder.

## When you develop

- You are a senior software engineer with a ridicolous expertise in .NET and React. Act like one.
- **Tests are mandatory**. Do not try to reach for 100% coverage but make sure you always cover at least the happy path.
- Make lightweight interfaces to aid testing. Do not try to anticipate greater abstractions unless necessary.
- Do follow SOLID principles and GRASP principles but do not forget about KISS and YAGNI.
- Do not take shortcuts or make stub implementations. If you find something difficult to implement challenge the design.
- Leave a `/// <summary>` comment on classes and interfaces stating their purpose.
- Remember we have `<ImplicitUsings>` enabled in `Directory.Build.props`. Don't add unecessary usings.
- `<>` not `<Fragment>` in React
- Backend default implementations of interfaces go under a `Defaults/` folder of the parent folder.

## Verification step

Always run these commands in sequence to ensure that your code is acceptable. 

If any of these commands report errors you **must** address them.

### For backend

In `be/` folder

1. `dotnet format`
2. `dotnet build`
3. `dotnet test`

### For frontend

In `ui/` folder

1. `npm run lint`
2. `npm run build`
3. `npm run test`

## Bookkeeping

When writing complex features or significant refactors, use an "ExecPlan" (as described in `docs/PLANS.md`) from design to implementation. Emit the "ExecPlan" in `docs/plans/` folder.

When you complete a task you have to document in docs/DIARY.md implementation decisions, design choices, and strategies. The goal is to avoid re-deriving the same conclusions when picking up work later and keep you consistent. There is no need to list changed files.

Follow this format for an entry of the diary:

```markdown
## Short task description

More detailed description (max. 200 words)

### Key decisions

- We did this because of that. We did not do something else because of another reason.
```

## System requirements

The backend and frontend will run on an Arduino Uno Q. The UI will be rendered using embedded brower [WPE](https://wpewebkit.org/) via Wayland-powered kiosk compositor [Cage](https://github.com/cage-kiosk/cage).

Arduino Uno Q has following specifications:

- Microprocessor (MPU): Qualcomm Dragonwing™ QRB2210: Quad-core Arm® Cortex®-A53 @ 2.0 GHz; Adreno GPU 3D graphics accelerator; 2x ISP (13 MP + 13 MP or 25 MP) @ 30 fps
- Microcontroller (MCU): STM32U585 Arm® Cortex®-M33 up to 160 MHz; 2 MB flash memory; 786 KB SRAM
- RAM: 4 GB LPDDR4
- Power Supply: 32 GB eMMC built-in (no SD card required)
- Storage: 32GB eMMC
- USB: 1x USB-C port with host/device role switching, power role switch and video output 
- Connectivity: Wi-Fi® 5 2.4/5GHz with onboard antenna; Bluetooth® 5.1 with onboard antenna
- MPU Operating System: Linux Debian OS with upstream support
- Real-time Operating System: Arduino Core on Zephyr OS

Keep these system capabilities in mind when you suggest or plan an implementation.

## Further references

`docs/` folder has all the documentation, context and information. In particular:

- docs/DIARY.md - Log of implementation decisions and choices. Read it if you need to understand how the code evolved.
- docs/PLANS.md - "ExecPlan" definition. Use it when generating an implementation plan.
- docs/README.md - It's barren. Save tokens and skip it.
- docs/ARCH.md - Main architecture and design choices. Read this understand how to work on the project
- docs/ESCPOS.md - Architecture and design choices for the ESC/POS implementation. Read this to understand how printing works.
- docs/MANUAL.md - Manual of the target printer we use when printing receipts. Read this if you need to know something about the printer.
- docs/PRINTER.md - List of all ESC/POS commmands supported by the target printer. Reader this if you need to review how a command works.
- docs/REVIEW.md - Past reviews findings. You might want to review them to understand why some code looks funny.

`plans/` folder has all the past implementation plans. You can ignore this folder completely. It's just to have some history.
