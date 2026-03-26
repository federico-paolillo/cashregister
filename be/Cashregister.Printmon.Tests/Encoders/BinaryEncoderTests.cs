using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Peripheral;

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
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_WithNoOp_StartsWithEscAtBytes()
    {
        var program = new PrintProgramBuilder().NoOp().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontA_NoFlags_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().PrintMode(CharacterFont.A, FormatMode.None).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x21, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontB_NoFlags_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().PrintMode(CharacterFont.B, FormatMode.None).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x21, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontA_WithEmphasizedAndDoubleHeight_ProducesCorrectBitmask()
    {
        var program = new PrintProgramBuilder()
            .PrintMode(CharacterFont.A, FormatMode.Emphasized | FormatMode.DoubleHeight)
            .Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x21, 0x18, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_PrintMode_FontB_WithAllFlags_ProducesCorrectBitmask()
    {
        var program = new PrintProgramBuilder()
            .PrintMode(CharacterFont.B, FormatMode.Emphasized | FormatMode.DoubleHeight | FormatMode.DoubleWidth | FormatMode.Underline)
            .Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x21, 0xB9, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x2D, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnThin_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.Thin).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x2D, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UnderlineOnThick_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UnderlineOn(Thickness.Thick).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x2D, 0x02, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_BoldOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().BoldOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x45, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_BoldOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().BoldOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x45, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x47, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_DoubleStrikeOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().DoubleStrikeOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x47, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FontA_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Font(CharacterFont.A).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x4D, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FontB_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Font(CharacterFont.B).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x4D, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_RotateOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().RotateOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x56, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_RotateOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().RotateOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x56, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UpsideDownOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x7B, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_UpsideDownOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().UpsideDownOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x7B, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FontSize_NormalSize_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FontSize(1, 1).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x21, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FontSize_DoubleWidthAndHeight_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FontSize(2, 2).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x21, 0x11, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_AlignLeft_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Left).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x61, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_AlignCenter_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Center).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x61, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_AlignRight_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().Align(Alignment.Right).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x61, 0x02, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_MoveToColumn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().MoveToColumn(0x0104).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x24, 0x04, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_MoveToColumn_Zero_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().MoveToColumn(0).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x24, 0x00, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_MoveBy_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().MoveBy(0x0304).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x5C, 0x04, 0x03, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_LeftMargin_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().LeftMargin(0x0200).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x4C, 0x00, 0x02, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_Text_ProducesAsciiBytes()
    {
        var program = new PrintProgramBuilder().Text("Hi").Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x48, 0x69, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_HorizontalTab_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().HorizontalTab().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x09, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_LineFeed_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().LineFeed().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x0A, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_PrintLine_ProducesTextThenLineFeedBytes()
    {
        var program = new PrintProgramBuilder().PrintLine("Hi").Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x48, 0x69, 0x0A, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FeedAndCut_WithCustomLines_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FeedAndCut(50).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x56, 0x42, 0x32, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_ResetLineSpacing_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().ResetLineSpacing().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x32, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_SetLineSpacing_ProducesCorrectBytes()
    {
        // 3.75mm = 30 units
        var program = new PrintProgramBuilder().SetLineSpacing(3.75).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x33, 0x1E, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_InvertOn_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().InvertOn().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x42, 0x01, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_InvertOff_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().InvertOff().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1D, 0x42, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_SetCharacterSpacing_ProducesCorrectBytes()
    {
        // 2.5mm = 20 units
        var program = new PrintProgramBuilder().SetCharacterSpacing(2.5).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x20, 0x14, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_SetHorizontalTabs_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().SetHorizontalTabs(8, 16, 24).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x44, 0x08, 0x10, 0x18, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_ClearHorizontalTabs_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().ClearHorizontalTabs().Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x44, 0x00, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FeedLines_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().FeedLines(5).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x64, 0x05, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_FeedPaper_ProducesCorrectBytes()
    {
        // 12.5mm = 100 units
        var program = new PrintProgramBuilder().FeedPaper(12.5).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x4A, 0x64, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_KickDrawer_Pin2_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin2, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(500)).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x70, 0x00, 0x19, 0xFA, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }

    [Fact]
    public void Encode_KickDrawer_Pin5_ProducesCorrectBytes()
    {
        var program = new PrintProgramBuilder().KickDrawer(ConnectorPin.Pin5, TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(40)).Build();
        var encoder = new BinaryEncoder();

        var result = encoder.Encode(program);

        Assert.True(result.Ok);
        Assert.Equal([0x1B, 0x40, 0x1B, 0x74, 0x00, 0x1B, 0x21, 0x00, 0x1B, 0x70, 0x01, 0x0A, 0x14, 0x0A, 0x1D, 0x56, 0x42, 0x01], result.Value);
    }
}
