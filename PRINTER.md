# 80mm Thermal Receipt Printer — ESC/POS Command Reference

## Initialization

### ESC @ — Initialize printer

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 40`                        |
| Decimal    | `27 64`                        |
| ASCII      | `ESC @`                        |

**Behavior:**
Clears the data in the print buffer and resets the printer mode to the mode that was in effect when the power was turned on.

**Constraints / Notes:**
- The DIP switch settings are not checked again.
- The data in the receive buffer is not cleared.
- The macro definition is not cleared.

---

## Text Formatting

### ESC ! n — Select print mode(s)

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 21 n`                      |
| Decimal    | `27 33 n`                      |
| ASCII      | `ESC ! n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): Bitmask selecting print modes. Bit meanings:
  - Bit 0: 0 = Character font A (12x24), 1 = Character font B (9x17).
  - Bit 1: N/A.
  - Bit 2: N/A.
  - Bit 3: 0 = Emphasized mode not selected, 1 = Emphasized mode selected.
  - Bit 4: 0 = Double-height mode not selected, 1 = Double-height mode selected.
  - Bit 5: 0 = Double-width mode not selected, 1 = Double-width mode selected.
  - Bit 6: N/A.
  - Bit 7: 0 = Underline mode not selected, 1 = Underline mode selected.

**Behavior:**
Selects print mode(s) using n as a bitmask. When both double-height and double-width modes are selected, quadruple-size characters are printed. Default n=0.

**Constraints / Notes:**
- The printer can underline all characters, but cannot underline the space set by HT or 90-degree clockwise rotated characters.
- The thickness of the underline is that selected by ESC -, regardless of character size.
- When some characters in a line are double or more height, all characters on the line are aligned at the baseline.
- ESC E can also turn on or off emphasized mode. However, the setting of the last received command is effective.
- ESC - can also turn on or off underline mode. However, the setting of the last received command is effective.
- GS ! can also select character size. However, the setting of the last received command is effective.
- Emphasized mode is effective for alphanumeric and Hanzi. All print modes except emphasized mode are effective only for alphanumeric.

---

### ESC - n — Turn underline mode on/off

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 2D n`                      |
| Decimal    | `27 45 n`                      |
| ASCII      | `ESC - n`                      |

**Parameters:**
- n (byte, 0 <= n <= 2 or 48 <= n <= 50): Selects underline mode.
  - 0, 48: Turns off underline mode.
  - 1, 49: Turns on underline mode (1-dot thick).
  - 2, 50: Turns on underline mode (2-dots thick).

**Behavior:**
Turns underline mode on or off. The printer can underline all characters including right-side character spacing, but cannot underline the space set by HT. Default n=0.

**Constraints / Notes:**
- The printer cannot underline 90-degree clockwise rotated characters and white/black inverted characters.
- When underline mode is turned off by setting n to 0 or 48, the following data is not underlined, and the underline thickness set before the mode is turned off does not change. The default underline thickness is 1 dot.
- Changing the character size does not affect the current underline thickness.
- Underline mode can also be turned on or off by using ESC !. Note, however, that the last received command is effective.

---

### ESC E n — Turn emphasized mode on/off

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 45 n`                      |
| Decimal    | `27 69 n`                      |
| ASCII      | `ESC E n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): When the LSB of n is 0, emphasized mode is turned off. When the LSB of n is 1, emphasized mode is turned on.

**Behavior:**
Turns emphasized (bold) mode on or off. Only the least significant bit of n is enabled. Default n=0.

**Constraints / Notes:**
- This command and ESC ! turn on and off emphasized mode in the same way. Be careful when this command is used with ESC !.

---

### ESC G n — Turn on/off double-strike mode

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 47 n`                      |
| Decimal    | `27 71 n`                      |
| ASCII      | `ESC G n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): When the LSB of n is 0, double-strike mode is turned off. When the LSB of n is 1, double-strike mode is turned on.

**Behavior:**
Turns double-strike mode on or off. Only the lowest bit of n is enabled. Default n=0.

**Constraints / Notes:**
- Printer output is the same in double-strike mode and in emphasized mode.

