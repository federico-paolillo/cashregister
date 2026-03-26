using System.Collections.Immutable;

using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Cut;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

// Preamble: [0] InitializeInstruction, [1] SelectCodePageInstruction, [2] ResetPrintModeInstruction
// User instructions start at index 3
// Postamble: LineFeedInstruction, CutAfterInstruction(1)

namespace Cashregister.Printmon.Tests;

public sealed class PrintProgramBuilderTests
{
    [Fact]
    public void Build_WithNoMethodsCalled_ContainsAutoInstructions()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Build();

        Assert.Equal(5, program.Instructions.Length);
        Assert.IsType<InitializeInstruction>(program.Instructions[0]);
        Assert.IsType<SelectCodePageInstruction>(program.Instructions[1]);
        Assert.IsType<ResetPrintModeInstruction>(program.Instructions[2]);
        Assert.IsType<LineFeedInstruction>(program.Instructions[3]);
        var cutAfter = Assert.IsType<CutAfterInstruction>(program.Instructions[4]);
        Assert.Equal(1, cutAfter.Distance);
    }

    [Fact]
    public void Build_WithAdditionalInstructions_InitializeIsFirst()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NoOp().Build();

        Assert.Equal(6, program.Instructions.Length);
        Assert.IsType<InitializeInstruction>(program.Instructions[0]);
        Assert.IsType<SelectCodePageInstruction>(program.Instructions[1]);
        Assert.IsType<ResetPrintModeInstruction>(program.Instructions[2]);
        Assert.IsType<NoOpInstruction>(program.Instructions[3]);
        Assert.IsType<LineFeedInstruction>(program.Instructions[4]);
        var cutAfter = Assert.IsType<CutAfterInstruction>(program.Instructions[5]);
        Assert.Equal(1, cutAfter.Distance);
    }

    [Fact]
    public void UseFontA_AddsSelectPrintModeInstruction_WithFontAAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontA(FormatMode.None).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[3]);
        Assert.False(instruction.UseFontB);
        Assert.Equal(FormatMode.None, instruction.Flags);
    }

    [Fact]
    public void UseFontB_AddsSelectPrintModeInstruction_WithFontBAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontB(FormatMode.Emphasized).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[3]);
        Assert.True(instruction.UseFontB);
        Assert.Equal(FormatMode.Emphasized, instruction.Flags);
    }

    [Fact]
    public void UnderlineOn_AddsUnderlineInstruction_EnabledWithGivenThickness()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOn(Thickness.TwoDots).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
        Assert.Equal(Thickness.TwoDots, instruction.Thickness);
    }

    [Fact]
    public void UnderlineOff_AddsUnderlineInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void UnderlineOn_InvalidThickness_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UnderlineInstruction(true, (Thickness)99));
    }

    [Fact]
    public void EmphasizeOn_AddsEmphasizeInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.EmphasizeOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void EmphasizeOff_AddsEmphasizeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.EmphasizeOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOn_AddsDoubleStrikeInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOff_AddsDoubleStrikeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void SelectFontA_AddsSelectFontInstruction_WithFontA()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontA().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[3]);
        Assert.Equal(CharacterFont.A, instruction.Font);
    }

    [Fact]
    public void SelectFontB_AddsSelectFontInstruction_WithFontB()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontB().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[3]);
        Assert.Equal(CharacterFont.B, instruction.Font);
    }

    [Fact]
    public void SelectFontInstruction_InvalidFont_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SelectFontInstruction((CharacterFont)99));
    }

    [Fact]
    public void NinetyDegsOn_AddsRotationInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NinetyDegsOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void NinetyDegsOff_AddsRotationInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NinetyDegsOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOn_AddsUpsideDownInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOff_AddsUpsideDownInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void FontSize_AddsFontSizeInstruction_WithGivenSize()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(0x11).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[3]);
        Assert.Equal(0x11, instruction.Size);
    }

    [Fact]
    public void FontSizeInstruction_InvalidSize_WidthTooLarge_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FontSizeInstruction(0x80));
    }

    [Fact]
    public void FontSizeInstruction_InvalidSize_HeightTooLarge_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FontSizeInstruction(0x08));
    }

    [Fact]
    public void Justify_AddsJustifyInstruction_WithGivenJustification()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Justify(Justification.Center).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<JustifyInstruction>(program.Instructions[3]);
        Assert.Equal(Justification.Center, instruction.Justification);
    }

    [Fact]
    public void JustifyInstruction_InvalidJustification_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JustifyInstruction((Justification)99));
    }

    [Fact]
    public void SetAbsolutePosition_AddsAbsolutePositionInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetAbsolutePosition(1000).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<AbsolutePositionInstruction>(program.Instructions[3]);
        Assert.Equal(1000, instruction.Position);
    }

    [Fact]
    public void SetRelativePosition_AddsRelativePositionInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetRelativePosition(500).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RelativePositionInstruction>(program.Instructions[3]);
        Assert.Equal(500, instruction.Offset);
    }

    [Fact]
    public void SetLeftMargin_AddsLeftMarginInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetLeftMargin(200).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<LeftMarginInstruction>(program.Instructions[3]);
        Assert.Equal(200, instruction.Margin);
    }

    [Fact]
    public void Text_AddsTextInstruction_WithGivenText()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Text("Hello").Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<TextInstruction>(program.Instructions[3]);
        Assert.Equal("Hello", instruction.Text);
    }

    [Fact]
    public void TextInstruction_NullText_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TextInstruction(null!));
    }

    [Fact]
    public void TextInstruction_EmptyText_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TextInstruction(""));
    }

    [Fact]
    public void HorizontalTab_AddsHorizontalTabInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.HorizontalTab().Build();

        Assert.Equal(6, program.Instructions.Length);
        Assert.IsType<HorizontalTabInstruction>(program.Instructions[3]);
    }

    [Fact]
    public void LineFeed_AddsLineFeedInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.LineFeed().Build();

        Assert.Equal(6, program.Instructions.Length);
        Assert.IsType<LineFeedInstruction>(program.Instructions[3]);
    }

    [Fact]
    public void PrintLine_AddsTextAndLineFeedInstructions()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.PrintLine("Hello").Build();

        Assert.Equal(7, program.Instructions.Length);
        var textInstruction = Assert.IsType<TextInstruction>(program.Instructions[3]);
        Assert.Equal("Hello", textInstruction.Text);
        Assert.IsType<LineFeedInstruction>(program.Instructions[4]);
    }

    [Fact]
    public void CutAfter_AddsCutAfterInstruction_WithGivenDistance()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.CutAfter(10).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<CutAfterInstruction>(program.Instructions[3]);
        Assert.Equal(10, instruction.Distance);
    }

    [Fact]
    public void ResetLineSpacing_AddsResetLineSpacingInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.ResetLineSpacing().Build();

        Assert.Equal(6, program.Instructions.Length);
        Assert.IsType<ResetLineSpacingInstruction>(program.Instructions[3]);
    }

    [Fact]
    public void SetLineSpacing_AddsSetLineSpacingInstruction_WithGivenSpacing()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetLineSpacing(30).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SetLineSpacingInstruction>(program.Instructions[3]);
        Assert.Equal(30, instruction.Spacing);
    }

    [Fact]
    public void ReverseOn_AddsReverseInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.ReverseOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<ReverseInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void ReverseOff_AddsReverseInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.ReverseOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<ReverseInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void SetRightSpacing_AddsRightSpacingInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetRightSpacing(10).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RightSpacingInstruction>(program.Instructions[3]);
        Assert.Equal(10, instruction.Spacing);
    }

    [Fact]
    public void SetHorizontalTabs_AddsSetHorizontalTabsInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetHorizontalTabs(8, 16, 24).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SetHorizontalTabsInstruction>(program.Instructions[3]);
        Assert.Equal<byte>([8, 16, 24], instruction.Positions);
    }

    [Fact]
    public void ClearHorizontalTabs_AddsSetHorizontalTabsInstruction_Empty()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.ClearHorizontalTabs().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SetHorizontalTabsInstruction>(program.Instructions[3]);
        Assert.True(instruction.Positions.IsEmpty);
    }

    [Fact]
    public void SetHorizontalTabsInstruction_TooManyPositions_Throws()
    {
        var positions = ImmutableArray.CreateRange(Enumerable.Range(1, 33).Select(i => (byte)i));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SetHorizontalTabsInstruction(positions));
    }

    [Fact]
    public void SetHorizontalTabsInstruction_ZeroValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SetHorizontalTabsInstruction([0]));
    }

    [Fact]
    public void SetHorizontalTabsInstruction_NotAscending_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SetHorizontalTabsInstruction([10, 5]));
    }

    [Fact]
    public void SetHorizontalTabsInstruction_DuplicateValues_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SetHorizontalTabsInstruction([10, 10]));
    }

    [Fact]
    public void FeedLines_AddsFeedLinesInstruction_WithGivenLines()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.FeedLines(5).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FeedLinesInstruction>(program.Instructions[3]);
        Assert.Equal(5, instruction.Lines);
    }

    [Fact]
    public void FeedPaper_AddsFeedPaperInstruction_WithGivenAmount()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.FeedPaper(100).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FeedPaperInstruction>(program.Instructions[3]);
        Assert.Equal(100, instruction.Amount);
    }

    [Fact]
    public void KickDrawer_AddsGeneratePulseInstruction_WithCorrectParams()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.KickDrawer(ConnectorPin.Pin5, 10, 20).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<GeneratePulseInstruction>(program.Instructions[3]);
        Assert.Equal(ConnectorPin.Pin5, instruction.Pin);
        Assert.Equal(10, instruction.OnTime);
        Assert.Equal(20, instruction.OffTime);
    }

    [Fact]
    public void OpenCashDrawer_AddsGeneratePulseInstruction_WithDefaults()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.OpenCashDrawer().Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<GeneratePulseInstruction>(program.Instructions[3]);
        Assert.Equal(ConnectorPin.Pin2, instruction.Pin);
        Assert.Equal(25, instruction.OnTime);
        Assert.Equal(250, instruction.OffTime);
    }

    [Fact]
    public void GeneratePulseInstruction_InvalidPin_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneratePulseInstruction((ConnectorPin)99, 25, 250));
    }
}