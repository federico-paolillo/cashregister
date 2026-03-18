using Cashregister.Printmon;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;

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

    [Fact]
    public void Encode_UseFontA_NoFlags_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UseFontA(FormatMode.None).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x21, 0x00], result.Value);
    }

    [Fact]
    public void Encode_UseFontB_NoFlags_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UseFontB(FormatMode.None).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x21, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UseFontA_WithEmphasizedAndDoubleHeight_ProducesCorrectBitmask()
    {
        var program = new PrintProgramBuilder()
            .UseFontA(FormatMode.Emphasized | FormatMode.DoubleHeight)
            .Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x21, 0x18], result.Value);
    }

    [Fact]
    public void Encode_UseFontB_WithAllFlags_ProducesCorrectBitmask()
    {
        var program = new PrintProgramBuilder()
            .UseFontB(FormatMode.Emphasized | FormatMode.DoubleHeight | FormatMode.DoubleWidth | FormatMode.Underline)
            .Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x21, 0xB9], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x2D, 0x00], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnOneDot_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.OneDot).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x2D, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnTwoDots_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.TwoDots).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x2D, 0x02], result.Value);
    }
}