---

### ESC M n — Select character font

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 4D n`                      |
| Decimal    | `27 77 n`                      |
| ASCII      | `ESC M n`                      |

**Parameters:**
- n (byte, n=0, 1, 48, 49): Selects character font.
  - 0, 48: Character font A (12 x 24) selected.
  - 1, 49: Character font B (9 x 17) selected.

**Behavior:**
Selects character font A or B.

**Constraints / Notes:**
- ESC ! can also select the font type. But last received command settings are made effective.

---

### ESC V n — Turn 90-degree clockwise rotation mode on/off

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 56 n`                      |
| Decimal    | `27 86 n`                      |
| ASCII      | `ESC V n`                      |

**Parameters:**
- n (byte, 0 <= n <= 1 or 48 <= n <= 49): Selects rotation mode.
  - 0, 48: Turns off 90-degree clockwise rotation mode.
  - 1, 49: Turns on 90-degree clockwise rotation mode.

**Behavior:**
Turns 90-degree clockwise rotation mode on or off. This command affects printing in standard mode. The setting is always effective. Default n=0.

**Constraints / Notes:**
- When underline mode is turned on, the printer does not underline 90-degree clockwise-rotated characters.
- Double-width and double-height commands in 90-degree rotation mode enlarge characters in the opposite directions from double-height and double-width commands in normal mode.

---

### ESC { n — Turn on/off upside-down printing mode

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 7B n`                      |
| Decimal    | `27 123 n`                     |
| ASCII      | `ESC { n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): When the LSB of n is 0, upside-down printing mode is turned off. When the LSB of n is 1, upside-down printing mode is turned on.

**Behavior:**
Turns on/off upside-down printing mode. Only the lowest bit of n is valid. In upside-down printing mode, the printer rotates the line to be printed by 180 degrees and then prints it. Default n=0.

**Constraints / Notes:**
- This command is enabled only when processed at the beginning of a line in standard mode.

---

### GS ! n — Set character size

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 21 n`                      |
| Decimal    | `29 33 n`                      |
| ASCII      | `GS ! n`                       |

**Parameters:**
- n (byte, 0 <= n <= 255): Selects character height using bits 0 to 3 and character width using bits 4 to 7. Valid vertical and horizontal multipliers are 1 to 8.
  - Width (bits 4-7): 0x00=1 (Normal), 0x10=2 (Double-width), 0x20=3, 0x30=4, 0x40=5, 0x50=6, 0x60=7, 0x70=8.
  - Height (bits 0-3): 0x00=1 (Normal), 0x01=2 (Double-height), 0x02=3, 0x03=4, 0x04=5, 0x05=6, 0x06=7, 0x07=8.

**Behavior:**
Selects the character height and width independently, allowing magnification from 1x to 8x in each direction. Default n=0 (normal size).

**Constraints / Notes:**
- This command is effective for all characters (alphanumeric and Kanji) except HRI characters.
- If n is outside the defined range, this command is ignored.
- In standard mode, the vertical direction is the paper feed direction, and the horizontal direction is perpendicular to the paper feed direction. However, when character orientation changes in 90-degree clockwise-rotation mode, the relationship between vertical and horizontal directions is reversed.
- When characters are enlarged with different sizes on one line, all the characters on the line are aligned at the baseline.
- The ESC ! command can also turn double-width and double-height modes on or off. However, the setting of the last received command is effective.

---

### GS B n — Turn white/black reverse printing mode

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 42 n`                      |
| Decimal    | `29 66 n`                      |
| ASCII      | `GS B n`                       |

**Parameters:**
- n (byte, 0 <= n <= 255): When the LSB of n is 0, white/black reverse mode is turned off. When the LSB of n is 1, white/black reverse mode is turned on.

**Behavior:**
Turns white/black reverse printing mode on or off. Only the lowest bit of n is valid. Default n=0.

**Constraints / Notes:**
- This command is available for built-in characters and user-defined characters.
- When white/black reverse printing mode is on, it is also applied to character spacing set by ESC SP.
- This command does not affect bit image, user-defined bit image, bar code, HRI characters, and spacing skipped by HT, ESC $, and ESC \.
- This command does not affect the space between lines.
- White/black reverse mode has a higher priority than underline mode. Even if underline mode is on, it is disabled (but not canceled) when white/black reverse mode is selected.

