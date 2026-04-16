using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Cut;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class InstructionExecutorTests
{
    private readonly InstructionExecutor _executor = new();
    private readonly Printer _printer = Printer.Default;

    // Unwraps the result for happy-path tests
    private Printer Execute(Printer printer, Instruction instruction) =>
        _executor.Execute(printer, instruction).Value!;

    // ---- Core ----

    [Fact]
    public void Execute_NoOp_ReturnsUnchangedPrinter()
    {
        var result = Execute(_printer, new NoOpInstruction());

        Assert.Equal(_printer, result);
    }

    [Fact]
    public void Execute_Initialize_ResetsStateToDefault()
    {
        var modified = _printer with { State = _printer.State with { Bold = true, Justification = Alignment.Center } };

        var result = Execute(modified, new InitializeInstruction());

        Assert.Equal(PrinterState.Default, result.State);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_Text_EmitsTextSpanWithCurrentStyle()
    {
        var printer = _printer with { State = _printer.State with { Bold = true } };

        var result = Execute(printer, new TextInstruction("hello"));

        var span = Assert.IsType<TextSpan>(Assert.Single(result.Receipt.Elements));
        Assert.Equal("hello", span.Text);
        Assert.True(span.Style.Bold);
    }

    [Fact]
    public void Execute_Text_StyleSnapshotMatchesCurrentState()
    {
        var printer = _printer with
        {
            State = _printer.State with
            {
                Bold = true,
                Underline = Thickness.Thin,
                DoubleStrike = true,
                Font = CharacterFont.B,
                Rotation = true,
                UpsideDown = true,
                Reverse = true,
                WidthMultiplier = 2,
                HeightMultiplier = 3,
                Justification = Alignment.Center
            }
        };

        var result = Execute(printer, new TextInstruction("x"));

        var span = Assert.IsType<TextSpan>(Assert.Single(result.Receipt.Elements));
        Assert.True(span.Style.Bold);
        Assert.Equal(Thickness.Thin, span.Style.Underline);
        Assert.True(span.Style.DoubleStrike);
        Assert.Equal(CharacterFont.B, span.Style.Font);
        Assert.True(span.Style.Rotation);
        Assert.True(span.Style.UpsideDown);
        Assert.True(span.Style.Reverse);
        Assert.Equal(2, span.Style.WidthMultiplier);
        Assert.Equal(3, span.Style.HeightMultiplier);
        Assert.Equal(Alignment.Center, span.Style.Justification);
    }

    [Fact]
    public void Execute_LineFeed_EmitsLineBreak()
    {
        var result = Execute(_printer, new LineFeedInstruction());

        Assert.IsType<LineBreak>(Assert.Single(result.Receipt.Elements));
        Assert.Equal(_printer.State, result.State);
    }

    // ---- Formatting ----

    [Fact]
    public void Execute_EmphasizeOn_SetsBold()
    {
        var result = Execute(_printer, new EmphasizeInstruction(true));

        Assert.True(result.State.Bold);
        Assert.Empty(result.Receipt.Elements);
        Assert.Equal(_printer.State with { Bold = true }, result.State);
    }

    [Fact]
    public void Execute_EmphasizeOff_ClearsBold()
    {
        var printer = _printer with { State = _printer.State with { Bold = true } };

        var result = Execute(printer, new EmphasizeInstruction(false));

        Assert.False(result.State.Bold);
    }

    [Fact]
    public void Execute_DoubleStrikeOn_SetsDoubleStrike()
    {
        var result = Execute(_printer, new DoubleStrikeInstruction(true));

        Assert.True(result.State.DoubleStrike);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_UnderlineThin_SetsUnderline()
    {
        var result = Execute(_printer, new UnderlineInstruction(true, Thickness.Thin));

        Assert.Equal(Thickness.Thin, result.State.Underline);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_UnderlineOff_ClearsUnderline()
    {
        var printer = _printer with { State = _printer.State with { Underline = Thickness.Thick } };

        var result = Execute(printer, new UnderlineInstruction(false, Thickness.None));

        Assert.Equal(Thickness.None, result.State.Underline);
    }

    [Fact]
    public void Execute_SelectFontB_SetsFont()
    {
        var result = Execute(_printer, new SelectFontInstruction(CharacterFont.B));

        Assert.Equal(CharacterFont.B, result.State.Font);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_FontSize_SetsWidthAndHeightMultipliers()
    {
        // Size = ((2-1) << 4) | (3-1) = 0x12
        var result = Execute(_printer, new FontSizeInstruction(0x12));

        Assert.Equal(2, result.State.WidthMultiplier);
        Assert.Equal(3, result.State.HeightMultiplier);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_ReverseOn_SetsReverse()
    {
        var result = Execute(_printer, new ReverseInstruction(true));

        Assert.True(result.State.Reverse);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_RotationOn_SetsRotation()
    {
        var result = Execute(_printer, new RotationInstruction(true));

        Assert.True(result.State.Rotation);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_UpsideDownOn_SetsUpsideDown()
    {
        var result = Execute(_printer, new UpsideDownInstruction(true));

        Assert.True(result.State.UpsideDown);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_ResetPrintMode_ClearsBoldFontSizeUnderline()
    {
        var printer = _printer with
        {
            State = _printer.State with
            {
                Bold = true,
                Font = CharacterFont.B,
                WidthMultiplier = 3,
                HeightMultiplier = 2,
                Underline = Thickness.Thick
            }
        };

        var result = Execute(printer, new ResetPrintModeInstruction());

        Assert.False(result.State.Bold);
        Assert.Equal(CharacterFont.A, result.State.Font);
        Assert.Equal(1, result.State.WidthMultiplier);
        Assert.Equal(1, result.State.HeightMultiplier);
        Assert.Equal(Thickness.None, result.State.Underline);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_SelectPrintMode_SetsMultipleFieldsSimultaneously()
    {
        var spm = new SelectPrintModeInstruction(
            UseFontB: true,
            Flags: FormatMode.Emphasized | FormatMode.DoubleWidth | FormatMode.DoubleHeight);

        var result = Execute(_printer, spm);

        Assert.Equal(CharacterFont.B, result.State.Font);
        Assert.True(result.State.Bold);
        Assert.Equal(2, result.State.WidthMultiplier);
        Assert.Equal(2, result.State.HeightMultiplier);
        Assert.Equal(Thickness.None, result.State.Underline);
        Assert.Empty(result.Receipt.Elements);
    }

    // ---- Layout ----

    [Fact]
    public void Execute_JustifyCenter_SetsJustification()
    {
        var result = Execute(_printer, new JustifyInstruction(Alignment.Center));

        Assert.Equal(Alignment.Center, result.State.Justification);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_LeftMargin_SetsLeftMargin()
    {
        var result = Execute(_printer, new LeftMarginInstruction(50));

        Assert.Equal(50, result.State.LeftMargin);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_RightSpacing_SetsRightSpacing()
    {
        var result = Execute(_printer, new RightSpacingInstruction(5));

        Assert.Equal(5, result.State.RightSpacing);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_PrintAreaWidth_SetsPrintAreaWidth()
    {
        var result = Execute(_printer, new PrintAreaWidthInstruction(300));

        Assert.Equal(300, result.State.PrintAreaWidth);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_AbsolutePosition_DoesNotChangeState()
    {
        var result = Execute(_printer, new AbsolutePositionInstruction(100));

        Assert.Equal(_printer.State, result.State);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_RelativePosition_DoesNotChangeState()
    {
        var result = Execute(_printer, new RelativePositionInstruction(50));

        Assert.Equal(_printer.State, result.State);
        Assert.Empty(result.Receipt.Elements);
    }

    // ---- Feed ----

    [Fact]
    public void Execute_ResetLineSpacing_SetsLineSpacingTo30()
    {
        var printer = _printer with { State = _printer.State with { LineSpacing = 40 } };

        var result = Execute(printer, new ResetLineSpacingInstruction());

        Assert.Equal(30, result.State.LineSpacing);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_SetLineSpacing_SetsLineSpacing()
    {
        var result = Execute(_printer, new SetLineSpacingInstruction(50));

        Assert.Equal(50, result.State.LineSpacing);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_FeedLines_EmitsFeedLinesElement()
    {
        var result = Execute(_printer, new FeedLinesInstruction(3));

        var feed = Assert.IsType<FeedLines>(Assert.Single(result.Receipt.Elements));
        Assert.Equal(3, feed.Count);
        Assert.Equal(_printer.State, result.State);
    }

    [Fact]
    public void Execute_FeedPaper_EmitsFeedLinesElement()
    {
        var result = Execute(_printer, new FeedPaperInstruction(10));

        var feed = Assert.IsType<FeedLines>(Assert.Single(result.Receipt.Elements));
        Assert.Equal(1, feed.Count);
    }

    // ---- Motion ----

    [Fact]
    public void Execute_HorizontalTab_EmitsTextSpanWithTab()
    {
        var result = Execute(_printer, new HorizontalTabInstruction());

        var span = Assert.IsType<TextSpan>(Assert.Single(result.Receipt.Elements));
        Assert.Equal("\t", span.Text);
    }

    [Fact]
    public void Execute_SetHorizontalTabs_SetsTabPositions()
    {
        var result = Execute(_printer, new SetHorizontalTabsInstruction([8, 16, 24]));

        Assert.Equal<byte>([8, 16, 24], result.State.TabPositions);
        Assert.Empty(result.Receipt.Elements);
    }

    // ---- Cut ----

    [Fact]
    public void Execute_CutAfter_EmitsHorizontalRule()
    {
        var result = Execute(_printer, new CutAfterInstruction(1));

        Assert.IsType<HorizontalRule>(Assert.Single(result.Receipt.Elements));
        Assert.Equal(_printer.State, result.State);
    }

    [Fact]
    public void Execute_Cut_EmitsHorizontalRule()
    {
        var result = Execute(_printer, new CutInstruction());

        Assert.IsType<HorizontalRule>(Assert.Single(result.Receipt.Elements));
    }

    [Fact]
    public void Execute_HalfCut_EmitsHorizontalRule()
    {
        var result = Execute(_printer, new HalfCutInstruction());

        Assert.IsType<HorizontalRule>(Assert.Single(result.Receipt.Elements));
    }

    // ---- CodePage ----

    [Fact]
    public void Execute_SelectCodePage_SetsCodePage()
    {
        var result = Execute(_printer, new SelectCodePageInstruction(CharacterCodePage.OEM850));

        Assert.Equal(CharacterCodePage.OEM850, result.State.CodePage);
        Assert.Empty(result.Receipt.Elements);
    }

    // ---- Peripheral ----

    [Fact]
    public void Execute_GeneratePulse_DoesNotChangeStateOrEmitElements()
    {
        var result = Execute(_printer, new GeneratePulseInstruction(ConnectorPin.Pin2, 25, 250));

        Assert.Equal(_printer.State, result.State);
        Assert.Empty(result.Receipt.Elements);
    }

    [Fact]
    public void Execute_RealTimePulse_DoesNotChangeStateOrEmitElements()
    {
        var result = Execute(_printer, new RealTimePulseInstruction(ConnectorPin.Pin5, 1));

        Assert.Equal(_printer.State, result.State);
        Assert.Empty(result.Receipt.Elements);
    }

    // ---- Composite scenario ----

    [Fact]
    public void Execute_BoldOnThenOffText_TwoSpansHaveDifferentBoldValues()
    {
        var p = _printer;

        p = Execute(p, new EmphasizeInstruction(true));
        p = Execute(p, new TextInstruction("bold"));
        p = Execute(p, new EmphasizeInstruction(false));
        p = Execute(p, new TextInstruction("normal"));

        // Receipt has accumulated: [TextSpan("bold"), TextSpan("normal")]
        Assert.Equal(2, p.Receipt.Elements.Length);
        var boldSpan = Assert.IsType<TextSpan>(p.Receipt.Elements[0]);
        var normalSpan = Assert.IsType<TextSpan>(p.Receipt.Elements[1]);

        Assert.True(boldSpan.Style.Bold);
        Assert.False(normalSpan.Style.Bold);
    }
}