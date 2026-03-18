using System.Collections.Immutable;

using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;

namespace Cashregister.Printmon;

/// <summary>
/// A builder pattern implementation that helps you build valid <see cref="PrintProgram"/><br/>
/// This class is single-use. Once you build the <see cref="PrintProgram"/> the builder is frozen.
/// </summary>
public sealed class PrintProgramBuilder
{
    private bool frozen;

    private readonly List<Instruction> instructions = [new InitializeInstruction()];

    public PrintProgramBuilder NoOp()
    {
        AddInstruction(new NoOpInstruction());

        return this;
    }

    public PrintProgramBuilder UseFontA(FormatMode formatMode)
    {
        AddInstruction(new SelectPrintModeInstruction(false, formatMode));

        return this;
    }

    public PrintProgramBuilder UseFontB(FormatMode formatMode)
    {
        AddInstruction(new SelectPrintModeInstruction(true, formatMode));

        return this;
    }

    private void AddInstruction(Instruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        
        if (frozen)
        {
            throw new InvalidOperationException("This builder has already emitted its program");
        } 
        
        instructions.Add(instruction);
    }
    
    public PrintProgram Build()
    {
        frozen = true;

        return new PrintProgram([..instructions]);
    }
}