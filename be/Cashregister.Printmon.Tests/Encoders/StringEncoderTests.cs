using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

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
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithInitToken()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][NOP][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UseFontA_NoFlags_ProducesFontAToken()
    {
        var program = new PrintProgramBuilder().UseFontA(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][PRINT_MODE:FONT_A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UseFontB_NoFlags_ProducesFontBToken()
    {
        var program = new PrintProgramBuilder().UseFontB(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][PRINT_MODE:FONT_B][LF][CUT_AFTER:1]", result.Value);
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
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][PRINT_MODE:FONT_A,EMPHASIZED,UNDERLINE][LF][CUT_AFTER:1]",
            result.Value);
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
        Assert.Equal(
            "[INIT][CODE_TABLE:STD_EUROPE][PRINT_MODE:FONT_B,EMPHASIZED,DOUBLE_HEIGHT,DOUBLE_WIDTH,UNDERLINE][LF][CUT_AFTER:1]",
            result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesOffToken()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][UNDERLINE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnOneDot_Produces1DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.OneDot).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][UNDERLINE:1DOT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnTwoDots_Produces2DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.TwoDots).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][UNDERLINE:2DOT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOn_ProducesBoldOnToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][BOLD:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOff_ProducesBoldOffToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][BOLD:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][DOUBLE_STRIKE:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][DOUBLE_STRIKE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontA_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontA().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][FONT:A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontB_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontB().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][FONT:B][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ROTATE_90:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ROTATE_90:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][UPSIDE_DOWN:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][UPSIDE_DOWN:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x00).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][FONT_SIZE:1x1][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x11).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][FONT_SIZE:2x2][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyLeft_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Left).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ALIGN:LEFT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyCenter_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Center).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ALIGN:CENTER][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyRight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Right).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ALIGN:RIGHT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_AbsolutePosition_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetAbsolutePosition(1000).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][ABS_POS:1000][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_RelativePosition_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetRelativePosition(500).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][REL_POS:500][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LeftMargin_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetLeftMargin(200).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][LEFT_MARGIN:200][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_Text_EmitsTextAsIs()
    {
        var program = new PrintProgramBuilder().Text("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE]Hello[LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_HorizontalTab_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().HorizontalTab().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][HT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LineFeed_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().LineFeed().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintLine_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().PrintLine("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE]Hello[LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_CutAfter_WithCustomDistance_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().CutAfter(50).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][CUT_AFTER:50][LF][CUT_AFTER:1]", result.Value);
    }
}