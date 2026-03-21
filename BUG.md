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

## How ESC/POS print modes work

### The printer has internal state registers

An ESC/POS printer is a stateful machine. When it receives text bytes, it renders them according to the **current values** of several internal registers. These registers control things like: which font to use, whether emphasis is on, whether underline is on, character width/height multipliers, justification, and so on.

Commands like `ESC E 1` don't print anything themselves — they modify a register. The text that follows is then rendered according to the updated register state. This is why mode commands must appear **before** the text they should affect.

### ESC @ (Initialize) sets registers to power-on defaults

When the printer processes `ESC @`, it resets all internal registers to the values they had at power-on. Per the manual:

> "Clears the data in the print buffer and resets the printer mode to the mode that was in effect when the power was turned on."

After `ESC @`, the printer is in this default state:

| Register / mode       | Default value | Equivalent command |
|-----------------------|---------------|-------------------|
| Font                  | Font A        | `ESC ! 0` bit 0 = 0 |
| Emphasis              | OFF           | `ESC ! 0` bit 3 = 0, or `ESC E 0` |
| Double-height         | OFF           | `ESC ! 0` bit 4 = 0 |
| Double-width          | OFF           | `ESC ! 0` bit 5 = 0 |
| Underline             | OFF           | `ESC ! 0` bit 7 = 0, or `ESC - 0` |
| Character size        | 1x width, 1x height | `GS ! 0` |
| Justification         | Left          | `ESC a 0` |
| Double-strike         | OFF           | `ESC G 0` |
| Rotation              | OFF           | `ESC V 0` |
| Upside-down           | OFF           | `ESC { 0` |

### ESC ! is the master print mode register — it writes ALL mode bits at once

`ESC ! n` is special. It is a **composite command** that controls five aspects of text rendering simultaneously through a single bitmask byte:

```
Bit 0: Font          (0 = Font A, 1 = Font B)
Bit 3: Emphasis       (0 = off, 1 = on)
Bit 4: Double-height  (0 = off, 1 = on)
Bit 5: Double-width   (0 = off, 1 = on)
Bit 7: Underline      (0 = off, 1 = on)
```

When you send `ESC ! n`, **every bit in the register is written**, not just the ones you care about. For example:

- `ESC ! 0x08` sets emphasis ON, but also sets font to A, double-height OFF, double-width OFF, and underline OFF.
- `ESC ! 0x00` sets **everything** OFF.
- `ESC ! 0x89` sets emphasis ON + underline ON + Font B, but double-height and double-width OFF.

This means `ESC !` is a "write-all" operation. It doesn't toggle a single bit — it replaces the entire register.

### Individual commands (ESC E, ESC -, etc.) modify specific bits

In contrast to `ESC !`, individual commands are narrow:

- `ESC E n` — modifies **only** the emphasis bit
- `ESC - n` — modifies **only** the underline bit (and its thickness)
- `ESC M n` — modifies **only** the font selection
- `GS ! n` — modifies **only** character width and height

These are intended as convenience commands for toggling one mode without disturbing the others.

### The "last received command is effective" rule

The manual warns for each overlapping pair:

> "ESC E can also turn on or off emphasized mode. However, the setting of the **last received command** is effective."

This means `ESC !` and `ESC E` write to the **same internal emphasis bit**. Whichever command the printer processes last determines the value. Example:

```
ESC ! 0x08    → emphasis ON  (via master register, also sets font=A, no underline, etc.)
ESC E 0       → emphasis OFF (individual command overrides the bit)
"Hello"       → printed WITHOUT emphasis (ESC E was last)
```

And the reverse:

```
ESC E 1       → emphasis ON  (individual command)
ESC ! 0x00    → emphasis OFF (master register sets ALL bits to 0, including emphasis)
"Hello"       → printed WITHOUT emphasis (ESC ! was last, and it zeroed emphasis)
```

This is the core danger: **sending `ESC ! n` for any reason (e.g., to change font) will silently reset emphasis unless bit 3 is explicitly set in `n`.** The individual commands and the master register are not independent — they are two interfaces to the same state.

### The same rule applies to GS ! and character size

`ESC !` bits 4 and 5 control double-height and double-width. `GS ! n` also controls character width and height (with finer granularity: 1x-8x instead of just 1x/2x). The manual says:

> "GS ! can also select character size. However, the setting of the last received command is effective."

So `ESC !` and `GS !` share the character-size state. This creates a potential interaction: if the printer firmware implements `GS !` by writing to the same internal register that `ESC !` uses, sending `GS !` might also affect (or be affected by) the emphasis, underline, and font bits that live in that register. The manual doesn't explicitly document whether `GS !` preserves or resets the non-size bits, which is where firmware differences between printer models become relevant.

### What this means for our program

The TestTool program after `ESC @` sends:
1. `GS ! 1`, then `GS ! 2`, then `GS ! 3` — three character size changes
2. Then `ESC E 1` — emphasis on
3. Then text

The question is: **does `ESC E 1` successfully set the emphasis bit after `ESC @` and three `GS !` commands, without `ESC !` ever having been sent?**

According to the spec, yes — `ESC E` should work independently. But the spec also warns to "be careful" about the interaction, and this particular printer is a budget model where the firmware may not implement the individual commands as truly independent register writes.

Sending `ESC ! 0` explicitly after `ESC @` ensures the master print mode register is in a known software-initialized state, not just a hardware-reset state. This is defensive programming: it costs 3 bytes and eliminates an entire class of firmware-dependent behavior.

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
