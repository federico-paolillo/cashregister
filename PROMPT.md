Implement one ESC/POS instruction in Cashregister.Printmon per the architecture 
in ESCPOS.md.

Take into account also the following files (read them):

- REVIEW.md
- AGENTS.md

## Instructions to implement

NAME: There is no name here. We need the actual facility to put characters to print in the print buffer
CATEGORY FOLDER: Instructions/Core/
BUILDER METHOD SIGNATURE: `.Text(string text)`


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
