Implement one ESC/POS instruction in Cashregister.Printmon per the architecture 
in ESCPOS.md.

Take into account also the following files (read them):

- REVIEW.md for some open points on the architecture of the application
- AGENTS.md general context and fundamental information
- MANUAL.md printer manual
- PRINTER.md printer programmer manual

## Instructions to implement

NAME: "GS V m / GS V m n — Select cut mode and cut paper"
CATEGORY FOLDER: Instructions/Core/
BUILDER METHOD SIGNATURE: `.CutAfter(byte distance)`. We will only offer partial cut mode 66 with an optional feed distance.

Given that `GS V m n` commands are superior to the `ESC m` counterpart, we should refactor the PrintProgramBuilder to auto-include a `LF` and then a `GS V m n` that feeds 1 lines at the end of each program. We need an LF because `GS V m n` says "This command is effective only when processed at the beginning of a line.".

---

Verify signatures are appropriate and suggest, if it exists, a better higher level signature. Ensure implementations do not contradict information reported in the printer manual and printer programmer manual. 

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