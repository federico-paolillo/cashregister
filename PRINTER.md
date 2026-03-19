# Zjiang ZJ-8370 — 80mm Thermal Receipt Printer ESC/POS Command Reference

## Initialization

### ESC @ — Initialize printer

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 40`        |
| Decimal    | `27 64`        |
| ASCII      | `ESC @`        |

**Behavior:**
Clears the data in the print buffer and resets the printer mode to the mode that was in effect when the power was turned on. The DIP switch settings are not rechecked. The data in the receive buffer is not cleared. The macro definition is not cleared.

---

## Text Formatting

### ESC ! n — Select print mode(s)

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 21 n`      |
| Decimal    | `27 33 n`      |
| ASCII      | `ESC ! n`      |

**Parameters:**
- n (byte, 0-255): Bitmask selecting print modes.

| Bit | OFF/ON | Hex  | Decimal | Function                          |
|-----|--------|------|---------|-----------------------------------|
| 0   | OFF    | 00   | 0       | Character font A (12x24) selected |
| 0   | ON     | 01   | 1       | Character font B (9x17) selected  |
| 1   | -      | -    | -       | N/A                               |
| 2   | -      | -    | -       | N/A                               |
| 3   | OFF    | 00   | 0       | Emphasized mode not selected      |
| 3   | ON     | 08   | 8       | Emphasized mode selected          |
| 4   | OFF    | 00   | 0       | Double-height mode not selected   |
| 4   | ON     | 10   | 16      | Double-height mode selected       |
| 5   | OFF    | 00   | 0       | Double-width mode not selected    |
| 5   | ON     | 20   | 32      | Double-width mode selected        |
| 6   | -      | -    | -       | N/A                               |
| 7   | OFF    | 00   | 0       | Underline mode not selected       |
| 7   | ON     | 80   | 128     | Underline mode selected           |

**Behavior:**
Selects print mode(s) using the bit flags in n. When both double-height and double-width modes are selected, quadruple-size characters are printed. The underline thickness is that selected by ESC -, regardless of the character size. Default is n=0.

**Constraints / Notes:**
- The printer can underline all characters, but cannot underline the space set by HT or 90-degree clockwise rotated characters.
- ESC E can also turn on or off emphasized mode. However, the setting of the last received command is effective.
- ESC - can also turn on or off underline mode. However, the setting of the last received command is effective.
- GS ! can also select character size. However, the setting of the last received command is effective.
- Emphasized mode is effective for alphanumeric and Hanzi. All print modes except emphasized mode are effective only for alphanumeric.

---

### ESC E n — Turn emphasized mode on/off

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 45 n`      |
| Decimal    | `27 69 n`      |
| ASCII      | `ESC E n`      |

**Parameters:**
- n (byte, 0-255): When the LSB of n is 0, emphasized mode is turned off. When the LSB of n is 1, emphasized mode is turned on.

**Behavior:**
Turns emphasized (bold) mode on or off. Only the least significant bit of n is enabled. Default is n=0 (off).

**Constraints / Notes:**
- This command and ESC ! turn on and off emphasized mode in the same way. Be careful when this command is used with ESC !.

---

### ESC G n — Turn on/off double-strike mode

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 47 n`      |
| Decimal    | `27 71 n`      |
| ASCII      | `ESC G n`      |

**Parameters:**
- n (byte, 0-255): When the LSB of n is 0, double-strike mode is turned off. When the LSB of n is 1, double-strike mode is turned on.

**Behavior:**
Turns double-strike mode on or off. Only the lowest bit of n is enabled. Printer output is the same in double-strike mode and in emphasized mode. Default is n=0.

---

### ESC - n — Turn underline mode on/off

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 2D n`      |
| Decimal    | `27 45 n`      |
| ASCII      | `ESC - n`      |

**Parameters:**
- n (byte, 0-2 or 48-50): Selects underline mode.
  - 0, 48: Turns off underline mode
  - 1, 49: Turns on underline mode (1-dot thick)
  - 2, 50: Turns on underline mode (2-dots thick)

**Behavior:**
Turns underline mode on or off based on the value of n. The printer can underline all characters (including right-side character spacing), but cannot underline the space set by HT. Default is n=0.

**Constraints / Notes:**
- The printer cannot underline 90-degree clockwise rotated characters and white/black inverted characters.
- When underline mode is turned off by setting n to 0 or 48, the following data is not underlined, and the underline thickness set before the mode is turned off does not change. The default underline thickness is 1 dot.
- Changing the character size does not affect the current underline thickness.
- Underline mode can also be turned on or off by using ESC !. The last received command is effective.

---

### ESC M n — Select character font

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 4D n`      |
| Decimal    | `27 77 n`      |
| ASCII      | `ESC M n`      |

