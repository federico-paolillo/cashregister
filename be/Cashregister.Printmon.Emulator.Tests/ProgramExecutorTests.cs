using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class ProgramExecutorTests
{
    private static readonly ProgramExecutor Executor = new(new InstructionDecoder(), new InstructionExecutor());

    private static byte[] BuildBytes(Action<PrintProgramBuilder> configure)
    {
        var builder = new PrintProgramBuilder();
        configure(builder);
        return new BinaryEncoder().Encode(builder.Build()).Value!;
    }

    // ---- ExecuteWithHistory ----

    [Fact]
    public void ExecuteWithHistory_EmptyInstructionList_ReturnsEmptyHistory()
    {
        var history = Executor.ExecuteWithHistory([]);

        Assert.Empty(history);
    }

    [Fact]
    public void ExecuteWithHistory_ReturnsOneDocumentPerInstruction()
    {
        var instructions = new Instruction[]
        {
            new InitializeInstruction(),
            new EmphasizeInstruction(true),
            new TextInstruction("hi")
        };

        var history = Executor.ExecuteWithHistory(instructions);

        Assert.Equal(3, history.Length);
    }

    [Fact]
    public void ExecuteWithHistory_StateProgresses_BoldBecomesTrue()
    {
        var instructions = new Instruction[]
        {
            new EmphasizeInstruction(true),
            new TextInstruction("bold")
        };

        var history = Executor.ExecuteWithHistory(instructions);

        // After instruction 0 (EmphasizeOn), state.Bold should be true
        Assert.True(history[0].State.Bold);
        // After instruction 1 (Text), state.Bold still true
        Assert.True(history[1].State.Bold);
    }

    [Fact]
    public void ExecuteWithHistory_WithCustomInitialState_UsesProvidedState()
    {
        var initialState = PrinterState.Default with { Bold = true };
        var instructions = new Instruction[] { new TextInstruction("x") };

        var history = Executor.ExecuteWithHistory(instructions, initialState);

        var span = Assert.IsType<TextSpan>(Assert.Single(history[0].Elements));
        Assert.True(span.Style.Bold);
    }

    [Fact]
    public void ExecuteWithHistory_TimeTravelIndex_ReturnsCorrectIntermediateState()
    {
        var instructions = new Instruction[]
        {
            new EmphasizeInstruction(true),
            new JustifyInstruction(Alignment.Center),
            new EmphasizeInstruction(false)
        };

        var history = Executor.ExecuteWithHistory(instructions);

        // At step 0: bold=true, justification=left
        Assert.True(history[0].State.Bold);
        Assert.Equal(Alignment.Left, history[0].State.Justification);

        // At step 1: bold=true, justification=center
        Assert.True(history[1].State.Bold);
        Assert.Equal(Alignment.Center, history[1].State.Justification);

        // At step 2: bold=false, justification=center
        Assert.False(history[2].State.Bold);
        Assert.Equal(Alignment.Center, history[2].State.Justification);
    }

    // ---- Execute (full pipeline) ----

    [Fact]
    public void Execute_EmptyProgram_ProducesLineBreakAndHorizontalRule()
    {
        var bytes = BuildBytes(_ => { });

        var document = Executor.Execute(bytes);

        // Empty program preamble+postamble: Init, CodePage, ResetPrintMode produce no elements
        // LineFeed -> LineBreak, CutAfter -> HorizontalRule
        Assert.Equal(2, document.Elements.Length);
        Assert.IsType<LineBreak>(document.Elements[0]);
        Assert.IsType<HorizontalRule>(document.Elements[1]);
    }

    [Fact]
    public void Execute_TextProgram_ProducesTextSpanLineBreakHorizontalRule()
    {
        var bytes = BuildBytes(b => b.Text("Hello"));

        var document = Executor.Execute(bytes);

        // TextSpan("Hello") + LineBreak (postamble LF) + HorizontalRule (postamble CutAfter)
        Assert.Equal(3, document.Elements.Length);
        var span = Assert.IsType<TextSpan>(document.Elements[0]);
        Assert.Equal("Hello", span.Text);
        Assert.IsType<LineBreak>(document.Elements[1]);
        Assert.IsType<HorizontalRule>(document.Elements[2]);
    }

    [Fact]
    public void Execute_BoldText_TextSpanHasBoldStyle()
    {
        var bytes = BuildBytes(b => b.BoldOn().Text("bold"));

        var document = Executor.Execute(bytes);

        var span = Assert.IsType<TextSpan>(document.Elements[0]);
        Assert.Equal("bold", span.Text);
        Assert.True(span.Style.Bold);
    }

    [Fact]
    public void Execute_CenteredText_TextSpanHasCenterJustification()
    {
        var bytes = BuildBytes(b => b.Align(Alignment.Center).Text("center"));

        var document = Executor.Execute(bytes);

        var span = Assert.IsType<TextSpan>(document.Elements[0]);
        Assert.Equal(Alignment.Center, span.Style.Justification);
    }

    [Fact]
    public void Execute_MultiLinePrintLine_ProducesAlternatingSpansAndLineBreaks()
    {
        var bytes = BuildBytes(b => b.PrintLine("line1").PrintLine("line2"));

        var document = Executor.Execute(bytes);

        // PrintLine = Text + LineFeed
        // line1: TextSpan + LineBreak
        // line2: TextSpan + LineBreak
        // Postamble: LineBreak + HorizontalRule
        // "line1line2" are adjacent ASCII, but separated by 0x0A (LF) so they can't merge
        Assert.IsType<TextSpan>(document.Elements[0]);
        Assert.IsType<LineBreak>(document.Elements[1]);
        Assert.IsType<TextSpan>(document.Elements[2]);
        Assert.IsType<LineBreak>(document.Elements[3]);
    }

    [Fact]
    public void Execute_RealProgram_ProducesExpectedElementCount()
    {
        var bytes = BuildBytes(b => b
            .Align(Alignment.Center)
            .BoldOn()
            .Text("RECEIPT")
            .BoldOff()
            .LineFeed()
            .Text("Item 1")
            .LineFeed()
            .FeedLines(2));

        var document = Executor.Execute(bytes);

        // Align: no element
        // BoldOn: no element
        // Text("RECEIPT"): TextSpan
        // BoldOff: no element
        // LineFeed: LineBreak
        // Text("Item 1"): TextSpan
        // LineFeed: LineBreak
        // FeedLines(2): FeedLines
        // Postamble LF: LineBreak
        // Postamble CutAfter: HorizontalRule
        Assert.Equal(7, document.Elements.Length);
    }
}
