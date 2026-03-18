# Cashregister.Printmon — ESC/POS Printer Library

## Project layout

```
Cashregister.Printmon/
├── Devices/
│   └── IDevice.cs
├── Encoders/
│   ├── IEncoder.cs
│   ├── BinaryEncoder.cs       ← grows each session
│   └── StringEncoder.cs       ← grows each session
├── Instructions/
│   ├── Instruction.cs         ← abstract record base, do not modify
│   ├── Core/
│   ├── Text/
│   ├── Feed/
│   ├── Cut/
│   ├── Barcode/
│   ├── CodePage/
│   └── Peripheral/
├── PrintProgram.cs
└── PrintProgramBuilder.cs     ← grows each session
Cashregister.Printmon.Tests/
```

## Architecture invariants — never violate these

- All instruction types are `sealed record` inheriting `abstract record Instruction`.
- `Instruction` has no properties. No `Mnemonic`, no display data. Encoders own all mapping.
- `PrintProgramBuilder.Add(Instruction)` is the only method that mutates state.
  It enforces the frozen guard and null check. Every public builder method must
  delegate exclusively to `Add()` as a one-liner expression body.
- `IEncoder<TOutput>.Encode(PrintProgram)` is synchronous. Encoding is a pure 
  in-memory transformation. Never make it async.
- `IDevice.PrintAsync` is the only async surface. It performs actual I/O.
- Byte literals in BinaryEncoder must use 0xHH hex notation with an inline comment
  citing the ESC/POS mnemonic, e.g.: `0x1B, 0x40 // ESC @`
- StringEncoder tokens follow the convention:
    - Flag commands:   [BOLD:ON] / [BOLD:OFF]
    - Valued commands: [ALIGN:CENTER], [CUT:FULL]
    - Text commands:   emit text as-is; PrintLine appends \n
    - Parameterless:   [INIT], [NOP]

## Namespace

Cashregister.Printmon
All files use file-scoped namespace declaration.

## Target

.NET 10, C# 14. No external dependencies. Must compile with `dotnet build`.

## Encoders — structure

Both BinaryEncoder and StringEncoder use an exhaustive switch expression 
over the instruction list:
```csharp
foreach (var instruction in program.Instructions)
{
    switch (instruction)
    {
        case NoOpInstruction:
            // ...
            break;
        // ... one case per instruction type
        default:
            throw new NotSupportedException(
                $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
    }
}
```

The default throw arm must always be present and must always be last.
When adding a new instruction, insert the new case before the default arm.

## Implemented instructions

| ESC/POS command | Instruction record       | Category folder | Builder method | Binary encoding  | String encoding |
|-----------------|--------------------------|-----------------|----------------|------------------|-----------------|
| *(internal)*    | `NoOpInstruction`        | `Core/`         | `NoOp()`       | *(no bytes)*     | `[NOP]`         |
| ESC @           | `InitializeInstruction`  | `Core/`         | *(auto)*       | `0x1B 0x40`      | `[INIT]`        |

**Before implementing a new instruction, check this table.** If the command is already listed, the work is done.
After implementing a new instruction, append a row to this table.

## Per-session contract

Each session implements exactly one instruction. Always implement relevant tests focusing on expected behavior rather than edge-cases. After completing, always run:

    dotnet build Cashregister.Printmon/Cashregister.Printmon.csproj
    dotnet test Cashregister.Tests.Integration/Cashregister.Tests.Integration.csproj

Fix any compiler errors and failing tests before considering the task done. Do not leave 
warnings unaddressed.