**Parameters:**
- n (byte, 0, 1, 48, 49): Selects the character font.
  - 0, 48: Character font A (12 x 24) selected
  - 1, 49: Character font B (9 x 17) selected

**Behavior:**
Selects one of the two built-in character fonts. ESC ! can also select the font type, but the last received command settings are effective.

---

### ESC V n — Turn 90-degree clockwise rotation mode on/off

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 56 n`      |
| Decimal    | `27 86 n`      |
| ASCII      | `ESC V n`      |

**Parameters:**
- n (byte, 0-1 or 48-49): Selects rotation mode.
  - 0, 48: Turns off 90-degree clockwise rotation mode
  - 1, 49: Turns on 90-degree clockwise rotation mode

**Behavior:**
Turns 90-degree clockwise rotation mode on or off. This command affects printing in standard mode; the setting is always effective. Default is n=0.

**Constraints / Notes:**
- When underline mode is turned on, the printer does not underline 90-degree clockwise-rotated characters.
- Double-width and double-height commands in 90-degree rotation mode enlarge characters in the opposite directions from double-height and double-width commands in normal mode.

---

### ESC { n — Turns on/off upside-down printing mode

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 7B n`      |
| Decimal    | `27 123 n`     |
| ASCII      | `ESC { n`      |

**Parameters:**
- n (byte, 0-255): When the LSB of n is 0, upside-down printing mode is turned off. When the LSB of n is 1, upside-down printing mode is turned on.

**Behavior:**
Turns upside-down printing mode on or off. Only the lowest bit of n is valid. This command is enabled only when processed at the beginning of a line in standard mode. In upside-down printing mode, the printer rotates the line to be printed by 180 degrees and then prints it. Default is n=0.

---

### GS ! n — Set character size

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 21 n`      |
| Decimal    | `29 33 n`      |
| ASCII      | `GS ! n`       |

**Parameters:**
- n (byte, 0-255): Bits 0-2 select character height (1x to 8x); Bits 4-6 select character width (1x to 8x).

Character width selection (bits 4-7):

| Hex | Decimal | Width    |
|-----|---------|----------|
| 00  | 0       | 1 (Normal) |
| 10  | 16      | 2 (Double-width) |
| 20  | 32      | 3        |
| 30  | 48      | 4        |
| 40  | 64      | 5        |
| 50  | 80      | 6        |
| 60  | 96      | 7        |
| 70  | 112     | 8        |

Character height selection (bits 0-3):

| Hex | Decimal | Height   |
|-----|---------|----------|
| 00  | 0       | 1 (Normal) |
| 01  | 1       | 2 (Double-height) |
| 02  | 2       | 3        |
| 03  | 3       | 4        |
| 04  | 4       | 5        |
| 05  | 5       | 6        |
| 06  | 6       | 7        |
| 07  | 7       | 8        |

**Behavior:**
Selects the character height using bits 0-2 and the character width using bits 4-6. This command is effective for all characters (alphanumeric and Kanji) except HRI characters. Default is n=0.

**Constraints / Notes:**
- If n is outside of the defined range, this command is ignored.
- In standard mode, the vertical direction is the paper feed direction, and the horizontal direction is perpendicular to the paper feed direction. However, when character orientation changes in 90-degree clockwise-rotation mode, the relationship between vertical and horizontal directions is reversed.
- When characters are enlarged with different sizes on one line, all the characters on the line are aligned at the baseline.
- The ESC ! command can also turn double-width and double-height modes on or off. However, the setting of the last received command is effective.

---

### GS B n — Turn white/black reverse printing mode

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 42 n`      |
| Decimal    | `29 66 n`      |
| ASCII      | `GS B n`       |

**Parameters:**
- n (byte, 0-255): When the LSB of n is 0, white/black reverse mode is turned off. When the LSB of n is 1, white/black reverse mode is turned on.

**Behavior:**
Turns white/black reverse printing mode on or off. Only the lowest bit of n is valid. This command is available for built-in characters and user-defined characters. Default is n=0.

