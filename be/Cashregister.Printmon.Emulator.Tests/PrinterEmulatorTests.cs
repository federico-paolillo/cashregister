using System.Collections.Immutable;

using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class PrinterEmulatorTests
{
    private static readonly PrinterEmulator Emulator = new(new InstructionDecoder(), new InstructionExecutor());

    // Unwraps the result for happy-path tests
    private static ImmutableArray<Printer> Emulate(byte[] bytes) =>
        Emulator.Emulate(bytes).Value!;

    private static byte[] BuildBytes(Action<PrintProgramBuilder> configure)
    {
        var builder = new PrintProgramBuilder();
        configure(builder);
        return new BinaryEncoder().Encode(builder.Build()).Value!;
    }

    // ---- Emulate ----

    [Fact]
    public void Emulate_EmptyProgram_ReturnsOneStepPerInstruction()
    {
        var bytes = BuildBytes(_ => { });

        var history = Emulate(bytes);

        // Empty program preamble+postamble: Init, CodePage, ResetPrintMode, LineFeed, CutAfter = 5 instructions
        Assert.Equal(5, history.Length);
    }

    [Fact]
    public void Emulate_ReturnsOneEntryPerInstruction()
    {
        var bytes = BuildBytes(b => b.BoldOn().Text("hi").BoldOff());

        var history = Emulate(bytes);

        // Preamble (3) + BoldOn + Text + BoldOff + Postamble (2) = 8
        Assert.Equal(8, history.Length);
    }

    [Fact]
    public void Emulate_StateProgresses_BoldBecomesTrue()
    {
        var bytes = BuildBytes(b => b.BoldOn().Text("bold"));

        var history = Emulate(bytes);

        // Preamble is 3 steps: Init (index 0), CodePage (index 1), ResetPrintMode (index 2)
        // BoldOn is index 3
        Assert.True(history[3].State.Bold);
        // Text "bold" is index 4 — state still bold
        Assert.True(history[4].State.Bold);
    }

    [Fact]
    public void Emulate_Receipt_AccumulatesAcrossSteps()
    {
        var bytes = BuildBytes(b => b.BoldOn().Text("bold"));

        var history = Emulate(bytes);

        // At each step the receipt grows: by the end it contains all emitted elements
        var lastReceipt = history[^1].Receipt;
        Assert.Contains(lastReceipt.Elements, e => e is TextSpan { Text: "bold" });
    }

    [Fact]
    public void Emulate_FinalReceipt_ContainsExpectedElements()
    {
        var bytes = BuildBytes(_ => { });

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

        // Empty program preamble+postamble: Init, CodePage, ResetPrintMode produce no elements
        // LineFeed -> LineBreak, CutAfter -> HorizontalRule
        Assert.Equal(2, elements.Length);
        Assert.IsType<LineBreak>(elements[0]);
        Assert.IsType<HorizontalRule>(elements[1]);
    }

    [Fact]
    public void Emulate_TextProgram_FinalReceiptHasTextSpanLineBreakHorizontalRule()
    {
        var bytes = BuildBytes(b => b.Text("Hello"));

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

        // TextSpan("Hello") + LineBreak (postamble LF) + HorizontalRule (postamble CutAfter)
        Assert.Equal(3, elements.Length);
        var span = Assert.IsType<TextSpan>(elements[0]);
        Assert.Equal("Hello", span.Text);
        Assert.IsType<LineBreak>(elements[1]);
        Assert.IsType<HorizontalRule>(elements[2]);
    }

    [Fact]
    public void Emulate_BoldText_TextSpanHasBoldStyle()
    {
        var bytes = BuildBytes(b => b.BoldOn().Text("bold"));

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

        var span = Assert.IsType<TextSpan>(elements[0]);
        Assert.Equal("bold", span.Text);
        Assert.True(span.Style.Bold);
    }

    [Fact]
    public void Emulate_CenteredText_TextSpanHasCenterJustification()
    {
        var bytes = BuildBytes(b => b.Align(Alignment.Center).Text("center"));

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

        var span = Assert.IsType<TextSpan>(elements[0]);
        Assert.Equal(Alignment.Center, span.Style.Justification);
    }

    [Fact]
    public void Emulate_MultiLinePrintLine_ProducesAlternatingSpansAndLineBreaks()
    {
        var bytes = BuildBytes(b => b.PrintLine("line1").PrintLine("line2"));

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

        // PrintLine = Text + LineFeed
        // line1: TextSpan + LineBreak
        // line2: TextSpan + LineBreak
        // Postamble: LineBreak + HorizontalRule
        Assert.IsType<TextSpan>(elements[0]);
        Assert.IsType<LineBreak>(elements[1]);
        Assert.IsType<TextSpan>(elements[2]);
        Assert.IsType<LineBreak>(elements[3]);
    }

    [Fact]
    public void Emulate_RealProgram_ProducesExpectedElementCount()
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

        var history = Emulate(bytes);
        var elements = history[^1].Receipt.Elements;

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
        Assert.Equal(7, elements.Length);
    }

    [Fact]
    public void Emulate_TimeTravelIndex_ReturnsCorrectIntermediateState()
    {
        var bytes = BuildBytes(b => b
            .BoldOn()
            .Align(Alignment.Center)
            .BoldOff());

        var history = Emulate(bytes);

        // Preamble: Init (0), CodePage (1), ResetPrintMode (2)
        // BoldOn (3): bold=true
        // Align center (4): bold=true, justification=center
        // BoldOff (5): bold=false, justification=center
        Assert.True(history[3].State.Bold);
        Assert.Equal(Alignment.Left, history[3].State.Justification);

        Assert.True(history[4].State.Bold);
        Assert.Equal(Alignment.Center, history[4].State.Justification);

        Assert.False(history[5].State.Bold);
        Assert.Equal(Alignment.Center, history[5].State.Justification);
    }
}