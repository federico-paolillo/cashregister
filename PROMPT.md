Implement one ESC/POS instruction in Cashregister.Printmon per the architecture 
in ESCPOS.md.

Take into account also the following files (read them):

- REVIEW.md
- AGENTS.md

## Instructions to implement

NAME: "ESC t n — Select character code table"
CATEGORY FOLDER: Instructions/Core/
BUILDER METHOD SIGNATURE: None. When ProgramBuilder is initialized it should include this instruction (like initialize) with value 0 (european code page).

---

NAME: "ESC m — Partial cut"
CATEGORY FOLDER: Instructions/Core/
BUILDER METHOD SIGNATURE: None. When ProgramBuilder.Build is called, ProgramBuilder will include this instruction before returning the program.

--- 

NAME: "LF — Print and line feed"
CATEGORY FOLDER: Instructions/Core/
BUILDER METHOD SIGNATURE: `.LineFeed()` AND it is automatically included when ProgramBuilder.Build is called. Before including the PartialCut instruction.

---

**Note**: Verify signatures are appropriate and suggest, if it exists, a better higher level signature.

Review existing tests and update them to account for the new automatically included instructions at init and build time of ProgramBuilder.

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
