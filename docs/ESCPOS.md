# Cashregister.Printmon ESC/POS Reference

> This document is the source of truth for `Cashregister.Printmon.*`, the printer emulator, and the Printmon CLI. Keep command-level ESC/POS details here, not in `ARCH.md`.

## Project Layout

```text
be/Cashregister.Printmon/
├── Devices/
│   ├── IDevice.cs
│   ├── FileDevice.cs
│   ├── FileDeviceSettings.cs
│   └── FileDeviceTargetStore.cs
├── Encoders/
│   ├── IEncoder.cs
│   ├── BinaryEncoder.cs
│   └── StringEncoder.cs
├── Instructions/
│   ├── Instruction.cs
│   ├── CodePage/
│   ├── Core/
│   ├── Cut/
│   ├── Feed/
│   ├── Formatting/
│   ├── Layout/
│   ├── Motion/
│   └── Peripheral/
├── PrintProgram.cs
└── PrintProgramBuilder.cs

be/Cashregister.Printmon.Emulator/
├── InstructionDecoder.cs
├── InstructionExecutor.cs
├── MarkdownRenderer.cs
├── Printer.cs
├── PrinterEmulator.cs
├── PrinterState.cs
├── Receipt.cs
└── Problems/

be/Cashregister.Cli/
└── Tools/

be/Cashregister.Printmon.Tests/
be/Cashregister.Printmon.Emulator.Tests/
```

The API device-selection code that enumerates Linux printer file devices lives in `be/Cashregister.Api/Devices`, because it is an HTTP-facing adapter around the Printmon file-device model.

## Core Model

`PrintProgram` is an immutable list of `Instruction` records:

```csharp
public sealed record PrintProgram(ImmutableArray<Instruction> Instructions);
```

All printer commands are represented as sealed records inheriting the property-free base record:

```csharp
public abstract record Instruction;
```

Instruction records carry command parameters only. They do not carry mnemonics, display strings, or encoded bytes. Encoders own all mappings from instruction records to concrete output formats.

## Builder Invariants

`PrintProgramBuilder` is the intended way to construct a valid program.

- The builder is single-use. Calling `Build()` freezes it.
- Construction starts every program with `InitializeInstruction`, `SelectCodePageInstruction(CharacterCodePage.OEM437)`, and `ResetPrintModeInstruction`.
- `Build()` appends a final `LineFeedInstruction` and `CutAfterInstruction(1)`, then returns `PrintProgram`.
- All state mutation goes through private `AddInstruction(Instruction)`, which checks for null and rejects mutation after freeze.
- Public builder methods may validate inputs or convert user-friendly units before calling `AddInstruction`.
- Millimeter-based methods convert to printer units of 0.125 mm and currently reject values greater than 31.875 mm.
- TimeSpan-based pulse helpers convert to the ESC/POS units required by the instruction records.

Do not bypass the builder in application code unless a test is explicitly asserting encoder, decoder, or emulator behavior for a specific instruction.

## Devices

`IDevice.PrintAsync(PrintProgram)` is the async I/O boundary. Encoding is synchronous; device output is async.

`FileDevice` depends on `FileDeviceTargetStore` and `IEncoder<byte[]>`. It encodes a program and writes the bytes to the current target with `FileStream`. The target must be an existing writable filesystem path such as `/dev/usb/lp0`; CUPS queue URIs are not valid targets for this implementation.

`FileDeviceSettings.Section` is `FileDevice`, and `FileDeviceSettings.Target` is the startup target. `FileDeviceTargetStore` stores the active target in memory and can be changed at runtime.

The API device catalog enumerates writable `/dev/usb/lp*` and `/dev/lp*` paths, derives URL-safe ids from target paths, and rejects selection ids that are not in the current catalog.

## Encoders

`IEncoder<TOutput>` is synchronous:

```csharp
public interface IEncoder<TOutput>
{
    Result<TOutput> Encode(PrintProgram printProgram);
}
```

`BinaryEncoder` writes ESC/POS bytes to an in-memory stream. Byte literals must use `0xHH` notation and comments should cite the ESC/POS mnemonic, for example:

```csharp
stream.Write([0x1B, 0x40]); // ESC @
```