**Constraints / Notes:**
- When white/black reverse printing mode is on, it also applies to character spacing set by ESC SP.
- This command does not affect bit image, user-defined bit image, bar code, HRI characters, and spacing skipped by HT, ESC $, and ESC \.
- This command does not affect the space between lines.
- White/black reverse mode has a higher priority than underline mode. Even if underline mode is on, it is disabled (but not canceled) when white/black reverse mode is selected.

---

## Text Alignment and Layout

### ESC a n — Select justification

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 61 n`      |
| Decimal    | `27 97 n`      |
| ASCII      | `ESC a n`      |

**Parameters:**
- n (byte, 0-2 or 48-50): Selects justification.
  - 0, 48: Left justification
  - 1, 49: Centering
  - 2, 50: Right justification

**Behavior:**
Aligns all the data in one line to the specified position. This command is enabled only when processed at the beginning of the line in standard mode. This command executes justification in the printing area. Default is n=0 (left).

**Constraints / Notes:**
- This command justifies the space area according to HT, ESC $ or ESC \.

---

### ESC $ nL nH — Set absolute print position

| Field      | Value              |
|------------|--------------------|
| Hex        | `1B 24 nL nH`     |
| Decimal    | `27 36 nL nH`     |
| ASCII      | `ESC $ nL nH`     |

**Parameters:**
- nL (byte, 0-255): Low byte of position.
- nH (byte, 0-255): High byte of position.

**Behavior:**
Sets the distance from the beginning of the line to the position at which subsequent characters are to be printed. The distance from the beginning of the line to the print position is [(nL + nH * 256) * (vertical or horizontal motion unit)] inches.

**Constraints / Notes:**
- Settings outside the specified printable area are ignored.
- In standard mode, the horizontal motion unit (x) is used.

---

### ESC \ nL nH — Set relative print position

| Field      | Value              |
|------------|--------------------|
| Hex        | `1B 5C nL nH`     |
| Decimal    | `27 92 nL nH`     |
| ASCII      | `ESC \ nL nH`     |

**Parameters:**
- nL (byte, 0-255): Low byte of relative distance.
- nH (byte, 0-255): High byte of relative distance.

**Behavior:**
Sets the print starting position based on the current position by using the horizontal or vertical motion unit. The distance from the current position is [(nL + nH * 256) * horizontal or vertical motion unit].

**Constraints / Notes:**
- Any setting that exceeds the printable area is ignored.
- When pitch N is specified to the right: nL + nH * 256 = N. When pitch N is specified to the left (the negative direction), use the complement of 65536.
- In standard mode, the horizontal motion unit is used.

---

### GS L nL nH — Set left margin

| Field      | Value              |
|------------|--------------------|
| Hex        | `1D 4C nL nH`     |
| Decimal    | `29 76 nL nH`     |
| ASCII      | `GS L nL nH`      |

**Parameters:**
- nL (byte, 0-255): Low byte of left margin.
- nH (byte, 0-255): High byte of left margin.

**Behavior:**
Sets the left margin using nL and nH. The left margin is set to [(nL + nH * 256) * 0.125 mm]. Default is nL=0, nH=0.

**Constraints / Notes:**
- This command is effective only when processed at the beginning of the line in standard mode.
- If the setting exceeds the printable area, the maximum value of the printable area is used.

---

### ESC SP n — Set right-side character spacing

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 20 n`      |
| Decimal    | `27 32 n`      |
| ASCII      | `ESC SP n`     |

**Parameters:**
- n (byte, 0-255): Sets the right-side character spacing to [n * 0.125 mm].

**Behavior:**
Sets the right-side character spacing. For double-width mode, the right-side character spacing is double than the normal mode. When the character is magnified, the right-side character spacing is n times than the normal mode. Default is n=0.

**Constraints / Notes:**
- This command does not affect the setting of Hanzi characters.
- The command sets the value of independent standard mode in each mode.

---

### HT — Horizontal tab

| Field      | Value  |
|------------|--------|
| Hex        | `09`   |
| Decimal    | `9`    |
| ASCII      | `HT`   |

**Behavior:**
Moves the print position to the next horizontal tab position. This command is ignored unless the next horizontal tab position has been set with ESC D.

**Constraints / Notes:**
- If the next horizontal tab position exceeds the printing area, the printer sets the printing position to [Printing area width + 1].
- Horizontal tab positions are set with ESC D.
- If this command is received when the printing position is at [printing area width + 1], the printer executes print buffer-full printing of the current line and horizontal tab processing from the beginning of the next line.

---

### ESC D n1...nk NUL — Set horizontal tab positions

