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
    public void PrintMode_FontA_AddsSelectPrintModeInstruction_WithFontAAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.PrintMode(CharacterFont.A, FormatMode.None).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[3]);
        Assert.False(instruction.UseFontB);
        Assert.Equal(FormatMode.None, instruction.Flags);
    }

    [Fact]
    public void PrintMode_FontB_AddsSelectPrintModeInstruction_WithFontBAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.PrintMode(CharacterFont.B, FormatMode.Emphasized).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[3]);
        Assert.True(instruction.UseFontB);
        Assert.Equal(FormatMode.Emphasized, instruction.Flags);
    }

    [Fact]
    public void UnderlineOn_AddsUnderlineInstruction_EnabledWithGivenThickness()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOn(Thickness.Thick).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
        Assert.Equal(Thickness.Thick, instruction.Thickness);
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
    public void BoldOn_AddsEmphasizeInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.BoldOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void BoldOff_AddsEmphasizeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.BoldOff().Build();

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
    public void Font_AddsSelectFontInstruction_WithFontA()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Font(CharacterFont.A).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[3]);
        Assert.Equal(CharacterFont.A, instruction.Font);
    }

    [Fact]
    public void Font_AddsSelectFontInstruction_WithFontB()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Font(CharacterFont.B).Build();

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
    public void RotateOn_AddsRotationInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.RotateOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void RotateOff_AddsRotationInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.RotateOff().Build();

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
    public void FontSize_WidthAndHeight_AddsFontSizeInstruction_WithPackedByte()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(2, 2).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[3]);
        Assert.Equal(0x11, instruction.Size);
    }

    [Fact]
    public void FontSize_UniformMultiplier_AddsFontSizeInstruction_WithPackedByte()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(3).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[3]);
        Assert.Equal(0x22, instruction.Size);
    }

    [Fact]
    public void FontSize_OneByOne_ProducesZeroByte()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(1, 1).Build();

        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[3]);
        Assert.Equal(0x00, instruction.Size);
    }

    [Fact]
    public void FontSize_MaxWidthAndHeight_ProducesCorrectByte()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(8, 8).Build();

        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[3]);
        Assert.Equal(0x77, instruction.Size);
    }

    [Fact]
    public void FontSize_WidthTooSmall_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FontSize(0, 1));
    }

    [Fact]
    public void FontSize_WidthTooLarge_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FontSize(9, 1));
    }

    [Fact]
    public void FontSize_HeightTooSmall_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FontSize(1, 0));
    }

    [Fact]
    public void FontSize_HeightTooLarge_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FontSize(1, 9));
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
    public void Align_AddsJustifyInstruction_WithGivenAlignment()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Align(Alignment.Center).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<JustifyInstruction>(program.Instructions[3]);
        Assert.Equal(Alignment.Center, instruction.Alignment);
    }

    [Fact]
    public void JustifyInstruction_InvalidAlignment_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JustifyInstruction((Alignment)99));
    }

    [Fact]
    public void MoveToColumn_AddsAbsolutePositionInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.MoveToColumn(1000).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<AbsolutePositionInstruction>(program.Instructions[3]);
        Assert.Equal(1000, instruction.Position);
    }

    [Fact]
    public void MoveBy_AddsRelativePositionInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.MoveBy(500).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RelativePositionInstruction>(program.Instructions[3]);
        Assert.Equal(500, instruction.Offset);
    }

    [Fact]
    public void LeftMargin_AddsLeftMarginInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.LeftMargin(200).Build();

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
    public void FeedAndCut_AddsCutAfterInstruction_WithGivenLines()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FeedAndCut(10).Build();

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
    public void SetLineSpacing_InMillimeters_AddsSetLineSpacingInstruction()
    {
        var builder = new PrintProgramBuilder();

        // 3.75mm = 30 units (3.75 / 0.125 = 30)
        var program = builder.SetLineSpacing(3.75).Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<SetLineSpacingInstruction>(program.Instructions[3]);
        Assert.Equal(30, instruction.Spacing);
    }

    [Fact]
    public void InvertOn_AddsReverseInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.InvertOn().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<ReverseInstruction>(program.Instructions[3]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void InvertOff_AddsReverseInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.InvertOff().Build();

        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<ReverseInstruction>(program.Instructions[3]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void SetCharacterSpacing_InMillimeters_AddsRightSpacingInstruction()
    {
        var builder = new PrintProgramBuilder();

        // 1.25mm = 10 units (1.25 / 0.125 = 10)
        var program = builder.SetCharacterSpacing(1.25).Build();

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
    public void FeedPaper_InMillimeters_AddsFeedPaperInstruction()
    {
        var builder = new PrintProgramBuilder();
        // 12.5mm = 100 units (12.5 / 0.125 = 100)
        var program = builder.FeedPaper(12.5).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<FeedPaperInstruction>(program.Instructions[3]);
        Assert.Equal(100, instruction.Amount);
    }

    [Fact]
    public void KickDrawer_WithTimeSpan_AddsGeneratePulseInstruction()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.KickDrawer(ConnectorPin.Pin5, TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(40)).Build();
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

    [Fact]
    public void RealTimePulse_WithTimeSpan_AddsRealTimePulseInstruction()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.RealTimePulse(ConnectorPin.Pin2, TimeSpan.FromMilliseconds(300)).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<RealTimePulseInstruction>(program.Instructions[3]);
        Assert.Equal(ConnectorPin.Pin2, instruction.Pin);
        Assert.Equal(3, instruction.Duration);
    }

    [Fact]
    public void RealTimePulse_DurationTooShort_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.RealTimePulse(ConnectorPin.Pin2, TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public void RealTimePulse_DurationTooLong_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            builder.RealTimePulse(ConnectorPin.Pin2, TimeSpan.FromMilliseconds(900)));
    }

    [Fact]
    public void FeedPaper_NegativeMillimeters_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FeedPaper(-1.0));
    }

    [Fact]
    public void FeedPaper_TooLargeMillimeters_Throws()
    {
        var builder = new PrintProgramBuilder();
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.FeedPaper(32.0));
    }

    [Fact]
    public void PrintWidth_AddsPrintAreaWidthInstruction()
    {
        var builder = new PrintProgramBuilder();
        var program = builder.PrintWidth(512).Build();
        Assert.Equal(6, program.Instructions.Length);
        var instruction = Assert.IsType<PrintAreaWidthInstruction>(program.Instructions[3]);
        Assert.Equal(512, instruction.Width);
    }
}
