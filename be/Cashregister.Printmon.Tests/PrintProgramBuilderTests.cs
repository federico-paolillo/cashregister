using Cashregister.Printmon;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;

namespace Cashregister.Printmon.Tests;

public sealed class PrintProgramBuilderTests
{
    [Fact]
    public void Build_WithNoMethodsCalled_ContainsExactlyOneInitializeInstruction()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.Build();

        var instruction = Assert.Single(program.Instructions);
        Assert.IsType<InitializeInstruction>(instruction);
    }

    [Fact]
    public void Build_WithAdditionalInstructions_InitializeIsFirst()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NoOp().Build();

        Assert.Equal(2, program.Instructions.Length);
        Assert.IsType<InitializeInstruction>(program.Instructions[0]);
        Assert.IsType<NoOpInstruction>(program.Instructions[1]);
    }

    [Fact]
    public void UseFontA_AddsSelectPrintModeInstruction_WithFontAAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontA(FormatMode.None).Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[1]);
        Assert.False(instruction.UseFontB);
        Assert.Equal(FormatMode.None, instruction.Flags);
    }

    [Fact]
    public void UseFontB_AddsSelectPrintModeInstruction_WithFontBAndGivenFlags()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UseFontB(FormatMode.Emphasized).Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<SelectPrintModeInstruction>(program.Instructions[1]);
        Assert.True(instruction.UseFontB);
        Assert.Equal(FormatMode.Emphasized, instruction.Flags);
    }

    [Fact]
    public void UnderlineOn_AddsUnderlineInstruction_EnabledWithGivenThickness()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOn(Thickness.TwoDots).Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[1]);
        Assert.True(instruction.Enabled);
        Assert.Equal(Thickness.TwoDots, instruction.Thickness);
    }

    [Fact]
    public void UnderlineOff_AddsUnderlineInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UnderlineOff().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<UnderlineInstruction>(program.Instructions[1]);
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

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[1]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void EmphasizeOff_AddsEmphasizeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.EmphasizeOff().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<EmphasizeInstruction>(program.Instructions[1]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOn_AddsDoubleStrikeInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOn().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[1]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void DoubleStrikeOff_AddsDoubleStrikeInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.DoubleStrikeOff().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<DoubleStrikeInstruction>(program.Instructions[1]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void SelectFontA_AddsSelectFontInstruction_WithFontA()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontA().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[1]);
        Assert.Equal(CharacterFont.A, instruction.Font);
    }

    [Fact]
    public void SelectFontB_AddsSelectFontInstruction_WithFontB()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.SelectFontB().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<SelectFontInstruction>(program.Instructions[1]);
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

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[1]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void NinetyDegsOff_AddsRotationInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.NinetyDegsOff().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<RotationInstruction>(program.Instructions[1]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOn_AddsUpsideDownInstruction_Enabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOn().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[1]);
        Assert.True(instruction.Enabled);
    }

    [Fact]
    public void UpsideDownOff_AddsUpsideDownInstruction_Disabled()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.UpsideDownOff().Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<UpsideDownInstruction>(program.Instructions[1]);
        Assert.False(instruction.Enabled);
    }

    [Fact]
    public void FontSize_AddsFontSizeInstruction_WithGivenSize()
    {
        var builder = new PrintProgramBuilder();

        var program = builder.FontSize(0x11).Build();

        Assert.Equal(2, program.Instructions.Length);
        var instruction = Assert.IsType<FontSizeInstruction>(program.Instructions[1]);
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
}
