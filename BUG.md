# BUG: Emphasize and Double Strike ESC/POS Commands Have No Visible Effect

## Observed behavior

The `TestTool` program sends `ESC E 1` (Emphasize ON) before printing "Emphasis", then `ESC E 0` (Emphasize OFF) before printing "Emphasis" again. On the Zjiang ZJ-8370 thermal printer, both lines print identically — no visible emphasis or boldness difference.

Double Strike (`ESC G n`) is not tested in the current `TestTool` but would exhibit the same problem per the analysis below. The printer manual itself states: "Printer output is the same in double-strike mode and in emphasized mode" (PRINTER.md, ESC G section).

## Byte stream produced by TestTool

The `PrintProgramBuilder` auto-prepends `ESC @` (initialize) and `ESC t 0` (select code table). The full byte sequence for the TestTool program is:

```
1B 40           ESC @           Initialize printer (resets all modes to defaults)
1B 74 00        ESC t 0         Select code page 0 (Std. Europe)
0A              LF              Line feed
1B 61 00        ESC a 0         Justify left
1D 21 01        GS ! 1          Font size: 1x width, 2x height
"Hello, world!" + 0A
1D 21 02        GS ! 2          Font size: 1x width, 3x height
1B 61 01        ESC a 1         Justify center
"Hello, world!" + 0A
1B 61 02        ESC a 2         Justify right
1D 21 03        GS ! 3          Font size: 1x width, 4x height
"Hello, world!" + 0A
1B 61 00        ESC a 0         Justify left
1B 45 01        ESC E 1         *** Emphasize ON ***
"Emphasis" + 0A
1B 45 00        ESC E 0         *** Emphasize OFF ***
"Emphasis" + 0A
0A              LF              (auto-appended by Build())
1D 56 42 01     GS V 66 1      Cut after 1 line (auto-appended by Build())
```

The `ESC E 1` and `ESC E 0` bytes are correct per the ESC/POS specification. The BinaryEncoder implementation matches the manual. Yet the printer produces no visible difference.

## Analysis

### Hypothesis 1 — LIKELY: Missing `ESC ! n` baseline after initialization

**The `ESC !` (Select Print Mode) command is the master mode register.** It controls font selection, emphasis, double-height, double-width, and underline simultaneously via a bitmask. The individual commands (`ESC E`, `ESC -`, `ESC M`, `GS !`) are documented as being able to override specific bits, with the critical note:

> "ESC E can also turn on or off emphasized mode. However, the setting of the **last received command** is effective." — PRINTER.md, ESC ! section

The TestTool program **never sends `ESC !`**. After `ESC @` (initialize), the printer's internal print mode register is reset to the power-on default (which is `ESC ! 0` — no emphasis, Font A, no underline, no double-height/width). However, the register may not be in an _explicitly initialized_ state — it's merely in a _default_ state.

Some ESC/POS printer firmwares (especially on budget thermal printers like the ZJ-8370) require an **explicit `ESC ! 0`** after `ESC @` to properly initialize the print mode register before individual mode commands like `ESC E` can modify specific bits. Without this explicit baseline, the printer may treat `ESC E` as a no-op because the mode register hasn't been "opened for editing" by a prior `ESC !`.

**Correction:** The `PrintProgramBuilder` should auto-include `ESC ! 0` (select print mode: Font A, no flags) in the preamble, immediately after `ESC @` and `ESC t 0`. This gives the print mode register an explicit baseline.

In code:

```csharp
// PrintProgramBuilder constructor — add ESC ! 0 to preamble
private readonly List<Instruction> instructions = [
    new InitializeInstruction(),
    new SelectCodeTableInstruction(),
    new SelectPrintModeInstruction(false, FormatMode.None)  // ESC ! 0 — establish baseline
];
```

### Hypothesis 2 — LIKELY: `GS ! n` with non-zero value interferes with emphasis state

The TestTool sets `GS ! 3` (4x height) and **never resets it back to `GS ! 0`** before the emphasis text. The "Emphasis" text is printed at 4x character height.

The PRINTER.md notes for `ESC !` state:

> "GS ! can also select character size. However, the setting of the last received command is effective."