`StringEncoder` writes deterministic diagnostic tokens. Current token conventions:

- parameterless commands use tokens such as `[INIT]`, `[NOP]`, `[LF]`;
- flags use tokens such as `[BOLD:ON]`, `[BOLD:OFF]`;
- values use tokens such as `[ALIGN:CENTER]`, `[FONT_SIZE:2x2]`;
- text commands emit raw text as-is.

Both encoders iterate over `PrintProgram.Instructions` and switch by instruction type. Unsupported instruction types throw `NotSupportedException` in the default branch.

## Implemented Instructions

| ESC/POS command | Instruction record | Builder method | Binary encoding | String encoding | Decoded |
|---|---|---|---|---|---|
| internal | `NoOpInstruction` | `NoOp()` | no bytes | `[NOP]` | no |
| `ESC @` | `InitializeInstruction` | auto at builder construction | `0x1B 0x40` | `[INIT]` | yes |
| raw ASCII text | `TextInstruction` | `Text(string)`, `PrintLine(string)` | ASCII bytes | text as-is | yes, printable `0x20..0x7E` chunks |
| `LF` | `LineFeedInstruction` | `LineFeed()`, `PrintLine(string)`, auto in `Build()` | `0x0A` | `[LF]` | yes |
| `ESC ! 0` | `ResetPrintModeInstruction` | auto at builder construction | `0x1B 0x21 0x00` | `[RESET_PRINT_MODE]` | yes |
| `ESC ! n` | `SelectPrintModeInstruction` | `PrintMode(CharacterFont, FormatMode)` | `0x1B 0x21 n` | `[PRINT_MODE:FONT_A,...]` or `[PRINT_MODE:FONT_B,...]` | yes |
| `ESC - n` | `UnderlineInstruction` | `UnderlineOn(Thickness)`, `UnderlineOff()` | `0x1B 0x2D n` | `[UNDERLINE:OFF]`, `[UNDERLINE:THIN]`, `[UNDERLINE:THICK]` | yes |
| `ESC E n` | `EmphasizeInstruction` | `BoldOn()`, `BoldOff()` | `0x1B 0x45 n` | `[BOLD:ON]`, `[BOLD:OFF]` | yes |
| `ESC G n` | `DoubleStrikeInstruction` | `DoubleStrikeOn()`, `DoubleStrikeOff()` | `0x1B 0x47 n` | `[DOUBLE_STRIKE:ON]`, `[DOUBLE_STRIKE:OFF]` | yes |
| `ESC M n` | `SelectFontInstruction` | `Font(CharacterFont)` | `0x1B 0x4D n` | `[FONT:A]`, `[FONT:B]` | yes |
| `ESC V n` | `RotationInstruction` | `RotateOn()`, `RotateOff()` | `0x1B 0x56 n` | `[ROTATE_90:ON]`, `[ROTATE_90:OFF]` | yes |
| `ESC { n` | `UpsideDownInstruction` | `UpsideDownOn()`, `UpsideDownOff()` | `0x1B 0x7B n` | `[UPSIDE_DOWN:ON]`, `[UPSIDE_DOWN:OFF]` | yes |
| `GS B n` | `ReverseInstruction` | `InvertOn()`, `InvertOff()` | `0x1D 0x42 n` | `[REVERSE:ON]`, `[REVERSE:OFF]` | yes |
| `GS ! n` | `FontSizeInstruction` | `FontSize(int)`, `FontSize(int, int)` | `0x1D 0x21 n` | `[FONT_SIZE:WxH]` | yes |
| `ESC a n` | `JustifyInstruction` | `Align(Alignment)` | `0x1B 0x61 n` | `[ALIGN:LEFT]`, `[ALIGN:CENTER]`, `[ALIGN:RIGHT]` | yes |
| `ESC $ nL nH` | `AbsolutePositionInstruction` | `MoveToColumn(ushort dots)` | `0x1B 0x24 nL nH` | `[ABS_POS:value]` | yes |
| `ESC \ nL nH` | `RelativePositionInstruction` | `MoveBy(ushort dots)` | `0x1B 0x5C nL nH` | `[REL_POS:value]` | yes |
| `GS L nL nH` | `LeftMarginInstruction` | `LeftMargin(ushort dots)` | `0x1D 0x4C nL nH` | `[LEFT_MARGIN:value]` | yes |
| `ESC SP n` | `RightSpacingInstruction` | `SetCharacterSpacing(double millimeters)` | `0x1B 0x20 n` | `[RIGHT_SPACING:n]` | yes |
| `GS W nL nH` | `PrintAreaWidthInstruction` | `PrintWidth(ushort dots)` | `0x1D 0x57 nL nH` | `[PRINT_WIDTH:n]` | yes |
| `ESC 2` | `ResetLineSpacingInstruction` | `ResetLineSpacing()` | `0x1B 0x32` | `[LINE_SPACING:DEFAULT]` | yes |
| `ESC 3 n` | `SetLineSpacingInstruction` | `SetLineSpacing(double millimeters)` | `0x1B 0x33 n` | `[LINE_SPACING:n]` | yes |
| `ESC d n` | `FeedLinesInstruction` | `FeedLines(byte)` | `0x1B 0x64 n` | `[FEED_LINES:n]` | yes |
| `ESC J n` | `FeedPaperInstruction` | `FeedPaper(double millimeters)` | `0x1B 0x4A n` | `[FEED_PAPER:n]` | yes |
| `HT` | `HorizontalTabInstruction` | `HorizontalTab()` | `0x09` | `[HT]` | yes |
| `ESC D n1..nk NUL` | `SetHorizontalTabsInstruction` | `SetHorizontalTabs(params byte[])`, `ClearHorizontalTabs()` | `0x1B 0x44 ... 0x00` | `[SET_TABS:8,16,24]`, `[SET_TABS:CLEAR]` | yes |
| `ESC m` | `PartialCutInstruction` | none, legacy record only | `0x1B 0x6D` | `[CUT:PARTIAL]` | no |
| `GS V 66 n` | `CutAfterInstruction` | `FeedAndCut(byte)`, auto in `Build()` | `0x1D 0x56 0x42 n` | `[CUT_AFTER:n]` | yes |
| `ESC i` | `HalfCutInstruction` | `HalfCut()` | `0x1B 0x69` | `[CUT:HALF]` | yes |
| `GS V 1` | `CutInstruction` | `PartialCut()` | `0x1D 0x56 0x01` | `[CUT:PARTIAL_GS]` | yes |
| `ESC t n` | `SelectCodePageInstruction` | `SelectCodePage(CharacterCodePage)`, auto OEM437 at construction | `0x1B 0x74 n` | `[CODE_PAGE:name]` | yes |
| `ESC p m t1 t2` | `GeneratePulseInstruction` | `KickDrawer(ConnectorPin, TimeSpan, TimeSpan)`, `OpenCashDrawer()` | `0x1B 0x70 m t1 t2` | `[PULSE:PINx,ON=t1,OFF=t2]` | yes |
| `DLE DC4 1 m t` | `RealTimePulseInstruction` | `RealTimePulse(ConnectorPin, TimeSpan)` | `0x10 0x14 0x01 m t` | `[RT_PULSE:PINx,t=n]` | yes |

