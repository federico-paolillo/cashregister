Implement one ESC/POS instruction in Cashregister.Printmon per the architecture 
in ESCPOS.md.

Take into account also the following files (read them):

- REVIEW.md
- AGENTS.md

## Instructions to implement

NAME: "ESC a n — Select justification"
CATEGORY FOLDER: Instructions/Layout/
BUILDER METHOD SIGNATURE: `.Justify(Justification justification)`

---

NAME: "ESC $ nL nH — Set absolute print position"
CATEGORY FOLDER: Instructions/Layout/
BUILDER METHOD SIGNATURE: `.SetAbsolutePosition(byte left, byte top)`

--- 

NAME: "ESC \ nL nH — Set relative print position"
CATEGORY FOLDER: Instructions/Layout/
BUILDER METHOD SIGNATURE: `.SetRelativePosition(byte left, byte top)`

---

NAME: "GS L nL nH — Set left margin"
CATEGORY FOLDER: Instructions/Layout/
BUILDER METHOD SIGNATURE: `.SetLeftMargin(byte amount)`

---

**Note**: Verify signatures are appropriate and suggest, if it exists, a better higher level signature

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
