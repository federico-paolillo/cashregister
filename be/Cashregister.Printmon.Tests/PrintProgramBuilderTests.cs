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
}