Before adding a new instruction, check this table. If the command is already listed, extend tests or builder APIs instead of adding a duplicate record. After adding an instruction, update the record, builder, encoders, decoder, executor, tests, and this table together.

## Emulator

The emulator pipeline is:

```text
byte[]
  -> InstructionDecoder.Decode
  -> Result<PrintProgram>
  -> PrinterEmulator.Emulate
  -> Result<ImmutableArray<Printer>>
  -> history[^1].Receipt
  -> MarkdownRenderer.Render
  -> string
```

`InstructionDecoder` is the inverse of `BinaryEncoder` for supported byte sequences. It returns `Result<PrintProgram>`.

Expected decode failures use `Problem` records:

- `TruncatedSequenceProblem`
- `UnrecognizedBytesProblem`

`NoOpInstruction` encodes to no bytes and is never produced by the decoder. `PartialCutInstruction` is encoded by `BinaryEncoder` but is a legacy record with no builder method and no decoder case.

`InstructionExecutor` executes one instruction against a `Printer` and returns `Result<Printer>`. It is pure: it does not perform I/O and does not mutate the input printer. Unsupported instruction types return `UnsupportedInstructionProblem`.

`PrinterEmulator` orchestrates decoding and execution and returns the full immutable history, one `Printer` per instruction. This history enables tests to inspect intermediate state, while normal rendering uses the final receipt.

