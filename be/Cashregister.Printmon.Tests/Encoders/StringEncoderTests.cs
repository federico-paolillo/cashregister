using Cashregister.Printmon;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;

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

    [Fact]
    public void Encode_UseFontA_NoFlags_ProducesFontAToken()
    {
        var program = new PrintProgramBuilder().UseFontA(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][PRINT_MODE:FONT_A]", result.Value);
    }

    [Fact]
    public void Encode_UseFontB_NoFlags_ProducesFontBToken()
    {
        var program = new PrintProgramBuilder().UseFontB(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][PRINT_MODE:FONT_B]", result.Value);
    }

    [Fact]
    public void Encode_UseFontA_WithEmphasizedAndUnderline_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder()
            .UseFontA(FormatMode.Emphasized | FormatMode.Underline)
            .Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][PRINT_MODE:FONT_A,EMPHASIZED,UNDERLINE]", result.Value);
    }

    [Fact]
    public void Encode_UseFontB_WithAllFlags_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder()
            .UseFontB(FormatMode.Emphasized | FormatMode.DoubleHeight | FormatMode.DoubleWidth | FormatMode.Underline)
            .Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][PRINT_MODE:FONT_B,EMPHASIZED,DOUBLE_HEIGHT,DOUBLE_WIDTH,UNDERLINE]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesOffToken()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][UNDERLINE:OFF]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnOneDot_Produces1DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.OneDot).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][UNDERLINE:1DOT]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnTwoDots_Produces2DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.TwoDots).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][UNDERLINE:2DOT]", result.Value);
    }
}