---

## Text Alignment & Layout

### ESC a n — Select justification

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 61 n`                      |
| Decimal    | `27 97 n`                      |
| ASCII      | `ESC a n`                      |

**Parameters:**
- n (byte, 0 <= n <= 2 or 48 <= n <= 50): Selects justification.
  - 0, 48: Left justification.
  - 1, 49: Centering.
  - 2, 50: Right justification.

**Behavior:**
Aligns all the data in one line to the specified position. Default n=0 (left justification).

**Constraints / Notes:**
- The command is enabled only when processed at the beginning of the line in standard mode.
- This command executes justification in the printing area.
- This command justifies the space area according to HT, ESC $ or ESC \.

---

### ESC $ nL nH — Set absolute print position

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 24 nL nH`                  |
| Decimal    | `27 36 nL nH`                  |
| ASCII      | `ESC $ nL nH`                  |

**Parameters:**
- nL (byte, 0 <= nL <= 255): Low byte of position.
- nH (byte, 0 <= nH <= 255): High byte of position.

**Behavior:**
Sets the distance from the beginning of the line to the position at which subsequent characters are to be printed. The distance is [(nL + nH * 256) * (horizontal motion unit)] inches.

**Constraints / Notes:**
- Settings outside the specified printable area are ignored.
- In standard mode, the horizontal motion unit (x) is used.

---

### ESC \ nL nH — Set relative print position

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 5C nL nH`                  |
| Decimal    | `27 92 nL nH`                  |
| ASCII      | `ESC \ nL nH`                  |

**Parameters:**
- nL (byte, 0 <= nL <= 255): Low byte of relative position.
- nH (byte, 0 <= nH <= 255): High byte of relative position.

**Behavior:**
Sets the print starting position based on the current position by using the horizontal or vertical motion unit. The distance from the current position is [(nL + nH * 256) * (horizontal or vertical motion unit)].

**Constraints / Notes:**
- Any setting that exceeds the printable area is ignored.
- When pitch N is specified to the right: nL + nH * 256 = N. When pitch N is specified to the left (negative direction), use the complement of 65536.
- In standard mode, the horizontal motion unit is used.

---

### GS L nL nH — Set left margin

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 4C nL nH`                  |
| Decimal    | `29 76 nL nH`                  |
| ASCII      | `GS L nL nH`                   |

**Parameters:**
- nL (byte, 0 <= nL <= 255): Low byte of left margin.
- nH (byte, 0 <= nH <= 255): High byte of left margin.

**Behavior:**
Sets the left margin using nL and nH. The left margin is set to [(nL + nH * 256) * 0.125mm]. Default nL=0, nH=0.

**Constraints / Notes:**
- This command is effective only when processed at the beginning of the line in standard mode.
- If the setting exceeds the printable area, the maximum value of the printable area is used.

---

## Character Spacing

