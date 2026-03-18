using Cashregister.Printmon;
using Cashregister.Printmon.Encoders;

namespace Cashregister.Printmon.Tests.Encoders;

public sealed class StringEncoderTests
{
    [Fact]
    public void Encode_AutoInitOnly_ProducesInitToken()
    {
        var program = new PrintProgramBuilder().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT]", result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithInitToken()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][NOP]", result.Value);
    }
}
