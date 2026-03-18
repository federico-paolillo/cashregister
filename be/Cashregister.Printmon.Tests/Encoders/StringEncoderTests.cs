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

    [Fact]
    public void Encode_EmphasizeOn_ProducesBoldOnToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][BOLD:ON]", result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOff_ProducesBoldOffToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][BOLD:OFF]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][DOUBLE_STRIKE:ON]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][DOUBLE_STRIKE:OFF]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontA_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontA().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][FONT:A]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontB_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontB().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][FONT:B]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][ROTATE_90:ON]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][ROTATE_90:OFF]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][UPSIDE_DOWN:ON]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][UPSIDE_DOWN:OFF]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x00).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][FONT_SIZE:1x1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x11).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][FONT_SIZE:2x2]", result.Value);
    }
}