### ESC SP n — Set right-side character spacing

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 20 n`                      |
| Decimal    | `27 32 n`                      |
| ASCII      | `ESC SP n`                     |

**Parameters:**
- n (byte, 0 <= n <= 255): Sets the right-side character spacing to [n * 0.125 mm].

**Behavior:**
Sets the right-side character spacing for subsequent characters. Default n=0.

**Constraints / Notes:**
- For double-width mode, the right-side character spacing is double than the normal mode. When the character is magnified, the right-side character spacing is n times than the normal mode.
- This command does not affect the setting of Hanzi characters.
- The command sets the value of independent standard mode in each mode.

---

## Feed & Motion

### HT — Horizontal tab

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `09`                           |
| Decimal    | `9`                            |
| ASCII      | `HT`                          |

**Behavior:**
Moves the print position to the next horizontal tab position.

**Constraints / Notes:**
- This command is ignored unless the next horizontal tab position has been set.
- If the next horizontal tab position exceeds the printing area, the printer sets the printing position to [Printing area width + 1].
- Horizontal tab positions are set with ESC D.
- If this command is received when the printing position is at [printing area width + 1], the printer executes print buffer-full printing of the current line and horizontal tab processing from the beginning of the next line.

---

### LF — Print and line feed

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `0A`                           |
| Decimal    | `10`                           |
| ASCII      | `LF`                          |

**Behavior:**
Prints the data in the print buffer and feeds one line based on the current line spacing. This command sets the print position to the beginning of the line.

---

### ESC J n — Print and feed paper

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 4A n`                      |
| Decimal    | `27 74 n`                      |
| ASCII      | `ESC J n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): Feed amount in [n * vertical or horizontal motion unit] inches.

**Behavior:**
Prints the data in the print buffer and feeds the paper [n * vertical or horizontal motion unit] inches. After printing is completed, this command sets the print starting position to the beginning of the line.

**Constraints / Notes:**
- The paper feed amount set by this command does not affect the values set by ESC 2 or ESC 3.
- In standard mode, the printer uses the vertical motion unit (y).

---

### ESC d n — Print and feed n lines

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 64 n`                      |
| Decimal    | `27 100 n`                     |
| ASCII      | `ESC d n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): Number of lines to feed.

**Behavior:**
Prints the data in the print buffer and feeds n lines. This command sets the print starting position to the beginning of the line.

**Constraints / Notes:**
- This command does not affect the line spacing set by ESC 2 or ESC 3.
- The maximum paper feed amount is 1016 mm (40 inches). If the paper feed amount (n x line spacing) is more than 1016 mm (40 inches), the printer feeds the paper only 1016 mm (40 inches).

---

### ESC D n1...nk NUL — Set horizontal tab positions

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 44 n1...nk 00`             |
| Decimal    | `27 68 n1...nk 0`              |
| ASCII      | `ESC D n1...nk NUL`            |

**Parameters:**
- n (byte, 1 <= n <= 255): Column number for setting a horizontal tab position from the beginning of the line.
- k (byte, 0 <= k <= 32): Total number of horizontal tab positions to be set.

**Behavior:**
Sets horizontal tab positions. n specifies the column number for a tab position from the beginning of the line. k indicates the total number of positions to set.

**Constraints / Notes:**
- The horizontal tab position is stored as a value of [character width * n] measured from the beginning of the line. The character width includes the right-side character spacing, and double-width characters are set with twice the width of normal characters.
- This command cancels the previous horizontal tab settings.
- When setting n = 8, the print position is moved to column 9 by sending HT.
- Up to 32 tab positions (k = 32) can be set. Data exceeding 32 tab positions is processed as normal data.
- When [n] k is less than or equal to the preceding value [n] k-1, tab setting is finished and the following data is processed as normal data.
- ESC D NUL cancels all horizontal tab positions.
- The previously specified horizontal tab positions do not change, even if the character width changes.
- The character width is memorized for each standard and page mode.
- Default tab positions are at intervals of 8 characters (columns 9, 17, 25,...) for font A (12x24).

---

## Line Spacing

### ESC 2 — Select default line spacing

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 32`                        |
| Decimal    | `27 50`                        |
| ASCII      | `ESC 2`                        |

**Behavior:**
Selects default line spacing of 3.75mm (30 * 0.125mm). The line spacing can be set independently in standard mode and in page mode.

---

### ESC 3 n — Set line spacing

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 33 n`                      |
| Decimal    | `27 51 n`                      |
| ASCII      | `ESC 3 n`                      |

**Parameters:**
- n (byte, 0 <= n <= 255): Sets the line spacing to [n * 0.125mm].

**Behavior:**
Sets the line spacing for subsequent line feeds. Default n=30 (3.75mm).

**Constraints / Notes:**
- The line spacing can be set independently in standard mode and in page mode.
- In standard mode, the vertical motion unit (y) is used.

---

## Cut

### ESC i — Part cutter (half cut)

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 69`                        |
| Decimal    | `27 105`                       |
| ASCII      | `ESC i`                        |

**Behavior:**
Selects cut mode and performs a half cut (partial cut).

---

### ESC m — Partial cut

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 6D`                        |
| Decimal    | `27 109`                       |
| ASCII      | `ESC m`                        |