| Field      | Value                  |
|------------|------------------------|
| Hex        | `1B 44 n1...nk 00`    |
| Decimal    | `27 68 n1...nk 0`     |
| ASCII      | `ESC D n1...nk NUL`   |

**Parameters:**
- n (byte, 1-255): Column number for setting a horizontal tab position from the beginning of the line.
- k (byte, 0-32): Total number of horizontal tab positions to be set.

**Behavior:**
Sets horizontal tab positions. n specifies the column number for setting a horizontal tab position from the beginning of the line. k indicates the total number of horizontal tab positions to be set.

**Constraints / Notes:**
- The horizontal tab position is stored as a value of [character width * n] measured from the beginning of the line. The character width includes the right-side character spacing, and double-width characters are set with twice the width of normal characters.
- This command cancels the previous horizontal tab settings.
- When setting n = 8, the print position is moved to column 9 by sending HT.
- Up to 32 tab positions (k = 32) can be set. Data exceeding 32 tab positions is processed as normal data.
- When [n] k is less than or equal to the preceding value [n] k-1, tab setting is finished and the following data is processed as normal data.
- ESC D NUL cancels all horizontal tab positions.
- The previously specified horizontal tab positions do not change, even if the character width changes.
- The default tab positions are at intervals of 8 characters (columns 9, 17, 25, ...) for font A (12x24).

---

## Feed and Motion

### LF — Print and line feed

| Field      | Value  |
|------------|--------|
| Hex        | `0A`   |
| Decimal    | `10`   |
| ASCII      | `LF`   |

**Behavior:**
Prints the data in the print buffer and feeds one line based on the current line spacing. This command sets the print position to the beginning of the line.

---

### ESC 2 — Select default line spacing

| Field      | Value      |
|------------|------------|
| Hex        | `1B 32`    |
| Decimal    | `27 50`    |
| ASCII      | `ESC 2`    |

**Behavior:**
Selects default line spacing of 3.75 mm (30 * 0.125 mm). The line spacing can be set independently in standard mode and in page mode.

---

### ESC 3 n — Set line spacing

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 33 n`      |
| Decimal    | `27 51 n`      |
| ASCII      | `ESC 3 n`      |

**Parameters:**
- n (byte, 0-255): Sets the line spacing to [n * 0.125 mm].

**Behavior:**
Sets the line spacing to [n * 0.125 mm]. The line spacing can be set independently in standard mode and in page mode. In standard mode, the vertical motion unit (y) is used. Default is n=30.

---

### ESC J n — Print and feed paper

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 4A n`      |
| Decimal    | `27 74 n`      |
| ASCII      | `ESC J n`      |

**Parameters:**
- n (byte, 0-255): Paper feed amount in [n * vertical or horizontal motion unit] inches.

**Behavior:**
Prints the data in the print buffer and feeds the paper [n * vertical or horizontal motion unit] inches. After printing is completed, this command sets the print starting position to the beginning of the line.

**Constraints / Notes:**
- The paper feed amount set by this command does not affect the values set by ESC 2 or ESC 3.
- In standard mode, the printer uses the vertical motion unit (y).

---

