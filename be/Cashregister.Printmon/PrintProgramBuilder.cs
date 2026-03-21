using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;

namespace Cashregister.Printmon;

/// <summary>
///     A builder pattern implementation that helps you build valid <see cref="PrintProgram" /><br />
///     This class is single-use. Once you build the <see cref="PrintProgram" /> the builder is frozen.
/// </summary>
public sealed class PrintProgramBuilder
{
    private readonly List<Instruction> instructions = [new InitializeInstruction(), new SelectCodeTableInstruction(), new ResetPrintModeInstruction()];
    private bool frozen;

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

    public PrintProgramBuilder UnderlineOn(Thickness thickness)
    {
        AddInstruction(new UnderlineInstruction(true, thickness));

        return this;
    }

    public PrintProgramBuilder UnderlineOff()
    {
        AddInstruction(new UnderlineInstruction(false, default));

        return this;
    }

    public PrintProgramBuilder EmphasizeOn()
    {
        AddInstruction(new EmphasizeInstruction(true));

        return this;
    }

    public PrintProgramBuilder EmphasizeOff()
    {
        AddInstruction(new EmphasizeInstruction(false));

        return this;
    }

    public PrintProgramBuilder DoubleStrikeOn()
    {
        AddInstruction(new DoubleStrikeInstruction(true));

        return this;
    }

    public PrintProgramBuilder DoubleStrikeOff()
    {
        AddInstruction(new DoubleStrikeInstruction(false));

        return this;
    }

    public PrintProgramBuilder SelectFontA()
    {
        AddInstruction(new SelectFontInstruction(CharacterFont.A));

        return this;
    }

    public PrintProgramBuilder SelectFontB()
    {
        AddInstruction(new SelectFontInstruction(CharacterFont.B));

        return this;
    }

    public PrintProgramBuilder NinetyDegsOn()
    {
        AddInstruction(new RotationInstruction(true));

        return this;
    }

    public PrintProgramBuilder NinetyDegsOff()
    {
        AddInstruction(new RotationInstruction(false));

        return this;
    }

    public PrintProgramBuilder UpsideDownOn()
    {
        AddInstruction(new UpsideDownInstruction(true));

        return this;
    }

    public PrintProgramBuilder UpsideDownOff()
    {
        AddInstruction(new UpsideDownInstruction(false));

        return this;
    }

    public PrintProgramBuilder FontSize(byte size)
    {
        AddInstruction(new FontSizeInstruction(size));

        return this;
    }

    public PrintProgramBuilder Justify(Justification justification)
    {
        AddInstruction(new JustifyInstruction(justification));

        return this;
    }

    public PrintProgramBuilder SetAbsolutePosition(ushort position)
    {
        AddInstruction(new AbsolutePositionInstruction(position));

        return this;
    }

    public PrintProgramBuilder SetRelativePosition(ushort offset)
    {
        AddInstruction(new RelativePositionInstruction(offset));

        return this;
    }

    public PrintProgramBuilder SetLeftMargin(ushort margin)
    {
        AddInstruction(new LeftMarginInstruction(margin));

        return this;
    }

    public PrintProgramBuilder Text(string text)
    {
        AddInstruction(new TextInstruction(text));

        return this;
    }

    public PrintProgramBuilder HorizontalTab()
    {
        AddInstruction(new HorizontalTabInstruction());

        return this;
    }

    public PrintProgramBuilder LineFeed()
    {
        AddInstruction(new LineFeedInstruction());

        return this;
    }

    public PrintProgramBuilder PrintLine(string text)
    {
        return Text(text).LineFeed();
    }

    public PrintProgramBuilder CutAfter(byte distance)
    {
        AddInstruction(new CutAfterInstruction(distance));

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
        AddInstruction(new LineFeedInstruction());
        AddInstruction(new CutAfterInstruction(1));

        frozen = true;

        return new PrintProgram([.. instructions]);
    }
}