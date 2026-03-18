using Cashregister.Printmon;
using Cashregister.Printmon.Encoders;

namespace Cashregister.Printmon.Tests.Encoders;

public sealed class BinaryEncoderTests
{
    [Fact]
    public void Encode_AutoInitOnly_ProducesEscAtBytes()
    {
        var program = new PrintProgramBuilder().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40], result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithEscAtBytes()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40], result.Value);
    }
}