### ESC d n — Print and feed n lines

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 64 n`      |
| Decimal    | `27 100 n`     |
| ASCII      | `ESC d n`      |

**Parameters:**
- n (byte, 0-255): Number of lines to feed.

**Behavior:**
Prints the data in the print buffer and feeds n lines. This command sets the print starting position to the beginning of the line.

**Constraints / Notes:**
- This command does not affect the line spacing set by ESC 2 or ESC 3.
- The maximum paper feed amount is 1016 mm (40 inches). If the paper feed amount (n * line spacing) exceeds 1016 mm, the printer feeds the paper only 1016 mm.

---

## Cut

### ESC i — Part cutter (half cut)

| Field      | Value      |
|------------|------------|
| Hex        | `1B 69`    |
| Decimal    | `27 105`   |
| ASCII      | `ESC i`    |

**Behavior:**
Selects cut mode and performs a half cut (partial cut).

---

### ESC m — Partial cut

| Field      | Value      |
|------------|------------|
| Hex        | `1B 6D`    |
| Decimal    | `27 109`   |
| ASCII      | `ESC m`    |

**Behavior:**
Selects cut mode and performs a half cut (partial cut).

---

### GS V m / GS V m n — Select cut mode and cut paper

| Field      | Value                              |
|------------|------------------------------------|
| Hex        | `1D 56 m` or `1D 56 m n`          |
| Decimal    | `29 86 m` or `29 86 m n`          |
| ASCII      | `GS V m` or `GS V m n`            |

**Parameters:**
- m (byte): Cut mode selector.
  - 1, 49: Partial cut (one point left uncut) — format 1: `GS V m`
  - 66: Feeds paper to (cutting position + [n * vertical motion unit]) and cuts the paper partially (one point left uncut) — format 2: `GS V m n`
- n (byte, 0-255): Feed distance before cut (only for m=66).

**Behavior:**
Selects a mode for cutting paper and executes paper cutting. For m=1 or 49, a partial cut is performed immediately. For m=66, the printer feeds paper to the cutting position plus an offset of [n * vertical motion unit], then cuts partially.

**Constraints / Notes:**
- Only the partial cut is available; there is no full cut.
- This command is effective only when processed at the beginning of a line.
- Cutting state is different depending on the automatically loaded cutter type.
- When n = 0 (for m=66), the printer feeds the paper to the cutting position and cuts it.

---

## Drawer Kick

### DLE DC4 n m t — Real-time pulse generator

| Field      | Value              |
|------------|--------------------|
| Hex        | `10 14 n m t`      |
| Decimal    | `16 20 n m t`      |
| ASCII      | `DLE DC4 n m t`    |

**Parameters:**
- n: Fixed value, n=1.
- m (byte, 0-1): Connector pin selector.
  - 0: Cash Drawer Connect Pin 2
  - 1: Cash Drawer Connect Pin 5
- t (byte, 1-8): Pulse duration. Pulse high time is [t * 100 ms]; low time is [t * 100 ms].

**Behavior:**
Outputs a pulse to the specified connector pin for the specified duration. When the printer is already executing a command to open the cash drawer (ESC p or DLE DC4), the command is ignored.

**Constraints / Notes:**
- In serial mode, the printer immediately executes after receiving the order.
- In parallel mode, the printer is busy when the command is not executed.
- If the print data is the same as the command data, the data will be used as the command is executed. The user must take into account this situation.
- Do not try to insert the command in two or more bytes in the command sequence.
- Even if the printer is set to disabled by the command of ESC = (select peripheral), the order is still valid.

---

### ESC p m t1 t2 — Generate pulse

| Field      | Value              |
|------------|--------------------|
| Hex        | `1B 70 m t1 t2`   |
| Decimal    | `27 112 m t1 t2`  |
| ASCII      | `ESC p m t1 t2`   |

**Parameters:**
- m (byte, 0, 1, 48, 49): Connector pin selector.
  - 0, 48: Drawer kick-out connector pin 2
  - 1, 49: Drawer kick-out connector pin 5
- t1 (byte, 0-255): On time = t1 * 2 milliseconds.
- t2 (byte, 0-255): Off time = t2 * 2 milliseconds.

**Behavior:**
Sends a pulse to the specified connection pins. The on-time is t1 * 2 milliseconds, and the off-time is t2 * 2 milliseconds.

---

## Barcodes

### GS H n — Select printing position for HRI characters

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 48 n`      |
| Decimal    | `29 72 n`      |
| ASCII      | `GS H n`       |

**Parameters:**
- n (byte, 0-3 or 48-51): Selects the printing position of HRI characters.
  - 0, 48: Not printed
  - 1, 49: Above the bar code
  - 2, 50: Below the bar code
  - 3, 51: Both above and below the bar code

**Behavior:**
Selects the printing position of HRI (Human Readable Interpretation) characters when printing a bar code. HRI characters are printed using the font specified by GS f. Default is n=0.

---

### GS f n — Select font for HRI characters

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 66 n`      |
| Decimal    | `29 102 n`     |
| ASCII      | `GS f n`       |

**Parameters:**
- n (byte, 0, 1, 48, 49): Selects the HRI font.
  - 0, 48: Font A (12x24)
  - 1, 49: Font B (9x17)

**Behavior:**
Selects a font for the HRI characters used when printing a bar code. HRI characters are printed at the position specified by GS H. Default is n=0.

---

### GS h n — Set bar code height

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 68 n`      |
| Decimal    | `29 104 n`     |
| ASCII      | `GS h n`       |

**Parameters:**
- n (byte, 1-255): Height of the bar code in dots (vertical direction).

**Behavior:**
Selects the height of the bar code. n specifies the number of dots in the vertical direction. Default is n=162.

---