## Printer State

`PrinterState.Default` models power-on defaults:

| Property | Default |
|---|---|
| `Bold` | `false` |
| `Underline` | `Thickness.None` |
| `DoubleStrike` | `false` |
| `Font` | `CharacterFont.A` |
| `Rotation` | `false` |
| `UpsideDown` | `false` |
| `Reverse` | `false` |
| `WidthMultiplier` | `1` |
| `HeightMultiplier` | `1` |
| `Justification` | `Alignment.Left` |
| `LeftMargin` | `0` |
| `PrintAreaWidth` | `0` |
| `RightSpacing` | `0` |
| `LineSpacing` | `30` |
| `CodePage` | `CharacterCodePage.OEM437` |
| `TabPositions` | `[9, 17, 25, 33, 41, 49, 57]` |

`InitializeInstruction` resets state to `PrinterState.Default`.

## Receipt Model

`Printer` is a pair of current state and accumulated receipt:

```csharp
public sealed record Printer(PrinterState State, Receipt Receipt);
```

`Receipt` contains immutable document elements:

```text
TextSpan(string Text, TextStyle Style)
LineBreak
FeedLines(byte Count)
HorizontalRule
Barcode
QrCode
Image
```

`TextStyle` snapshots formatting state when text is emitted. Later state changes do not affect existing receipt spans.

Content-emitting instructions append receipt elements. State-only instructions update `PrinterState` and append nothing.

| Instruction | Receipt effect |
|---|---|
| `TextInstruction` | `TextSpan(text, StyleFrom(state))` |
| `LineFeedInstruction` | `LineBreak` |
| `HorizontalTabInstruction` | tab `TextSpan` |
| `FeedLinesInstruction` | `FeedLines(count)` |
| `FeedPaperInstruction` | `FeedLines(1)` |
| cut instructions | `HorizontalRule` |
| peripheral pulse instructions | no effect |

Positioning instructions such as absolute and relative position currently have no receipt effect in the emulator.

## Markdown Renderer

`MarkdownRenderer` converts receipt elements into diagnostic Markdown:

| Element or style | Markdown |
|---|---|
| bold or double strike | `**text**` |
| underline | `<u>text</u>` |
| reverse | `` `text` `` |
| height multiplier >= 2 | `## text` |
| center alignment | `<p align="center">text</p>` |
| right alignment | `<p align="right">text</p>` |
| `LineBreak` | newline |
| `FeedLines(n)` | `n` blank lines |
| `HorizontalRule` | `---` |
| `Barcode`, `QrCode`, `Image` | `[BARCODE]`, `[QR]`, `[IMAGE]` |

Formatting order is reverse, underline, bold/double-strike, heading, then alignment wrapper.

## CLI

The CLI is a developer tool for printer experiments.

Emulate a raw ESC/POS binary file and render Markdown:

```bash
dotnet run --project be/Cashregister.Cli -- emulate --input receipt.bin
```

Print a built-in test program to a file device:

```bash
dotnet run --project be/Cashregister.Cli -- print --device /dev/usb/lp0 test
```

The `PrintTool` class is currently a placeholder and is not wired as an order-printing command.

## Tests

Printmon tests live in:

```text
be/Cashregister.Printmon.Tests/
be/Cashregister.Printmon.Emulator.Tests/
```

When changing instructions, builder behavior, encoders, decoder, executor, or renderer behavior, update tests in the same change. At minimum:

- builder tests should prove emitted instruction order and validation behavior;
- binary encoder tests should prove emitted bytes;
- string encoder tests should prove diagnostic tokens;
- decoder tests should prove supported bytes and expected failure problems;
- executor/emulator tests should prove state and receipt effects;
- renderer tests should prove Markdown output for new receipt elements or styles.

Run backend verification from `be/`:

```bash
dotnet format
dotnet build
dotnet test
```
