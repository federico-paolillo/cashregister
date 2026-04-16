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
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithInitToken()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][NOP][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontA_NoFlags_ProducesFontAToken()
    {
        var program = new PrintProgramBuilder().PrintMode(CharacterFont.A, FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PRINT_MODE:FONT_A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontB_NoFlags_ProducesFontBToken()
    {
        var program = new PrintProgramBuilder().PrintMode(CharacterFont.B, FormatMode.None).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PRINT_MODE:FONT_B][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontA_WithEmphasizedAndUnderline_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder()
            .PrintMode(CharacterFont.A, FormatMode.Emphasized | FormatMode.Underline)
            .Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PRINT_MODE:FONT_A,EMPHASIZED,UNDERLINE][LF][CUT_AFTER:1]",
            result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontB_WithAllFlags_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder()
            .PrintMode(CharacterFont.B, FormatMode.Emphasized | FormatMode.DoubleHeight | FormatMode.DoubleWidth | FormatMode.Underline)
            .Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal(
            "[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PRINT_MODE:FONT_B,EMPHASIZED,DOUBLE_HEIGHT,DOUBLE_WIDTH,UNDERLINE][LF][CUT_AFTER:1]",
            result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesOffToken()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][UNDERLINE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnThin_ProducesThinToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.Thin).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][UNDERLINE:THIN][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnThick_ProducesThickToken()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.Thick).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][UNDERLINE:THICK][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_BoldOn_ProducesBoldOnToken()
    {
        var program = new PrintProgramBuilder().BoldOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][BOLD:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_BoldOff_ProducesBoldOffToken()
    {
        var program = new PrintProgramBuilder().BoldOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][BOLD:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][DOUBLE_STRIKE:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][DOUBLE_STRIKE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontA_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Font(CharacterFont.A).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FONT:A][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontB_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Font(CharacterFont.B).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FONT:B][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_RotateOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().RotateOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ROTATE_90:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_RotateOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().RotateOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ROTATE_90:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][UPSIDE_DOWN:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][UPSIDE_DOWN:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(1, 1).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FONT_SIZE:1x1][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FontSize(2, 2).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FONT_SIZE:2x2][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_AlignLeft_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Left).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ALIGN:LEFT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_AlignCenter_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Center).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ALIGN:CENTER][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_AlignRight_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Right).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ALIGN:RIGHT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_MoveToColumn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().MoveToColumn(1000).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][ABS_POS:1000][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_MoveBy_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().MoveBy(500).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][REL_POS:500][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LeftMargin_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().LeftMargin(200).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][LEFT_MARGIN:200][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_Text_EmitsTextAsIs()
    {
        var program = new PrintProgramBuilder().Text("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE]Hello[LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_HorizontalTab_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().HorizontalTab().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][HT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_LineFeed_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().LineFeed().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_PrintLine_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().PrintLine("Hello").Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE]Hello[LF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FeedAndCut_WithCustomLines_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FeedAndCut(50).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][CUT_AFTER:50][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ResetLineSpacing_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ResetLineSpacing().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][LINE_SPACING:DEFAULT][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SetLineSpacing_ProducesCorrectToken()
    {
        // 3.75mm = 30 units
        var program = new PrintProgramBuilder().SetLineSpacing(3.75).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][LINE_SPACING:30][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_InvertOn_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().InvertOn().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][REVERSE:ON][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_InvertOff_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().InvertOff().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][REVERSE:OFF][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SetCharacterSpacing_ProducesCorrectToken()
    {
        // 2.5mm = 20 units
        var program = new PrintProgramBuilder().SetCharacterSpacing(2.5).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][RIGHT_SPACING:20][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_SetHorizontalTabs_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().SetHorizontalTabs(8, 16, 24).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][SET_TABS:8,16,24][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_ClearHorizontalTabs_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().ClearHorizontalTabs().Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][SET_TABS:CLEAR][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FeedLines_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().FeedLines(5).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FEED_LINES:5][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_FeedPaper_ProducesCorrectToken()
    {
        // 12.5mm = 100 units
        var program = new PrintProgramBuilder().FeedPaper(12.5).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][FEED_PAPER:100][LF][CUT_AFTER:1]", result.Value);
    }

    /// <summary>
    ///     Tab positions are absolute columns. Each HT advances to the next stop
    ///     to the right of the current position. After a line feed the position
    ///     resets to column 0, so the stops cycle again on the next line.
    /// </summary>
    [Fact]
    public void Encode_KickDrawer_Pin2_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin2, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(500)).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PULSE:PIN2,ON=25,OFF=250][LF][CUT_AFTER:1]", result.Value);
    }

    [Fact]
    public void Encode_KickDrawer_Pin5_ProducesCorrectToken()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin5, TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(40)).Build();
        var encoder = new StringEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal("[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE][PULSE:PIN5,ON=10,OFF=20][LF][CUT_AFTER:1]", result.Value);
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
            "[INIT][CODE_PAGE:OEM437][RESET_PRINT_MODE]" +
            "[SET_TABS:4,8,16]" +
            "A[HT]B[HT]C[HT]D[LF]" +
            "[SET_TABS:CLEAR]" +
            "[LF][CUT_AFTER:1]",
            result.Value);
    }
}