### GS w n — Set bar code width

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 77 n`      |
| Decimal    | `29 119 n`     |
| ASCII      | `GS w n`       |

**Parameters:**
- n (byte, 2-6): Sets the horizontal size of the bar code.

| n | Module Width (mm) Multi-level | Thin element (mm) Binary-level | Thick element (mm) Binary-level |
|---|-------------------------------|--------------------------------|---------------------------------|
| 2 | 0.250                         | 0.250                          | 0.625                           |
| 3 | 0.375                         | 0.375                          | 1.000                           |
| 4 | 0.560                         | 0.500                          | 1.250                           |
| 5 | 0.625                         | 0.625                          | 1.625                           |
| 6 | 0.750                         | 0.750                          | 2.000                           |

**Behavior:**
Sets the horizontal size of the bar code. Multi-level bar codes: UPC-A, UPC-E, JAN13 (EAN13), JAN8 (EAN8), CODE93, CODE128. Binary-level bar codes: CODE39, ITF, CODABAR. Default is n=3.

---

### GS x n — Set the left pitch of bar code printing

| Field      | Value          |
|------------|----------------|
| Hex        | `1D 78 n`      |
| Decimal    | `29 120 n`     |
| ASCII      | `GS x n`       |

**Parameters:**
- n (byte, 0-255): Bar code starting position from the left.

**Behavior:**
Sets the bar code print starting position from the left margin.

---

### GS k m d1...dk NUL / GS k m n d1...dn — Print barcode

| Field      | Value                                        |
|------------|----------------------------------------------|
| Hex        | `1D 6B m d1...dk 00` or `1D 6B m n d1...dn` |
| Decimal    | `29 107 m d1...dk 0` or `29 107 m n d1...dn`|
| ASCII      | `GS k m d1...dk NUL` or `GS k m n d1...dn`  |

**Parameters (Format 1 — NUL terminated):**
- m (byte, 0-6): Bar code system selector.
- d1...dk: Bar code data bytes, terminated by NUL (0x00).

**Parameters (Format 2 — length prefixed):**
- m (byte, 65-73): Bar code system selector.
- n (byte): Number of bar code data bytes.
- d1...dn: Bar code data bytes.

Bar code system selection:

| m (fmt 1) | m (fmt 2) | System       | Characters (fmt 1) | Characters (fmt 2) | Data range                         |
|------------|-----------|--------------|--------------------|--------------------|-------------------------------------|
| 0          | 65        | UPC-A        | 11 <= k <= 12      | 11 <= n <= 12       | 48 <= d <= 57                       |
| 1          | 66        | UPC-E        | 11 <= k <= 12      | 11 <= n <= 12       | 48 <= d <= 57                       |
| 2          | 67        | JAN13 (EAN13)| 12 <= k <= 13      | 12 <= n <= 13       | 48 <= d <= 57                       |
| 3          | 68        | JAN8 (EAN8)  | 7 <= k <= 8        | 7 <= n <= 8         | 48 <= d <= 57                       |
| 4          | 69        | CODE39       | 1 <= k             | 1 <= n <= 255       | 48-57, 65-90, 32, 36-37, 43, 45-47 |
| 5          | 70        | ITF          | 1 <= k (even)      | 1 <= n <= 255 (even)| 48 <= d <= 57                       |
| 6          | 71        | CODABAR      | 1 <= k             | 1 <= n <= 255       | 48-57, 65-68, 36, 43, 45-47, 58    |
| -          | 72        | CODE93       | -                  | 1 <= n <= 255       | 0 <= d <= 127                       |
| -          | 73        | CODE128      | -                  | 2 <= n <= 255       | 0 <= d <= 127                       |

**Behavior:**
Selects a bar code system and prints the bar code. Format 1 uses NUL-terminated data and is available for m=0-6. Format 2 uses a length prefix and is available for m=65-73.

**Constraints / Notes:**
- Format 1: The command ends with a NUL code. For UPC-A/UPC-E, the printer prints after receiving 12 bytes. For JAN13, after 13 bytes. For JAN8, after 8 bytes.
- Format 2: n indicates the number of bar code data bytes. If n is outside of the specified range, the printer stops command processing and processes the following data as normal data.
- If d is outside of the specified range, the printer only feeds paper and processes the following data as normal data.
- If the horizontal size exceeds the printing area, the printer only feeds the paper.
- This command feeds as much paper as is required to print the bar code, regardless of the line spacing specified by ESC 2 or ESC 3.
- This command is enabled only when no data exists in the print buffer. When data exists, the data following m is processed as normal data.
- After printing bar code, this command sets the print position to the beginning of the line.
- This command is not affected by print modes (emphasized, double-strike, underline, character size, white/black reverse printing, or 90-degree rotated character, etc.), except for upside-down printing mode.
- The number of data for ITF bar code must be even numbers. When an odd number of data is input, the printer ignores the last received data.
- When using CODE128, the barcode data string head shall be required to select the character set encoding (CODEA, CODEB, or CODEC). Special characters for CODE128 include: {S (SHIFT), {A (CODEA), {B (CODEB), {C (CODEC), {1 (FNC1), {2 (FNC2), {3 (FNC3), {4 (FNC4), {{ (literal "{").

---

## QR Codes

### ESC Z m n k dL dH d1...dn — Print QR Code

| Field      | Value                        |
|------------|------------------------------|
| Hex        | `1B 5A m n k dL dH d1...dn` |
| Decimal    | `27 90 m n k dL dH d1...dn` |
| ASCII      | `ESC Z m n k dL dH d1...dn` |

**Parameters:**
- m (byte, 0-40): QR code version (1-40 for specific version, 0 for auto size).
- n (byte): Error correction level.
  - L: 7% recovery
  - M: 15% recovery
  - Q: 25% recovery
  - H: 30% recovery
- k (byte, 1-8): Component type (module size).
- dL (byte): Low byte of data length.
- dH (byte): High byte of data length. Total data length = dL + dH * 256.
- d1...dn: QR code data.

**Behavior:**
Prints a QR code with the specified version, error correction level, module size, and data. When m is 0, the printer automatically selects the bar code type (version).

**Constraints / Notes:**
- QR code capacity by version and EC level (numeric characters):

| Version | L (7%) | M (15%) | Q (25%) | H (30%) |
|---------|--------|---------|---------|---------|
| 1       | 19     | 16      | 13      | 9       |
| 2       | 34     | 28      | 22      | 16      |
| 3       | 55     | 44      | 34      | 26      |
| 4       | 80     | 64      | 48      | 36      |
| 5       | 108    | 86      | 62      | 46      |
| 6       | 136    | 108     | 76      | 60      |
| 7       | 156    | 124     | 88      | 66      |
| 8       | 194    | 154     | 110     | 86      |
| 9       | 232    | 182     | 132     | 100     |
| 10      | 274    | 216     | 154     | 122     |
| 11      | 324    | 254     | 180     | 140     |
| 12      | 370    | 290     | 206     | 158     |
| 13      | 428    | 334     | 244     | 180     |
| 14      | 461    | 365     | 261     | 197     |
| 15      | 523    | 415     | 295     | 223     |
| 16      | 589    | 453     | 325     | 253     |
| 17      | 647    | 507     | 367     | 283     |
| 18      | 721    | 563     | 397     | 313     |
| 19      | 795    | 627     | 445     | 341     |

---

## Code Pages

### ESC t n — Select character code table

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 74 n`      |
| Decimal    | `27 116 n`     |
| ASCII      | `ESC t n`      |

