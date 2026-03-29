using System.Collections.Immutable;

using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class MarkdownRendererTests
{
    private readonly MarkdownRenderer _renderer = new();

    private static Receipt Rec(params IDocumentElement[] elements) =>
        new(elements.ToImmutableArray());

    private static TextStyle Style(
        bool bold = false,
        Thickness underline = Thickness.None,
        bool doubleStrike = false,
        CharacterFont font = CharacterFont.A,
        bool rotation = false,
        bool upsideDown = false,
        bool reverse = false,
        byte widthMultiplier = 1,
        byte heightMultiplier = 1,
        Alignment justification = Alignment.Left) =>
        new(bold, underline, doubleStrike, font, rotation, upsideDown, reverse,
            widthMultiplier, heightMultiplier, justification);

    // ---- Plain text ----

    [Fact]
    public void Render_PlainText_OutputsTextAsIs()
    {
        var receipt = Rec(new TextSpan("hello", Style()));

        var md = _renderer.Render(receipt);

        Assert.Equal("hello", md);
    }

    // ---- Bold ----

    [Fact]
    public void Render_BoldText_WrapsWithDoubleAsterisks()
    {
        var receipt = Rec(new TextSpan("bold", Style(bold: true)));

        var md = _renderer.Render(receipt);

        Assert.Equal("**bold**", md);
    }

    [Fact]
    public void Render_DoubleStrikeText_WrapsWithDoubleAsterisks()
    {
        var receipt = Rec(new TextSpan("struck", Style(doubleStrike: true)));

        var md = _renderer.Render(receipt);

        Assert.Equal("**struck**", md);
    }

    // ---- Underline ----

    [Fact]
    public void Render_UnderlineText_WrapsWithHtmlUnderlineTag()
    {
        var receipt = Rec(new TextSpan("under", Style(underline: Thickness.Thin)));

        var md = _renderer.Render(receipt);

        Assert.Equal("<u>under</u>", md);
    }

    // ---- Bold + underline ----

    [Fact]
    public void Render_BoldAndUnderline_WrapsUnderlineThenBold()
    {
        var receipt = Rec(new TextSpan("both", Style(bold: true, underline: Thickness.Thin)));

        var md = _renderer.Render(receipt);

        Assert.Equal("**<u>both</u>**", md);
    }

    // ---- Reverse ----

    [Fact]
    public void Render_ReverseText_WrapsWithBackticks()
    {
        var receipt = Rec(new TextSpan("inv", Style(reverse: true)));

        var md = _renderer.Render(receipt);

        Assert.Equal("`inv`", md);
    }

    // ---- Large text ----

    [Fact]
    public void Render_DoubleHeightText_PrependHeading()
    {
        var receipt = Rec(new TextSpan("big", Style(heightMultiplier: 2)));

        var md = _renderer.Render(receipt);

        Assert.Equal("## big", md);
    }

    // ---- Alignment ----

    [Fact]
    public void Render_CenteredText_WrapsWithCenterAlignParagraph()
    {
        var receipt = Rec(new TextSpan("center", Style(justification: Alignment.Center)));

        var md = _renderer.Render(receipt);

        Assert.Equal("<p align=\"center\">center</p>", md);
    }

    [Fact]
    public void Render_RightAlignedText_WrapsWithRightAlignParagraph()
    {
        var receipt = Rec(new TextSpan("right", Style(justification: Alignment.Right)));

        var md = _renderer.Render(receipt);

        Assert.Equal("<p align=\"right\">right</p>", md);
    }

    [Fact]
    public void Render_LeftAlignedText_NoWrapper()
    {
        var receipt = Rec(new TextSpan("left", Style(justification: Alignment.Left)));

        var md = _renderer.Render(receipt);

        Assert.Equal("left", md);
    }

    // ---- LineBreak ----

    [Fact]
    public void Render_LineBreak_OutputsNewline()
    {
        var receipt = Rec(new LineBreak());

        var md = _renderer.Render(receipt);

        Assert.Equal(Environment.NewLine, md);
    }

    // ---- FeedLines ----

    [Fact]
    public void Render_FeedLines2_OutputsTwoBlankLines()
    {
        var receipt = Rec(new FeedLines(2));

        var md = _renderer.Render(receipt);

        // AppendLine() adds Environment.NewLine per iteration
        Assert.Equal(Environment.NewLine + Environment.NewLine, md);
    }

    // ---- HorizontalRule ----

    [Fact]
    public void Render_HorizontalRule_OutputsThreeDashes()
    {
        var receipt = Rec(new HorizontalRule());

        var md = _renderer.Render(receipt);

        Assert.Equal("---" + Environment.NewLine, md);
    }

    // ---- Placeholders ----

    [Fact]
    public void Render_Barcode_OutputsBarcodeToken()
    {
        var receipt = Rec(new Barcode());

        var md = _renderer.Render(receipt);

        Assert.Contains("[BARCODE]", md, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_QrCode_OutputsQrToken()
    {
        var receipt = Rec(new QrCode());

        var md = _renderer.Render(receipt);

        Assert.Contains("[QR]", md, StringComparison.Ordinal);
    }

    // ---- Centered bold heading ----

    [Fact]
    public void Render_CenteredBoldText_AppliesBoldAndAlignment()
    {
        var receipt = Rec(new TextSpan("TITLE", Style(bold: true, justification: Alignment.Center)));

        var md = _renderer.Render(receipt);

        Assert.Equal("<p align=\"center\">**TITLE**</p>", md);
    }

    // ---- Multi-element document ----

    [Fact]
    public void Render_FullReceipt_ProducesExpectedMarkdown()
    {
        var receipt = Rec(
            new TextSpan("SHOP", Style(bold: true, justification: Alignment.Center)),
            new LineBreak(),
            new TextSpan("Item 1", Style()),
            new LineBreak(),
            new HorizontalRule());

        var md = _renderer.Render(receipt);

        Assert.Contains("<p align=\"center\">**SHOP**</p>", md, StringComparison.Ordinal);
        Assert.Contains("Item 1", md, StringComparison.Ordinal);
        Assert.Contains("---", md, StringComparison.Ordinal);
    }

    // ---- End-to-end: builder -> encoder -> decoder -> executor -> emulator -> renderer ----

    [Fact]
    public void Render_EndToEnd_BuilderEncoderDecoderExecutorRenderer()
    {
        var builder = new PrintProgramBuilder();
        builder.Align(Alignment.Center).BoldOn().PrintLine("RECEIPT").BoldOff();
        var bytes = new BinaryEncoder().Encode(builder.Build()).Value;

        var emulator = new PrinterEmulator(new InstructionDecoder(), new InstructionExecutor());
        var history = emulator.Emulate(bytes).Value!;
        var md = _renderer.Render(history[^1].Receipt);

        Assert.Contains("RECEIPT", md, StringComparison.Ordinal);
        Assert.Contains("---", md, StringComparison.Ordinal);
    }
}
