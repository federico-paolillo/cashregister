---
name: cashregister-printmon
description: Use for Cash Register Printmon, ESC/POS, receipt emulator, printer-device, and CLI work.
---

# Cash Register Printmon Skill

## Owned Paths

Printmon work normally lives under:

- `be/Cashregister.Printmon/`
- `be/Cashregister.Printmon.Emulator/`
- `be/Cashregister.Printmon.Tests/`
- `be/Cashregister.Printmon.Emulator.Tests/`
- `be/Cashregister.Cli/`
- Print-related API or application adapters only when assigned.

Command-level source of truth is `docs/ESCPOS.md`. High-level receipt printing architecture is in `docs/ARCH.md`.

## Conventions

- Represent print output as immutable `PrintProgram` instances containing `Instruction` records.
- Use `PrintProgramBuilder` for application-level program construction.
- Keep encoder mappings in encoders, not instruction records.
- Keep `IEncoder<TOutput>` synchronous.
- Preserve deterministic string encoder tokens and documented binary byte mappings.
- Keep the emulator pipeline pure until device output.
- Keep file-device behavior aligned with documented selected-target semantics.

## ESC/POS Changes

Before adding or changing an instruction, check the implemented-instructions table in `docs/ESCPOS.md`. If a command already exists, extend tests or builder APIs instead of adding duplicate instruction records.

When a command changes, update the relevant record, builder, binary encoder, string encoder, decoder, executor, tests, and `docs/ESCPOS.md` together unless the task explicitly scopes one part only and records why.

## Testing

- Use Printmon tests for builder and encoder behavior.
- Use emulator tests for decoder, executor, printer emulator, markdown rendering, and markdown device behavior.
- Keep receipt behavior observable through tests or emulator output.

## Validation

For Printmon source changes, run from `be/`:

    dotnet format
    dotnet build
    dotnet test

For documentation-only Printmon coordination changes, run `git diff --check` from the repository root.

## Common Pitfalls

- Do not bypass the builder in application code.
- Do not add command mnemonics or byte mappings to instruction records.
- Do not update Printmon behavior without keeping `docs/ESCPOS.md` synchronized.
- Do not assume CUPS queue URIs are valid file-device targets.
