using Cashregister.Printmon;
using Cashregister.Printmon.Instructions.Core;

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
}
