using Cashregister.Printmon;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

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

    [Fact]
    public void Encode_EmphasizeOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().EmphasizeOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x45, 0x01], result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().EmphasizeOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x45, 0x00], result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x47, 0x01], result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x47, 0x00], result.Value);
    }

    [Fact]
    public void Encode_SelectFontA_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SelectFontA().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x4D, 0x00], result.Value);
    }

    [Fact]
    public void Encode_SelectFontB_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SelectFontB().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x4D, 0x01], result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().NinetyDegsOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x56, 0x01], result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().NinetyDegsOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x56, 0x00], result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x7B, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x7B, 0x00], result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FontSize(0x00).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1D, 0x21, 0x00], result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FontSize(0x11).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1D, 0x21, 0x11], result.Value);
    }

    [Fact]
    public void Encode_JustifyLeft_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Left).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x61, 0x00], result.Value);
    }

    [Fact]
    public void Encode_JustifyCenter_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Center).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x61, 0x01], result.Value);
    }

    [Fact]
    public void Encode_JustifyRight_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Right).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x61, 0x02], result.Value);
    }

    [Fact]
    public void Encode_AbsolutePosition_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SetAbsolutePosition(0x0104).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x24, 0x04, 0x01], result.Value);
    }

    [Fact]
    public void Encode_AbsolutePosition_Zero_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SetAbsolutePosition(0).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x24, 0x00, 0x00], result.Value);
    }

    [Fact]
    public void Encode_RelativePosition_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SetRelativePosition(0x0304).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x5C, 0x04, 0x03], result.Value);
    }

    [Fact]
    public void Encode_LeftMargin_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SetLeftMargin(0x0200).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1D, 0x4C, 0x00, 0x02], result.Value);
    }

    [Fact]
    public void Encode_Text_ProducesAsciiBytes()
    {
        var program = new PrintProgramBuilder().Text("Hi").Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x48, 0x69], result.Value);
    }
}