**Behavior:**
Selects cut mode and performs a half cut (partial cut).

---

### GS V m / GS V m n — Select cut mode and cut paper

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 56 m` or `1D 56 m n`      |
| Decimal    | `29 86 m` or `29 86 m n`      |
| ASCII      | `GS V m` or `GS V m n`        |

**Parameters:**
- m (byte): Cut mode selector.
  - Form 1: m = 1 or 49 — Partial cut (one point left uncut).
  - Form 2: m = 66 — Feeds paper then partial cut.
- n (byte, 0 <= n <= 255, used only in Form 2): Feed amount before cutting. The printer feeds paper to (cutting position + [n * vertical motion unit]) and cuts the paper partially (one point left uncut).

**Behavior:**
Selects a mode for cutting paper and executes paper cutting. When m=1 or 49, a partial cut is performed immediately. When m=66, the printer feeds paper by the specified amount then performs a partial cut.

**Constraints / Notes:**
- Cutting state varies depending on automatically loaded cutter types.
- This command is effective only when processed at the beginning of a line.
- Only the partial cut is available; there is no full cut.
- When n = 0 (Form 2), the printer feeds the paper to the cutting position and cuts it.

---

## Barcodes

### GS h n — Set barcode height

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 68 n`                      |
| Decimal    | `29 104 n`                     |
| ASCII      | `GS h n`                       |

**Parameters:**
- n (byte, 1 <= n <= 255): Height of the bar code in number of dots in the vertical direction.

**Behavior:**
Selects the height of the bar code. Default n=162.

---

### GS w n — Set barcode width

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 77 n`                      |
| Decimal    | `29 119 n`                     |
| ASCII      | `GS w n`                       |

**Parameters:**
- n (byte, 2 <= n <= 6): Sets the horizontal size of the bar code. Values and widths:
  - 2: Multi-level 0.250mm, Binary thin 0.250mm, Binary thick 0.625mm.
  - 3: Multi-level 0.375mm, Binary thin 0.375mm, Binary thick 1.000mm.
  - 4: Multi-level 0.560mm, Binary thin 0.500mm, Binary thick 1.250mm.
  - 5: Multi-level 0.625mm, Binary thin 0.625mm, Binary thick 1.625mm.
  - 6: Multi-level 0.750mm, Binary thin 0.750mm, Binary thick 2.000mm.

**Behavior:**
Sets the horizontal size (module width) of the bar code. Default n=3.

**Constraints / Notes:**
- Multi-level bar codes: UPC-A, UPC-E, JAN13 (EAN13), JAN8 (EAN8), CODE93, CODE128.
- Binary-level bar codes: CODE39, ITF, CODABAR.

---

### GS H n — Select printing position for HRI characters

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 48 n`                      |
| Decimal    | `29 72 n`                      |
| ASCII      | `GS H n`                       |

**Parameters:**
- n (byte, 0 <= n <= 3 or 48 <= n <= 51): Selects printing position of HRI characters.
  - 0, 48: Not printed.
  - 1, 49: Above the bar code.
  - 2, 50: Below the bar code.
  - 3, 51: Both above and below the bar code.

**Behavior:**
Selects the printing position of Human Readable Interpretation (HRI) characters when printing a bar code. Default n=0.

**Constraints / Notes:**
- HRI characters are printed using the font specified by GS f.

---

### GS f n — Select font for HRI characters

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 66 n`                      |
| Decimal    | `29 102 n`                     |
| ASCII      | `GS f n`                       |

**Parameters:**
- n (byte, n=0, 1, 48, 49): Selects a font for HRI characters.
  - 0, 48: Font A (12x24).
  - 1, 49: Font B (9x17).

**Behavior:**
Selects the font used for printing Human Readable Interpretation (HRI) characters when printing a bar code. Default n=0.

**Constraints / Notes:**
- HRI characters are printed at the position specified by GS H.

---

### GS x n — Set barcode left offset

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1D 78 n`                      |
| Decimal    | `29 120 n`                     |
| ASCII      | `GS x n`                       |

