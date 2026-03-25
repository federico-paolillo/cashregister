using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Peripheral;

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
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithInitToken()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][NOP][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UseFontA_NoFlags_ProducesFontAToken()
    {
        var program = new PrintProgramBuilder().UseFontA(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PRINT_MODE:FONT_A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UseFontB_NoFlags_ProducesFontBToken()
    {
        var program = new PrintProgramBuilder().UseFontB(FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PRINT_MODE:FONT_B][LF][CUT_AFTER:1]", result.Value);
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
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PRINT_MODE:FONT_A,EMPHASIZED,UNDERLINE][LF][CUT_AFTER:1]",
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
            "[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PRINT_MODE:FONT_B,EMPHASIZED,DOUBLE_HEIGHT,DOUBLE_WIDTH,UNDERLINE][LF][CUT_AFTER:1]",
            result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesOffToken()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][UNDERLINE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnOneDot_Produces1DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.OneDot).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][UNDERLINE:1DOT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnTwoDots_Produces2DotToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.TwoDots).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][UNDERLINE:2DOT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOn_ProducesBoldOnToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][BOLD:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_EmphasizeOff_ProducesBoldOffToken()
    {
        var program = new PrintProgramBuilder().EmphasizeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][BOLD:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][DOUBLE_STRIKE:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][DOUBLE_STRIKE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontA_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontA().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FONT:A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SelectFontB_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SelectFontB().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FONT:B][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ROTATE_90:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_NinetyDegsOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().NinetyDegsOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ROTATE_90:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][UPSIDE_DOWN:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][UPSIDE_DOWN:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x00).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FONT_SIZE:1x1][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(0x11).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FONT_SIZE:2x2][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyLeft_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Left).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ALIGN:LEFT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyCenter_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Center).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ALIGN:CENTER][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_JustifyRight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Justify(Justification.Right).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ALIGN:RIGHT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_AbsolutePosition_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetAbsolutePosition(1000).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][ABS_POS:1000][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_RelativePosition_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetRelativePosition(500).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][REL_POS:500][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LeftMargin_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetLeftMargin(200).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][LEFT_MARGIN:200][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_Text_EmitsTextAsIs()
    {
        var program = new PrintProgramBuilder().Text("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE]Hello[LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_HorizontalTab_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().HorizontalTab().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][HT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LineFeed_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().LineFeed().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintLine_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().PrintLine("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE]Hello[LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_CutAfter_WithCustomDistance_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().CutAfter(50).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][CUT_AFTER:50][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ResetLineSpacing_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ResetLineSpacing().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][LINE_SPACING:DEFAULT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SetLineSpacing_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetLineSpacing(30).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][LINE_SPACING:30][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ReverseOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ReverseOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][REVERSE:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ReverseOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ReverseOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][REVERSE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_RightSpacing_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetRightSpacing(20).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][RIGHT_SPACING:20][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SetHorizontalTabs_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetHorizontalTabs(8, 16, 24).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][SET_TABS:8,16,24][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ClearHorizontalTabs_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ClearHorizontalTabs().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][SET_TABS:CLEAR][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FeedLines_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FeedLines(5).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FEED_LINES:5][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FeedPaper_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FeedPaper(100).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][FEED_PAPER:100][LF][CUT_AFTER:1]", result.Value);
    }

    /// <summary>
    ///     Tab positions are absolute columns. Each HT advances to the next stop
    ///     to the right of the current position. After a line feed the position
    ///     resets to column 0, so the stops cycle again on the next line.
    /// </summary>
    [Fact]
    public void Encode_KickDrawer_Pin2_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin2, 25, 250).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PULSE:PIN2,ON=25,OFF=250][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_KickDrawer_Pin5_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin5, 10, 20).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE][PULSE:PIN5,ON=10,OFF=20][LF][CUT_AFTER:1]", result.Value);
    }

    /// <summary>
    ///     Tab positions are absolute columns. Each HT advances to the next stop
    ///     to the right of the current position. After a line feed the position
    ///     resets to column 0, so the stops cycle again on the next line.
    /// </summary>
    [Fact]
    public void Encode_SetHorizontalTabs_ThenTextWithTabs_ProducesCorrectSequence()
    {
        // col 0   → "A" → HT jumps to col 4
        // col 4   → "B" → HT jumps to col 8
        // col 8   → "C" → HT jumps to col 16
        // col 16  → "D" → LF resets to col 0
        var program = new PrintProgramBuilder()
            .SetHorizontalTabs(4, 8, 16)
            .Text("A").HorizontalTab()
            .Text("B").HorizontalTab()
            .Text("C").HorizontalTab()
            .PrintLine("D")
            .ClearHorizontalTabs()
            .Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal(
            "[INIT][CODE_TABLE:STD_EUROPE][RESET_PRINT_MODE]" +
            "[SET_TABS:4,8,16]" +
            "A[HT]B[HT]C[HT]D[LF]" +
            "[SET_TABS:CLEAR]" +
            "[LF][CUT_AFTER:1]",
            result.Value);
    }
}