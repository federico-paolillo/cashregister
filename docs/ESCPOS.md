# Cashregister.Printmon — ESC/POS Printer Library

> This document provides development guidelines and project structure information for the Cash Register application ESC/POS integration.

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
│   ├── Layout/
│   ├── Feed/
│   ├── Motion/
│   ├── Cut/
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
- Represent devices as writable Linux printer file paths, not CUPS queues. The current printing implementation writes to `FileStream`, so a printable device choice must be a filesystem path such as `/dev/usb/lp0`, not a CUPS URI.

## Namespace

`Cashregister.Printmon`. All files use file-scoped namespace declaration.

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
| *(raw text)*    | `TextInstruction`            | `Core/`       | `Text(string text)` | ASCII bytes    | text as-is         |
| LF              | `LineFeedInstruction`        | `Core/`       | `LineFeed()` / `PrintLine(string)` | `0x0A` | `[LF]` |
| ESC ! 0         | `ResetPrintModeInstruction`  | `Formatting/` | *(auto)*       | `0x1B 0x21 0x00` | `[RESET_PRINT_MODE]` |
| ESC ! n         | `SelectPrintModeInstruction` | `Formatting/` | `UseFontA(FormatMode)` / `UseFontB(FormatMode)` | `0x1B 0x21 n` | `[PRINT_MODE:FONT_A,...]` |
| ESC - n         | `UnderlineInstruction`       | `Formatting/` | `UnderlineOn(Thickness)` / `UnderlineOff()` | `0x1B 0x2D n` | `[UNDERLINE:OFF]` / `[UNDERLINE:1DOT]` / `[UNDERLINE:2DOT]` |
| ESC E n         | `EmphasizeInstruction`       | `Formatting/` | `EmphasizeOn()` / `EmphasizeOff()` | `0x1B 0x45 n` | `[BOLD:ON]` / `[BOLD:OFF]` |
| ESC G n         | `DoubleStrikeInstruction`    | `Formatting/` | `DoubleStrikeOn()` / `DoubleStrikeOff()` | `0x1B 0x47 n` | `[DOUBLE_STRIKE:ON]` / `[DOUBLE_STRIKE:OFF]` |
| ESC M n         | `SelectFontInstruction`      | `Formatting/` | `SelectFontA()` / `SelectFontB()` | `0x1B 0x4D n` | `[FONT:A]` / `[FONT:B]` |
| ESC V n         | `RotationInstruction`        | `Formatting/` | `NinetyDegsOn()` / `NinetyDegsOff()` | `0x1B 0x56 n` | `[ROTATE_90:ON]` / `[ROTATE_90:OFF]` |
| ESC { n         | `UpsideDownInstruction`      | `Formatting/` | `UpsideDownOn()` / `UpsideDownOff()` | `0x1B 0x7B n` | `[UPSIDE_DOWN:ON]` / `[UPSIDE_DOWN:OFF]` |
| GS B n          | `ReverseInstruction`         | `Formatting/` | `ReverseOn()` / `ReverseOff()` | `0x1D 0x42 n` | `[REVERSE:ON]` / `[REVERSE:OFF]` |
| GS ! n          | `FontSizeInstruction`        | `Formatting/` | `FontSize(byte size)` | `0x1D 0x21 n` | `[FONT_SIZE:WxH]` |
| ESC a n         | `JustifyInstruction`         | `Layout/`     | `Justify(Justification)` | `0x1B 0x61 n` | `[ALIGN:LEFT]` / `[ALIGN:CENTER]` / `[ALIGN:RIGHT]` |
| ESC $ nL nH     | `AbsolutePositionInstruction`| `Layout/`     | `SetAbsolutePosition(ushort)` | `0x1B 0x24 nL nH` | `[ABS_POS:value]` |
| ESC \ nL nH     | `RelativePositionInstruction`| `Layout/`     | `SetRelativePosition(ushort)` | `0x1B 0x5C nL nH` | `[REL_POS:value]` |
| GS L nL nH      | `LeftMarginInstruction`      | `Layout/`     | `SetLeftMargin(ushort)` | `0x1D 0x4C nL nH` | `[LEFT_MARGIN:value]` |
| ESC SP n         | `RightSpacingInstruction`    | `Layout/`     | `SetRightSpacing(byte)` | `0x1B 0x20 n` | `[RIGHT_SPACING:n]` |
| GS W nL nH       | `PrintAreaWidthInstruction`  | `Layout/`     | `SetPrintAreaWidth(ushort)` | `0x1D 0x57 nL nH` | `[PRINT_WIDTH:n]` |
| ESC 2            | `ResetLineSpacingInstruction`| `Feed/`       | `ResetLineSpacing()` | `0x1B 0x32` | `[LINE_SPACING:DEFAULT]` |
| ESC 3 n          | `SetLineSpacingInstruction`  | `Feed/`       | `SetLineSpacing(byte)` | `0x1B 0x33 n` | `[LINE_SPACING:n]` |
| ESC d n          | `FeedLinesInstruction`       | `Feed/`       | `FeedLines(byte)` | `0x1B 0x64 n` | `[FEED_LINES:n]` |
| ESC J n          | `FeedPaperInstruction`       | `Feed/`       | `FeedPaper(byte)` | `0x1B 0x4A n` | `[FEED_PAPER:n]` |
| HT               | `HorizontalTabInstruction`   | `Motion/`     | `HorizontalTab()` | `0x09` | `[HT]` |
| ESC D n1..nk NUL | `SetHorizontalTabsInstruction` | `Motion/`   | `SetHorizontalTabs(params byte[])` / `ClearHorizontalTabs()` | `0x1B 0x44 ...positions 0x00` | `[SET_TABS:8,16,24]` / `[SET_TABS:CLEAR]` |
| ESC m            | `PartialCutInstruction`      | `Cut/`        | *(none — legacy)* | `0x1B 0x6D`    | `[CUT:PARTIAL]`   |
| GS V m n         | `CutAfterInstruction`        | `Cut/`        | `CutAfter(byte)` | `0x1D 0x56 0x42 n` | `[CUT_AFTER:n]` |
| ESC i            | `HalfCutInstruction`         | `Cut/`        | `HalfCut()` | `0x1B 0x69` | `[CUT:HALF]` |
| GS V 1           | `CutInstruction`             | `Cut/`        | `Cut()` | `0x1D 0x56 0x01` | `[CUT:PARTIAL_GS]` |
| ESC t n          | `SelectCodePageInstruction`  | `CodePage/`   | `SelectCodePage(CharacterCodePage)` | `0x1B 0x74 n` | `[CODE_PAGE:name]` |
| ESC p m t1 t2    | `GeneratePulseInstruction`   | `Peripheral/` | `KickDrawer(ConnectorPin, byte, byte)` / `OpenCashDrawer()` | `0x1B 0x70 m t1 t2` | `[PULSE:PINx,ON=t1,OFF=t2]` |
| DLE DC4 1 m t    | `RealTimePulseInstruction`   | `Peripheral/` | `RealTimePulse(ConnectorPin, byte)` | `0x10 0x14 0x01 m t` | `[RT_PULSE:PINx,t=n]` |

**Before implementing a new instruction, check this table.** If the command is already listed, the work is done.
After implementing a new instruction, append a row to this table.

## Emulator

### Project layout

```
Cashregister.Printmon.Emulator/
├── PrinterState.cs        ← immutable printer configuration snapshot
├── Receipt.cs             ← IDocumentElement hierarchy + TextStyle + Receipt
├── Printer.cs             ← state+receipt pair (time-travel step)
├── InstructionDecoder.cs  ← IInstructionDecoder + InstructionDecoder
├── InstructionExecutor.cs ← IInstructionExecutor + InstructionExecutor
├── PrinterEmulator.cs     ← IPrinterEmulator + PrinterEmulator
└── MarkdownRenderer.cs    ← IMarkdownRenderer + MarkdownRenderer
Cashregister.Printmon.Emulator.Tests/
```

### Architecture

The emulator is a pipeline:

```
byte[] ──► InstructionDecoder ──► Instruction[]
                                       │
                              InstructionExecutor (per step)
                                       │
                              Printer (state + receipt)
                                       │
                              PrinterEmulator (orchestrator)
                                       │
                              ImmutableArray<Printer> ──► history[^1].Receipt ──► MarkdownRenderer ──► string
```

- `InstructionDecoder` is the inverse of `BinaryEncoder`: same byte sequences, decoded back to instruction records.
- `InstructionExecutor` is a pure function: `Printer Execute(Printer printer, Instruction instruction)`. Each call takes the full current `Printer` and returns a new one — new elements are appended to the receipt, state changes are applied. Each execution reads like `printer = printer + instruction`.
- `PrinterEmulator` wires the pipeline together. `Emulate` always returns the full trace — one `Printer` per instruction — enabling time-travel inspection. The final receipt is `history[^1].Receipt`.
- `MarkdownRenderer` maps a `Receipt` to Markdown text.

### PrinterState

An immutable `sealed record` with init-only properties representing the printer's configuration at any point during execution. All fields mirror the power-on defaults from the printer manual.

| Property | Type | Default |
|---|---|---|
| `Bold` | `bool` | `false` |
| `Underline` | `Thickness` | `None` |
| `DoubleStrike` | `bool` | `false` |
| `Font` | `CharacterFont` | `A` |
| `Rotation` | `bool` | `false` |
| `UpsideDown` | `bool` | `false` |
| `Reverse` | `bool` | `false` |
| `WidthMultiplier` | `byte` | `1` |
| `HeightMultiplier` | `byte` | `1` |
| `Justification` | `Alignment` | `Left` |
| `LeftMargin` | `ushort` | `0` |
| `PrintAreaWidth` | `ushort` | `0` |
| `RightSpacing` | `byte` | `0` |
| `LineSpacing` | `byte` | `30` |
| `CodePage` | `CharacterCodePage` | `OEM437` |
| `TabPositions` | `ImmutableArray<byte>` | `[9,17,25,33,41,49,57]` |

`PrinterState.Default` is the static singleton representing these defaults. `InitializeInstruction` always resets to it.

### Receipt element hierarchy

```
abstract record IDocumentElement
  sealed record TextSpan(string Text, TextStyle Style)
  sealed record LineBreak
  sealed record FeedLines(byte Count)
  sealed record HorizontalRule              ← emitted by all cut instructions
  sealed record Barcode                     ← placeholder
  sealed record QrCode                      ← placeholder
  sealed record Image                       ← placeholder
```

`TextStyle` is a formatting snapshot taken at the moment a `TextInstruction` is executed. It carries: `Bold`, `Underline`, `DoubleStrike`, `Font`, `Rotation`, `UpsideDown`, `Reverse`, `WidthMultiplier`, `HeightMultiplier`, `Justification`.

`Receipt(ImmutableArray<IDocumentElement> Elements)` is the accumulated output of all executed instructions. Renderers consume a `Receipt` directly.

`Printer(PrinterState State, Receipt Receipt)` is the emulator's representation of the hardware: `State` is the current printer configuration, `Receipt` is the paper being printed. Each instruction produces a new `Printer` where state may change and new elements may be appended to the receipt. The time-travel history is `ImmutableArray<Printer>` — one entry per instruction. `Printer.Default` is the initial power-on state with an empty receipt.

### Decoder scope

`InstructionDecoder` handles exactly the commands that `BinaryEncoder` encodes. Instructions without a builder method cannot be round-trip tested and are excluded:

- `PartialCutInstruction` (`ESC m`, `0x1B 0x6D`) is encoded by `BinaryEncoder` but has no builder method. The decoder throws on this byte sequence.
- `NoOpInstruction` encodes to zero bytes and is never produced by the decoder.

Fail-fast rule: any unrecognized byte or truncated sequence throws `InvalidOperationException` with the byte offset and value.

ESC ! disambiguation:
- `0x1B 0x21 0x00` → `ResetPrintModeInstruction`
- `0x1B 0x21 n` (n ≠ 0) → `SelectPrintModeInstruction(fontB: (n & 0x01) != 0, flags: (FormatMode)(n & ~0x01))`

GS V disambiguation:
- `0x1D 0x56 0x01` → `CutInstruction`
- `0x1D 0x56 0x42 n` → `CutAfterInstruction(n)`

### Executor behavior

State-modifying instructions update `PrinterState` and append no elements. Content-emitting instructions leave state unchanged and append one or more elements to the receipt:

| Instruction | Element emitted |
|---|---|
| `TextInstruction` | `TextSpan(text, StyleFrom(state))` |
| `LineFeedInstruction` | `LineBreak` |
| `HorizontalTabInstruction` | `TextSpan("\t", StyleFrom(state))` |
| `FeedLinesInstruction` | `FeedLines(count)` |
| `FeedPaperInstruction` | `FeedLines(1)` |
| `CutAfterInstruction` / `CutInstruction` / `HalfCutInstruction` / `PartialCutInstruction` | `HorizontalRule` |

`SelectPrintModeInstruction` sets `Font`, `Bold`, `WidthMultiplier`, `HeightMultiplier`, and `Underline` simultaneously from its `Flags` bitmask. Peripheral instructions (`GeneratePulseInstruction`, `RealTimePulseInstruction`) are no-ops: no state change, no elements.

### Markdown rendering rules

| Element | Markdown |
|---|---|
| `TextSpan` bold | `**text**` |
| `TextSpan` underline | `<u>text</u>` |
| `TextSpan` reverse | `` `text` `` |
| `TextSpan` height ≥ 2 | `## text` |
| `TextSpan` center-justified | `<p align="center">text</p>` |
| `TextSpan` right-justified | `<p align="right">text</p>` |
| `LineBreak` | newline |
| `FeedLines(n)` | n blank lines |
| `HorizontalRule` | `---` |
| `Barcode` / `QrCode` / `Image` | `[BARCODE]` / `[QR]` / `[IMAGE]` placeholders |

Formatting is applied inside-out: reverse → underline → bold/double-strike. Alignment wraps the result last.

### CLI usage

```bash
dotnet run --project be/Cashregister.Cli -- emulate --input receipt.bin
```

Reads a raw ESC/POS binary file, decodes it, executes the instructions, and writes Markdown to stdout.