**Parameters:**
- n (byte, 0 <= n <= 255): Starting position (left pitch) for barcode printing.

**Behavior:**
Sets the left starting position (left pitch) for barcode printing.

---

### GS k m d1...dk NUL / GS k m n d1...dn — Print barcode

| Field      | Value                                          |
|------------|------------------------------------------------|
| Hex        | `1D 6B m d1...dk 00` (Form 1) or `1D 6B m n d1...dn` (Form 2) |
| Decimal    | `29 107 m d1...dk 0` (Form 1) or `29 107 m n d1...dn` (Form 2) |
| ASCII      | `GS k m d1...dk NUL` (Form 1) or `GS k m n d1...dn` (Form 2)   |

**Parameters:**
- m (byte): Selects the bar code system.
  - Form 1 (0 <= m <= 6):
    - 0: UPC-A (11 <= k <= 12, 48 <= d <= 57).
    - 1: UPC-E (11 <= k <= 12, 48 <= d <= 57).
    - 2: JAN13 / EAN13 (12 <= k <= 13, 48 <= d <= 57).
    - 3: JAN8 / EAN8 (7 <= k <= 8, 48 <= d <= 57).
    - 4: CODE39 (1 <= k, 48 <= d <= 57, 65 <= d <= 90, 32, 36, 37, 43, 45, 46, 47).
    - 5: ITF (1 <= k, even number; 48 <= d <= 57).
    - 6: CODABAR (1 <= k, 48 <= d <= 57, 65 <= d <= 68, 36, 43, 45, 46, 47, 58).
  - Form 2 (65 <= m <= 73):
    - 65: UPC-A (11 <= n <= 12, 48 <= d <= 57).
    - 66: UPC-E (11 <= n <= 12, 48 <= d <= 57).
    - 67: JAN13 / EAN13 (12 <= n <= 13, 48 <= d <= 57).
    - 68: JAN8 / EAN8 (7 <= n <= 8, 48 <= d <= 57).
    - 69: CODE39 (1 <= n <= 255, 48 <= d <= 57, 65 <= d <= 90, 32, 36, 37, 43, 45, 46, 47).
    - 70: ITF (1 <= n <= 255, even number; 48 <= d <= 57).
    - 71: CODABAR (1 <= n <= 255, 48 <= d <= 57, 65 <= d <= 68, 36, 43, 45, 46, 47, 58).
    - 72: CODE93 (1 <= n <= 255, 0 <= d <= 127).
    - 73: CODE128 (2 <= n <= 255, 0 <= d <= 127).
- n (byte, Form 2 only): Number of bar code data bytes.
- d1...dk/dn (bytes): Bar code data.

**Behavior:**
Selects a bar code system and prints the bar code. Form 1 terminates data with NUL (0x00). Form 2 uses n to specify the data length.

