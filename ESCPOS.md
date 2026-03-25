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
│   ├── Formatting/
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
| ESC ! n         | `SelectPrintModeInstruction` | `Formatting/` | `UseFontA(FormatMode)` / `UseFontB(FormatMode)` | `0x1B 0x21 n` | `[PRINT_MODE:FONT_A,...]` |
| ESC - n         | `UnderlineInstruction`       | `Formatting/` | `UnderlineOn(Thickness)` / `UnderlineOff()` | `0x1B 0x2D n` | `[UNDERLINE:OFF]` / `[UNDERLINE:1DOT]` / `[UNDERLINE:2DOT]` |
| ESC E n         | `EmphasizeInstruction`       | `Formatting/` | `EmphasizeOn()` / `EmphasizeOff()` | `0x1B 0x45 n` | `[BOLD:ON]` / `[BOLD:OFF]` |
| ESC G n         | `DoubleStrikeInstruction`    | `Formatting/` | `DoubleStrikeOn()` / `DoubleStrikeOff()` | `0x1B 0x47 n` | `[DOUBLE_STRIKE:ON]` / `[DOUBLE_STRIKE:OFF]` |
| ESC M n         | `SelectFontInstruction`      | `Formatting/` | `SelectFontA()` / `SelectFontB()` | `0x1B 0x4D n` | `[FONT:A]` / `[FONT:B]` |
| ESC V n         | `RotationInstruction`        | `Formatting/` | `NinetyDegsOn()` / `NinetyDegsOff()` | `0x1B 0x56 n` | `[ROTATE_90:ON]` / `[ROTATE_90:OFF]` |
| ESC { n         | `UpsideDownInstruction`      | `Formatting/` | `UpsideDownOn()` / `UpsideDownOff()` | `0x1B 0x7B n` | `[UPSIDE_DOWN:ON]` / `[UPSIDE_DOWN:OFF]` |
| GS ! n          | `FontSizeInstruction`        | `Formatting/` | `FontSize(byte size)` | `0x1D 0x21 n` | `[FONT_SIZE:WxH]` |
| *(raw text)*    | `TextInstruction`            | `Core/`       | `Text(string text)` | ASCII bytes    | text as-is         |
| ESC a n         | `JustifyInstruction`         | `Layout/`     | `Justify(Justification)` | `0x1B 0x61 n` | `[ALIGN:LEFT]` / `[ALIGN:CENTER]` / `[ALIGN:RIGHT]` |
| ESC $ nL nH     | `AbsolutePositionInstruction`| `Layout/`     | `SetAbsolutePosition(ushort)` | `0x1B 0x24 nL nH` | `[ABS_POS:value]` |
| ESC \ nL nH     | `RelativePositionInstruction`| `Layout/`     | `SetRelativePosition(ushort)` | `0x1B 0x5C nL nH` | `[REL_POS:value]` |
| GS L nL nH      | `LeftMarginInstruction`      | `Layout/`     | `SetLeftMargin(ushort)` | `0x1D 0x4C nL nH` | `[LEFT_MARGIN:value]` |
| ESC t 0         | `SelectCodeTableInstruction` | `Core/`       | *(auto)*       | `0x1B 0x74 0x00` | `[CODE_TABLE:STD_EUROPE]` |
| HT              | `HorizontalTabInstruction`   | `Motion/`     | `HorizontalTab()` | `0x09` | `[HT]` |
| LF              | `LineFeedInstruction`        | `Core/`       | `LineFeed()` / `PrintLine(string)` | `0x0A` | `[LF]` |
| ESC m           | `PartialCutInstruction`      | `Core/`       | *(none — legacy)* | `0x1B 0x6D`    | `[CUT:PARTIAL]`   |
| GS V m n        | `CutAfterInstruction`        | `Core/`       | `CutAfter(byte)` | `0x1D 0x56 0x42 n` | `[CUT_AFTER:n]` |
| ESC 2            | `ResetLineSpacingInstruction`| `Core/`       | `ResetLineSpacing()` | `0x1B 0x32` | `[LINE_SPACING:DEFAULT]` |
| GS B n           | `ReverseInstruction`         | `Formatting/` | `ReverseOn()` / `ReverseOff()` | `0x1D 0x42 n` | `[REVERSE:ON]` / `[REVERSE:OFF]` |
| ESC SP n         | `RightSpacingInstruction`    | `Layout/`     | `SetRightSpacing(byte)` | `0x1B 0x20 n` | `[RIGHT_SPACING:n]` |
| ESC D n1..nk NUL | `SetHorizontalTabsInstruction` | `Motion/`   | `SetHorizontalTabs(params byte[])` / `ClearHorizontalTabs()` | `0x1B 0x44 ...positions 0x00` | `[SET_TABS:8,16,24]` / `[SET_TABS:CLEAR]` |
| ESC J n           | `FeedPaperInstruction`       | `Feed/`       | `FeedPaper(byte)` | `0x1B 0x4A n` | `[FEED_PAPER:n]` |
| ESC p m t1 t2     | `GeneratePulseInstruction`   | `Peripheral/` | `KickDrawer(ConnectorPin, byte, byte)` / `OpenCashDrawer()` | `0x1B 0x70 m t1 t2` | `[PULSE:PINx,ON=t1,OFF=t2]` |

**Before implementing a new instruction, check this table.** If the command is already listed, the work is done.
After implementing a new instruction, append a row to this table.

## Per-session contract

Each session implements exactly one instruction. Always implement relevant tests focusing on expected behavior rather than edge-cases. After completing, always run:

    dotnet build be/Cashregister.Printmon/Cashregister.Printmon.csproj
    dotnet test be/Cashregister.Printmon.Tests/Cashregister.Printmon.Tests.csproj

Fix any compiler errors and failing tests before considering the task done. Do not leave 
warnings unaddressed.