This documents a shared-state interaction between `ESC !` and `GS !` for character size. While the manual only explicitly mentions _size_, some printer firmwares reset the entire mode register (including emphasis bits) when processing `GS !` — especially if `ESC !` was never explicitly sent to establish which bits the printer should preserve.

Combined with Hypothesis 1: if the mode register was never explicitly set via `ESC !`, then the printer has no record of which bits were set by individual commands. When `GS !` modifies the size bits, the printer may zero out the entire mode register (including emphasis) as a side effect.

**Correction:** Reset font size to normal before emphasis sections:

```csharp
.FontSize(0)        // GS ! 0 — reset to normal size
.EmphasizeOn()      // ESC E 1
.PrintLine("Emphasis")
```

Or better, send `ESC ! n` with the Emphasized flag to set emphasis via the master register (see Hypothesis 3).

### Hypothesis 3 — POSSIBLE: ESC E is ignored; only ESC ! controls emphasis on this printer

Some budget thermal printer firmwares implement `ESC !` as the sole mechanism for setting emphasis and only partially implement (or entirely ignore) the individual `ESC E` command. The printer accepts the `ESC E` bytes without error (they're syntactically valid) but doesn't actually change the emphasis state.

The manual says: "This command and ESC ! turn on and off emphasized mode **in the same way**." The phrase "in the same way" may mean they use the same internal register — but on this firmware, only `ESC !` has write access to that register.

**Correction:** Use `ESC ! n` (via `UseFontA(FormatMode.Emphasized)`) instead of `ESC E 1` to activate emphasis:

```csharp
.FontSize(0)                                    // Reset to normal size first
.UseFontA(FormatMode.Emphasized)                // ESC ! 0x08 — emphasis via master register
.PrintLine("Emphasis")
.UseFontA(FormatMode.None)                      // ESC ! 0x00 — emphasis off
.PrintLine("No Emphasis")
```

This bypasses `ESC E` entirely and controls emphasis through the master print mode command, which is universally supported.

### Hypothesis 4 — LESS LIKELY: Single write floods the printer receive buffer

The `FileDevice` writes the entire encoded byte array in a single `WriteAsync` call:

```csharp
await fileDeviceStream.WriteAsync(encodeResult.Value);
```

The MANUAL.md section 10.9 warns: "When sending large print jobs over Ethernet [...] insert small delays (~50 ms) between data chunks to prevent buffer overflow and garbled output." While the TestTool program is small (~100 bytes), USB printers can also experience buffer issues. If any bytes in the command stream are dropped or corrupted due to buffer overflow, `ESC E 1` could be silently lost.

For the small TestTool program this is unlikely, but it becomes relevant for larger print programs (e.g., full receipts with barcodes and images).

**Correction:** This is not likely the cause for the current small program. However, for robustness, `FileDevice` could be enhanced to write in chunks with small delays between them. This is a lower-priority improvement.

### Hypothesis 5 — LESS LIKELY: Emphasis is visually imperceptible at large font sizes

Thermal printers produce emphasis by double-heating or printing with a horizontal offset. At `GS ! 3` (4x character height), the characters are already very large and the thermal elements are already at high intensity. The emphasis effect may be physically present but visually indistinguishable from non-emphasized text at this size.

The TestTool doesn't reset `FontSize` to 0 before the emphasis test, so "Emphasis" is printed at 4x height. At normal size (1x), emphasis would be much more visible.

**Correction:** The TestTool should test emphasis at normal font size (`GS ! 0`) for a fair comparison.

## Recommended corrections (in priority order)

### 1. Add `ESC ! 0` to the PrintProgramBuilder preamble

Modify the `PrintProgramBuilder` to auto-include `SelectPrintModeInstruction(false, FormatMode.None)` in the preamble after `InitializeInstruction` and `SelectCodeTableInstruction`. This explicitly initializes the print mode register and gives individual commands like `ESC E` a known baseline to modify.

File: `be/Cashregister.Printmon/PrintProgramBuilder.cs`

```csharp
private readonly List<Instruction> instructions = [
    new InitializeInstruction(),
    new SelectCodeTableInstruction(),
    new SelectPrintModeInstruction(false, FormatMode.None)
];
```

### 2. Fix the TestTool to reset font size before emphasis test

The TestTool should reset to `FontSize(0)` before the emphasis/double-strike section so the comparison is fair and the `GS !` state doesn't interfere.

File: `be/Cashregister.Cli/Tools/TestTool.cs`

```csharp
.Justify(Justification.Left)
.FontSize(0)          // Reset to normal size
.EmphasizeOn()
.PrintLine("Emphasis ON")
.EmphasizeOff()
.PrintLine("Emphasis OFF")
.DoubleStrikeOn()
.PrintLine("Double Strike ON")
.DoubleStrikeOff()
.PrintLine("Double Strike OFF")
```

### 3. If ESC E still doesn't work: use ESC ! with FormatMode.Emphasized

If corrections 1 and 2 don't resolve the issue, replace standalone `ESC E` usage with `ESC !` using the `FormatMode.Emphasized` flag. This is the most reliable way to activate emphasis because `ESC !` is the master mode register and is universally supported across all ESC/POS printers.

Consider adding convenience methods to `PrintProgramBuilder` that internally use `ESC !` instead of `ESC E`:

```csharp
public PrintProgramBuilder BoldOn() => UseFontA(FormatMode.Emphasized);
public PrintProgramBuilder BoldOff() => UseFontA(FormatMode.None);
```

**Trade-off:** Using `ESC !` to control emphasis means it will also reset other mode bits (underline, double-height, etc.) unless they are explicitly OR'd into the bitmask. This requires the builder to track the current print mode state internally, which adds complexity. A `PrintModeTracker` that maintains the current bitmask and applies individual changes through `ESC !` would be the clean approach.

### 4. Future: Implement print mode state tracking in PrintProgramBuilder

The root cause of the interaction between `ESC !`, `ESC E`, `GS !`, and other mode commands is that they share internal printer state. The safest approach is to track the desired print mode in the builder and always emit `ESC !` with the full bitmask when any mode bit changes. This prevents the "last command wins" ambiguity entirely.

Proposed architecture:

```csharp
// Inside PrintProgramBuilder
private FormatMode currentMode = FormatMode.None;
private bool currentUseFontB = false;

public PrintProgramBuilder EmphasizeOn()
{
    currentMode |= FormatMode.Emphasized;
    return AddInstruction(new SelectPrintModeInstruction(currentUseFontB, currentMode));
}

public PrintProgramBuilder EmphasizeOff()
{
    currentMode &= ~FormatMode.Emphasized;
    return AddInstruction(new SelectPrintModeInstruction(currentUseFontB, currentMode));
}
```

This would make `EmphasizeOn()`/`EmphasizeOff()` emit `ESC ! n` instead of `ESC E n`, but with the correct accumulated bitmask. The `EmphasizeInstruction` and `DoubleStrikeInstruction` records would become unused and could be removed or kept as documentation artifacts.

### 5. Future: Add GS ! 0 (font size reset) as a builder convenience

Add a `FontSizeNormal()` or `FontSizeReset()` method that emits `GS ! 0`, making it easy to return to default character size. Currently, users must know to call `FontSize(0)`.

## Summary

| # | Hypothesis | Likelihood | Fix |
|---|-----------|-----------|-----|
| 1 | Missing `ESC ! 0` baseline after `ESC @` | **Likely** | Add `SelectPrintModeInstruction` to builder preamble |
| 2 | `GS ! n` interferes with emphasis state | **Likely** | Reset font size before emphasis; always send `ESC !` baseline |
| 3 | Printer ignores `ESC E`; only `ESC !` works | **Possible** | Use `UseFontA(FormatMode.Emphasized)` instead of `ESC E` |
| 4 | Single write buffer overflow | **Unlikely** for small programs | Chunk writes with delays (future) |
| 5 | Emphasis imperceptible at large font size | **Unlikely** as sole cause | Reset to `FontSize(0)` before emphasis test |

**Recommended testing order:** Apply fix 1, then fix 2, then fix 3 if needed. Fix 4 (print mode state tracking) is an architectural improvement for long-term reliability.