**Constraints / Notes:**
- If d is outside of the specified range, the printer only feeds paper and processes the following data as normal data.
- If the horizontal size exceeds the printing area, the printer only feeds the paper.
- This command feeds as much paper as is required to print the bar code, regardless of the line spacing specified by ESC 2 or ESC 3.
- This command is enabled only when no data exists in the print buffer. When data exists in the print buffer, the printer processes the data following m as normal data.
- After printing bar code, this command sets the print position to the beginning of the line.
- This command is not affected by print modes (emphasized, double-strike, underline, character size, white/black reverse printing, or 90-degree rotated character, etc.), except for upside-down printing mode.
- For Form 1, data ends with a NUL code. For UPC-A/UPC-E the printer prints after receiving 12 bytes; for JAN13 (EAN13) after 13 bytes; for JAN8 (EAN8) after 8 bytes.
- For ITF bar code, the number of data must be even numbers. When an odd number of data is input, the printer ignores the last received data.
- For CODE128: barcode data string head shall be required to select the character set encoding (CODEA, CODEB, or CODEC). Special characters are defined using "{" combined with a character (e.g., {A=CODEA, {B=CODEB, {C=CODEC, {S=SHIFT, {1=FNC1, {2=FNC2, {3=FNC3, {4=FNC4, {{=literal "{").

---

## QR Codes

### ESC Z m n k dL dH d1...dn — Print QR code

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 5A m n k dL dH d1...dn`   |
| Decimal    | `27 90 m n k dL dH d1...dn`   |
| ASCII      | `ESC Z m n k dL dH d1...dn`   |

**Parameters:**
- m (byte, 0 <= m <= 40): QR code version. 1-40 specifies a fixed version. 0 = Auto size (printer automatically selects the version).
- n (byte): Error correction level.
  - L: 7% recovery.
  - M: 15% recovery.
  - Q: 25% recovery.
  - H: 30% recovery.
- k (byte, 1 <= k <= 8): Component type (module size).
- dL (byte): Low byte of data length.
- dH (byte): High byte of data length. Total data length d = dL + dH * 256.
- d1...dn (bytes): QR code data.

**Behavior:**
Prints a QR code with the specified version, error correction level, module size, and data. When m is 0, the printer automatically selects the bar code type (version). The automatic method is recommended.

**Constraints / Notes:**
- QR code version capacity by EC level (number of data characters):
  - Version 1: L=19, M=16, Q=13, H=9.
  - Version 2: L=34, M=28, Q=22, H=16.
  - Version 3: L=55, M=44, Q=34, H=26.
  - Version 4: L=80, M=64, Q=48, H=36.
  - Version 5: L=108, M=86, Q=62, H=46.
  - Version 6: L=136, M=108, Q=76, H=60.
  - Version 7: L=156, M=124, Q=88, H=66.
  - Version 8: L=194, M=154, Q=110, H=86.
  - Version 9: L=232, M=182, Q=132, H=100.
  - Version 10: L=274, M=216, Q=154, H=122.
  - Version 11: L=324, M=254, Q=180, H=140.
  - Version 12: L=370, M=290, Q=206, H=158.
  - Version 13: L=428, M=334, Q=244, H=180.
  - Version 14: L=461, M=365, Q=261, H=197.
  - Version 15: L=523, M=415, Q=195, H=223.
  - Version 16: L=589, M=453, Q=325, H=253.
  - Version 17: L=647, M=507, Q=367, H=283.
  - Version 18: L=721, M=563, Q=397, H=313.
  - Version 19: L=795, M=627, Q=445, H=341.

---

## Code Pages

### ESC t n — Select character code table

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 74 n`                      |
| Decimal    | `27 116 n`                     |
| ASCII      | `ESC t n`                      |

**Parameters:**
- n (byte, 0 <= n <= 5, 16 <= n <= 19, or n = 255): Selects page n from the character code table.

**Behavior:**
Selects a page from the character code table. Default n=0.

---

## Miscellaneous

### ESC B n t — Set buzzer

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 42 n t`                    |
| Decimal    | `27 66 n t`                    |
| ASCII      | `ESC B n t`                    |

**Parameters:**
- n (byte, 1 <= n <= 9): Number of buzzer times.
- t (byte, 1 <= t <= 9): Buzzer interval; the buzzer beeps every [t * 100] milliseconds.

**Behavior:**
Activates the buzzer when printing the order. The buzzer rings n times with an interval of [t * 100] milliseconds between beeps.

---

### ESC c 5 n — Enable/disable panel buttons

| Field      | Value                          |
|------------|--------------------------------|
| Hex        | `1B 63 35 n`                   |
| Decimal    | `27 99 53 n`                   |
| ASCII      | `ESC c 5 n`                    |

**Parameters:**
- n (byte, 0 <= n <= 255): When the LSB of n is 0, the panel buttons are enabled. When the LSB of n is 1, the panel buttons are disabled.

**Behavior:**
Enables or disables the panel buttons. Only the lowest bit of n is valid. Default n=0 (enabled).

**Constraints / Notes:**
- When the panel buttons are disabled, none of them are usable when the printer cover is closed.
- In this printer, the panel buttons are the FEED button.
- In the macro ready mode, the FEED button is enabled regardless of the settings of this command; however, the paper cannot be fed by using these buttons.
