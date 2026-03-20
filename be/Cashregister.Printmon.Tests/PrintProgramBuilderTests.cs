using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;

namespace Cashregister.Printmon.Tests;

public sealed class PrintProgramBuilderTests
{
    [Fact]
    public void Build_WithNoMethodsCalled_ContainsAutoInstructions()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Build();

        Assert.Equal(4, program.Instructions.Length);
        Assert.IsType<InitializeInstruction>(program.Instructions[0]);
        Assert.IsType<SelectCodeTableInstruction>(program.Instructions[1]);
        Assert.IsType<LineFeedInstruction>(program.Instructions[2]);
        var cutAfter = Assert.IsType<CutAfterInstruction>(program.Instructions[3]);
        Assert.Equal(1, cutAfter.Distance);
    }

    [Fact]
    public void Build_WithAdditionalInstructions_InitializeIsFirst()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NoOp().Build();

        Assert.Equal(5, program.Instructions.Length);
        Assert.IsType<InitializeInstruction>(program.Instructions[0]);
        Assert.IsType<SelectCodeTableInstruction>(program.Instructions[1]);
        Assert.IsType<NoOpInstruction>(program.Instructions[2]);
        Assert.IsType<LineFeedInstruction>(program.Instructions[3]);
        var cutAfter = Assert.IsType<CutAfterInstruction>(program.Instructions[4]);
        Assert.Equal(1, cutAfter.Distance);
    }

    [Fact]
    public void UseFontA_AddsSelectPrintModeInstruction_WithFontAAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontA(FormatMode.None).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[2]);
        Assert.False(instruction.UseFontB);
        Assert.Equal(FormatMode.None, instruction.Flags);
    }

    [Fact]
    public void UseFontB_AddsSelectPrintModeInstruction_WithFontBAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontB(FormatMode.Emphasized).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[2]);
        Assert.True(instruction.UseFontB);
        Assert.Equal(FormatMode.Emphasized, instruction.Flags);
    }

    [Fact]
    public void UnderlineOn_AddsUnderlineInstruction_EnabledWithGivenThickness()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOn(Thickness.TwoDots).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[2]);
        Assert.True(instruction.Enabled);
        Assert.Equal(Thickness.TwoDots, instruction.Thickness);
    }

    [Fact]
    public void UnderlineOff_AddsUnderlineInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOff().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[2]);
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

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[2]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void EmphasizeOff_AddsEmphasizeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.EmphasizeOff().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[2]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOn_AddsDoubleStrikeInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOn().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[2]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOff_AddsDoubleStrikeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOff().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[2]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void SelectFontA_AddsSelectFontInstruction_WithFontA()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontA().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[2]);
        Assert.Equal(CharacterFont.A, instruction.Font);
    }

    [Fact]
    public void SelectFontB_AddsSelectFontInstruction_WithFontB()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontB().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[2]);
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

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[2]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void NinetyDegsOff_AddsRotationInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NinetyDegsOff().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[2]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOn_AddsUpsideDownInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOn().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[2]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOff_AddsUpsideDownInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOff().Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[2]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void FontSize_AddsFontSizeInstruction_WithGivenSize()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(0x11).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[2]);
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

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<JustifyInstruction>(program.Instructions[2]);
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

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<AbsolutePositionInstruction>(program.Instructions[2]);
        Assert.Equal(1000, instruction.Position);
    }

    [Fact]
    public void SetRelativePosition_AddsRelativePositionInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetRelativePosition(500).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<RelativePositionInstruction>(program.Instructions[2]);
        Assert.Equal(500, instruction.Offset);
    }

    [Fact]
    public void SetLeftMargin_AddsLeftMarginInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SetLeftMargin(200).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<LeftMarginInstruction>(program.Instructions[2]);
        Assert.Equal(200, instruction.Margin);
    }

    [Fact]
    public void Text_AddsTextInstruction_WithGivenText()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Text("Hello").Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<TextInstruction>(program.Instructions[2]);
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

        Assert.Equal(5, program.Instructions.Length);
        Assert.IsType<HorizontalTabInstruction>(program.Instructions[2]);
    }

    [Fact]
    public void LineFeed_AddsLineFeedInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.LineFeed().Build();

        Assert.Equal(5, program.Instructions.Length);
        Assert.IsType<LineFeedInstruction>(program.Instructions[2]);
    }

    [Fact]
    public void PrintLine_AddsTextAndLineFeedInstructions()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.PrintLine("Hello").Build();

        Assert.Equal(6, program.Instructions.Length);
        var textInstruction = Assert.IsType<TextInstruction>(program.Instructions[2]);
        Assert.Equal("Hello", textInstruction.Text);
        Assert.IsType<LineFeedInstruction>(program.Instructions[3]);
    }

    [Fact]
    public void CutAfter_AddsCutAfterInstruction_WithGivenDistance()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.CutAfter(10).Build();

        Assert.Equal(5, program.Instructions.Length);
        var instruction = Assert.IsType<CutAfterInstruction>(program.Instructions[2]);
        Assert.Equal(10, instruction.Distance);
    }
}