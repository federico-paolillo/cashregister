Implement one ESC/POS instruction in Cashregister.Printmon per the architecture 
in ESCPOS.md.

Take into account also the following files in the project root (read them):

- REVIEW.md for some open points on the architecture of the application
- AGENTS.md general context and fundamental information
- MANUAL.md printer manual
- PRINTER.md printer programmer manual

## Instructions to implement

NAME: "ESC - n — Turn underline mode on/off"
CATEGORY FOLDER: Instructions/Formatting
BUILDER METHOD SIGNATURE: `.UnderlineOn()` and `.UnderlineOff()`. 

---

NAME: "GS B n — Turn white/black reverse printing mode"
CATEGORY FOLDER: Instructions/Formatting
BUILDER METHOD SIGNATURE: `.ReverseOn()` and `ReverseOff()`

---

NAME: "ESC SP n — Set right-side character spacing"
CATEGORY FOLDER: Instructions/Layout
BUILDER METHOD SIGNATURE: `SetRightSpacing(byte spacing)`

---

NAME: "ESC D n1...nk NUL — Set horizontal tab positions"
CATEGORY FOLDER: Instructions/Motion
BUILDER METHOD SIGNATURE: `.SetHorizontalTabs(<propose parameters>)`

---

NAME: "ESC 2 — Select default line spacing"
CATEGORY FOLDER: Instructions/Core
BUILDER METHOD SIGNATURE: `.ResetLineSpacing()`

---

Verify signatures and category folders are appropriate and suggest, if it exists, a better alternative. Ensure implementations do not contradict information reported in the printer manual and printer programmer manual. 

## ESC/POS manual reference

Review the file PRINTER.md

## Files to create or modify

CREATE:  Instructions/[Category]/[Name]Instruction.cs - Keep any enum in the same file as the instruction
MODIFY:  PrintProgramBuilder.cs  — add one builder method  
MODIFY:  Encoders/BinaryEncoder.cs  — add one switch arm before the default throw  
MODIFY:  Encoders/StringEncoder.cs  — add one switch arm before the default throw  

If BinaryEncoder.cs or StringEncoder.cs do not yet exist, create them with 
the full boilerplate switch structure per CLAUDE.md, then add the new case.

## Constraints

- Read all existing files before writing. Do not regenerate content that 
  already exists; only add the delta.
- Validate parameter ranges in the instruction record if the manual specifies them. Use ArgumentOutOfRangeException with a descriptive message.