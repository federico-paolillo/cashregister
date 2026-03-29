using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
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
    private readonly PrinterState _default = PrinterState.Default;

    // ---- Core ----

    [Fact]
    public void Execute_NoOp_ReturnsUnchangedStateAndNoElements()
    {
        var doc = _executor.Execute(_default, new NoOpInstruction());

        Assert.Equal(_default, doc.State);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_Initialize_ResetsToDefault()
    {
        var modified = _default with { Bold = true, Justification = Alignment.Center };

        var doc = _executor.Execute(modified, new InitializeInstruction());

        Assert.Equal(PrinterState.Default, doc.State);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_Text_EmitsTextSpanWithCurrentStyle()
    {
        var state = _default with { Bold = true };

        var doc = _executor.Execute(state, new TextInstruction("hello"));

        var span = Assert.IsType<TextSpan>(Assert.Single(doc.Elements));
        Assert.Equal("hello", span.Text);
        Assert.True(span.Style.Bold);
    }

    [Fact]
    public void Execute_Text_StyleSnapshotMatchesCurrentState()
    {
        var state = _default with
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
        };

        var doc = _executor.Execute(state, new TextInstruction("x"));

        var span = Assert.IsType<TextSpan>(Assert.Single(doc.Elements));
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
        var doc = _executor.Execute(_default, new LineFeedInstruction());

        Assert.IsType<LineBreak>(Assert.Single(doc.Elements));
        Assert.Equal(_default, doc.State);
    }

    // ---- Formatting ----

    [Fact]
    public void Execute_EmphasizeOn_SetsBold()
    {
        var doc = _executor.Execute(_default, new EmphasizeInstruction(true));

        Assert.True(doc.State.Bold);
        Assert.Empty(doc.Elements);
        // All other fields unchanged
        Assert.Equal(_default with { Bold = true }, doc.State);
    }

    [Fact]
    public void Execute_EmphasizeOff_ClearsBold()
    {
        var state = _default with { Bold = true };

        var doc = _executor.Execute(state, new EmphasizeInstruction(false));

        Assert.False(doc.State.Bold);
    }

    [Fact]
    public void Execute_DoubleStrikeOn_SetsDoubleStrike()
    {
        var doc = _executor.Execute(_default, new DoubleStrikeInstruction(true));

        Assert.True(doc.State.DoubleStrike);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_UnderlineThin_SetsUnderline()
    {
        var doc = _executor.Execute(_default, new UnderlineInstruction(true, Thickness.Thin));

        Assert.Equal(Thickness.Thin, doc.State.Underline);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_UnderlineOff_ClearsUnderline()
    {
        var state = _default with { Underline = Thickness.Thick };

        var doc = _executor.Execute(state, new UnderlineInstruction(false, Thickness.None));

        Assert.Equal(Thickness.None, doc.State.Underline);
    }

    [Fact]
    public void Execute_SelectFontB_SetsFont()
    {
        var doc = _executor.Execute(_default, new SelectFontInstruction(CharacterFont.B));

        Assert.Equal(CharacterFont.B, doc.State.Font);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_FontSize_SetsWidthAndHeightMultipliers()
    {
        // Size = ((2-1) << 4) | (3-1) = 0x12
        var doc = _executor.Execute(_default, new FontSizeInstruction(0x12));

        Assert.Equal(2, doc.State.WidthMultiplier);
        Assert.Equal(3, doc.State.HeightMultiplier);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_ReverseOn_SetsReverse()
    {
        var doc = _executor.Execute(_default, new ReverseInstruction(true));

        Assert.True(doc.State.Reverse);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_RotationOn_SetsRotation()
    {
        var doc = _executor.Execute(_default, new RotationInstruction(true));

        Assert.True(doc.State.Rotation);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_UpsideDownOn_SetsUpsideDown()
    {
        var doc = _executor.Execute(_default, new UpsideDownInstruction(true));

        Assert.True(doc.State.UpsideDown);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_ResetPrintMode_ClearsBoldFontSizeUnderline()
    {
        var state = _default with
        {
            Bold = true,
            Font = CharacterFont.B,
            WidthMultiplier = 3,
            HeightMultiplier = 2,
            Underline = Thickness.Thick
        };

        var doc = _executor.Execute(state, new ResetPrintModeInstruction());

        Assert.False(doc.State.Bold);
        Assert.Equal(CharacterFont.A, doc.State.Font);
        Assert.Equal(1, doc.State.WidthMultiplier);
        Assert.Equal(1, doc.State.HeightMultiplier);
        Assert.Equal(Thickness.None, doc.State.Underline);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_SelectPrintMode_SetsMultipleFieldsSimultaneously()
    {
        var spm = new SelectPrintModeInstruction(
            UseFontB: true,
            Flags: FormatMode.Emphasized | FormatMode.DoubleWidth | FormatMode.DoubleHeight);

        var doc = _executor.Execute(_default, spm);

        Assert.Equal(CharacterFont.B, doc.State.Font);
        Assert.True(doc.State.Bold);
        Assert.Equal(2, doc.State.WidthMultiplier);
        Assert.Equal(2, doc.State.HeightMultiplier);
        Assert.Equal(Thickness.None, doc.State.Underline);
        Assert.Empty(doc.Elements);
    }

    // ---- Layout ----

    [Fact]
    public void Execute_JustifyCenter_SetsJustification()
    {
        var doc = _executor.Execute(_default, new JustifyInstruction(Alignment.Center));

        Assert.Equal(Alignment.Center, doc.State.Justification);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_LeftMargin_SetsLeftMargin()
    {
        var doc = _executor.Execute(_default, new LeftMarginInstruction(50));

        Assert.Equal(50, doc.State.LeftMargin);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_RightSpacing_SetsRightSpacing()
    {
        var doc = _executor.Execute(_default, new RightSpacingInstruction(5));

        Assert.Equal(5, doc.State.RightSpacing);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_PrintAreaWidth_SetsPrintAreaWidth()
    {
        var doc = _executor.Execute(_default, new PrintAreaWidthInstruction(300));

        Assert.Equal(300, doc.State.PrintAreaWidth);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_AbsolutePosition_DoesNotChangeState()
    {
        var doc = _executor.Execute(_default, new AbsolutePositionInstruction(100));

        Assert.Equal(_default, doc.State);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_RelativePosition_DoesNotChangeState()
    {
        var doc = _executor.Execute(_default, new RelativePositionInstruction(50));

        Assert.Equal(_default, doc.State);
        Assert.Empty(doc.Elements);
    }

    // ---- Feed ----

    [Fact]
    public void Execute_ResetLineSpacing_SetsLineSpacingTo30()
    {
        var state = _default with { LineSpacing = 40 };

        var doc = _executor.Execute(state, new ResetLineSpacingInstruction());

        Assert.Equal(30, doc.State.LineSpacing);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_SetLineSpacing_SetsLineSpacing()
    {
        var doc = _executor.Execute(_default, new SetLineSpacingInstruction(50));

        Assert.Equal(50, doc.State.LineSpacing);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_FeedLines_EmitsFeedLinesElement()
    {
        var doc = _executor.Execute(_default, new FeedLinesInstruction(3));

        var feed = Assert.IsType<FeedLines>(Assert.Single(doc.Elements));
        Assert.Equal(3, feed.Count);
        Assert.Equal(_default, doc.State);
    }

    [Fact]
    public void Execute_FeedPaper_EmitsFeedLinesElement()
    {
        var doc = _executor.Execute(_default, new FeedPaperInstruction(10));

        var feed = Assert.IsType<FeedLines>(Assert.Single(doc.Elements));
        Assert.Equal(1, feed.Count);
    }

    // ---- Motion ----

    [Fact]
    public void Execute_HorizontalTab_EmitsTextSpanWithTab()
    {
        var doc = _executor.Execute(_default, new HorizontalTabInstruction());

        var span = Assert.IsType<TextSpan>(Assert.Single(doc.Elements));
        Assert.Equal("\t", span.Text);
    }

    [Fact]
    public void Execute_SetHorizontalTabs_SetsTabPositions()
    {
        var doc = _executor.Execute(_default, new SetHorizontalTabsInstruction([8, 16, 24]));

        Assert.Equal<byte>([8, 16, 24], doc.State.TabPositions);
        Assert.Empty(doc.Elements);
    }

    // ---- Cut ----

    [Fact]
    public void Execute_CutAfter_EmitsHorizontalRule()
    {
        var doc = _executor.Execute(_default, new CutAfterInstruction(1));

        Assert.IsType<HorizontalRule>(Assert.Single(doc.Elements));
        Assert.Equal(_default, doc.State);
    }

    [Fact]
    public void Execute_Cut_EmitsHorizontalRule()
    {
        var doc = _executor.Execute(_default, new CutInstruction());

        Assert.IsType<HorizontalRule>(Assert.Single(doc.Elements));
    }

    [Fact]
    public void Execute_HalfCut_EmitsHorizontalRule()
    {
        var doc = _executor.Execute(_default, new HalfCutInstruction());

        Assert.IsType<HorizontalRule>(Assert.Single(doc.Elements));
    }

    // ---- CodePage ----

    [Fact]
    public void Execute_SelectCodePage_SetsCodePage()
    {
        var doc = _executor.Execute(_default, new SelectCodePageInstruction(CharacterCodePage.OEM850));

        Assert.Equal(CharacterCodePage.OEM850, doc.State.CodePage);
        Assert.Empty(doc.Elements);
    }

    // ---- Peripheral ----

    [Fact]
    public void Execute_GeneratePulse_DoesNotChangeStateOrEmitElements()
    {
        var doc = _executor.Execute(_default, new GeneratePulseInstruction(ConnectorPin.Pin2, 25, 250));

        Assert.Equal(_default, doc.State);
        Assert.Empty(doc.Elements);
    }

    [Fact]
    public void Execute_RealTimePulse_DoesNotChangeStateOrEmitElements()
    {
        var doc = _executor.Execute(_default, new RealTimePulseInstruction(ConnectorPin.Pin5, 1));

        Assert.Equal(_default, doc.State);
        Assert.Empty(doc.Elements);
    }

    // ---- Composite scenario ----

    [Fact]
    public void Execute_BoldOnThenOffText_TwoSpansHaveDifferentBoldValues()
    {
        var state = _default;

        var doc1 = _executor.Execute(state, new EmphasizeInstruction(true));
        state = doc1.State;

        var doc2 = _executor.Execute(state, new TextInstruction("bold"));
        state = doc2.State;

        var doc3 = _executor.Execute(state, new EmphasizeInstruction(false));
        state = doc3.State;

        var doc4 = _executor.Execute(state, new TextInstruction("normal"));

        var boldSpan = Assert.IsType<TextSpan>(Assert.Single(doc2.Elements));
        var normalSpan = Assert.IsType<TextSpan>(Assert.Single(doc4.Elements));

        Assert.True(boldSpan.Style.Bold);
        Assert.False(normalSpan.Style.Bold);
    }
}