**Parameters:**
- n (byte, 0-5, 16-19, or 255): Selects a page n from the character code table.

**Behavior:**
Selects a character code page. Default is n=0. The available code pages correspond to the extended character table in the product specifications (OEM437, Katakana, OEM850, OEM860, OEM863, OEM865, WestEurope, Greek, Hebrew, EastEurope, Iran, WPC1252, OEM866, OEM852, OEM858, IranII, Latvian, Arabic, PT151, 1251, OEM747, WPC1257, Vietnam, OEM864, Hebrew, WPC1255, Thai).

**Constraints / Notes:**
- Code page selection resets on power cycle (volatile setting). Send ESC t at the start of every print session if a non-default code page is required.

---

## Panel Control

### ESC c 5 n — Enable/disable panel buttons

| Field      | Value              |
|------------|--------------------|
| Hex        | `1B 63 35 n`       |
| Decimal    | `27 99 53 n`       |
| ASCII      | `ESC c 5 n`        |

**Parameters:**
- n (byte, 0-255): When the LSB of n is 0, the panel buttons are enabled. When the LSB of n is 1, the panel buttons are disabled.

**Behavior:**
Enables or disables the panel buttons. Only the lowest bit of n is valid. In this printer, the panel buttons are the FEED button. Default is n=0 (enabled).

**Constraints / Notes:**
- When the panel buttons are disabled, none of them are usable when the printer cover is closed.
- In the macro ready mode, the FEED button is enabled regardless of the settings of this command; however, the paper cannot be fed by using these buttons.

---

## Buzzer

### ESC B n t — Set buzzer

| Field      | Value          |
|------------|----------------|
| Hex        | `1B 42 n t`    |
| Decimal    | `27 66 n t`    |
| ASCII      | `ESC B n t`    |

**Parameters:**
- n (byte, 1-9): Number of buzzer times.
- t (byte, 1-9): Buzzer interval. The buzzer beeps every [t * 100] milliseconds.

**Behavior:**
Activates the buzzer when printing the order. n refers to the number of buzzer times. t refers to the buzzer beep interval of every [t * 100] milliseconds.

---

## Miscellaneous

### GS W nL nH — Set printing area width

This command is referenced by GS L as its companion. Together they define the printable region.

| Field      | Value              |
|------------|--------------------|
| Hex        | `1D 57 nL nH`     |
| Decimal    | `29 87 nL nH`     |
| ASCII      | `GS W nL nH`      |

**Parameters:**
- nL (byte, 0-255): Low byte of printing area width.
- nH (byte, 0-255): High byte of printing area width.

**Behavior:**
Sets the printing area width. The width is set to [(nL + nH * 256) * 0.125 mm]. Used in conjunction with GS L (set left margin) to define the printable region.

---

### FS p n m — Print NV bit image

| Field      | Value          |
|------------|----------------|
| Hex        | `1C 70 n m`    |
| Decimal    | `28 112 n m`   |
| ASCII      | `FS p n m`     |

**Parameters:**
- n (byte, 1-255): NV bit image number.
- m (byte, 0-3 or 48-51): Print mode.
  - 0, 48: Normal (203.2 dpi vertical, 203.2 dpi horizontal)
  - 1, 49: Double-width (203.2 dpi vertical, 101.6 dpi horizontal)
  - 2, 50: Double-height (101.6 dpi vertical, 203.2 dpi horizontal)
  - 3, 51: Quadruple (101.6 dpi vertical, 101.6 dpi horizontal)

**Behavior:**
Prints a NV bit image n using the mode specified by m. NV bit image means a bit image which is defined in a non-volatile memory by FS q and printed by FS p.

**Constraints / Notes:**
- This command is not effective when the specified NV bit image has not been defined.
- In standard mode, this command is effective only when there is no data in the print buffer.
- This command is not affected by print modes (emphasized, double-strike, underline, character size, white/black reverse printing, or 90-degree rotated characters, etc.), except upside-down printing mode.
- After printing the bit image, this command sets the print position to the beginning of the line and processes the data that follows as normal data.

---

### FS q n [xL xH yL yH d1...dk]1...[xL xH yL yH d1...dk]n — Define NV bit image

| Field      | Value                                          |
|------------|------------------------------------------------|
| Hex        | `1C 71 n [xLxHyLyHd1...dk]1...[xLxHyLyHd1...dk]n` |
| Decimal    | `28 113 n [xLxHyLyHd1...dk]1...[xLxHyLyHd1...dk]n` |
| ASCII      | `FS q n [xLxHyLyHd1...dk]1...[xLxHyLyHd1...dk]n`   |

**Parameters:**
- n (byte, 1-255): Number of NV bit images to define.
- xL (byte, 0-255): Low byte of horizontal size.
- xH (byte, 0-3): High byte of horizontal size. Horizontal size in dots = (xL + xH * 256) * 8, max 1023.
- yL (byte, 0-255): Low byte of vertical size.
- yH (byte, 0-1): High byte of vertical size. Vertical size in dots = (yL + yH * 256) * 8, max 288.
- d (byte, 0-255): Definition data. k = (xL + xH * 256) * (yL + yH * 256) * 8.
- Total defined data area = 192K bytes.

**Behavior:**
Defines one or more NV bit images. After writing to NV memory, the printer performs a hardware reset. User-defined characters, download bitmap, and macros should be redefined after the completion of the command.

**Constraints / Notes:**
- Frequently writing to NV memory may damage it. It is recommended to perform no more than 10 writes per day.
- This command cancels all previously defined NV bitmaps.
- During processing, the printer is BUSY and stops receiving data.
- The definition area in this printer is a maximum of 192K bytes.
- Once a NV bit image is defined, it is not erased by performing ESC @, reset, or power-off.
- This command performs only definition of a NV bit image and does not perform printing. Printing is performed by the FS p command.
