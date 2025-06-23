using EscPosEmulator;
using HtmlAgilityPack;

namespace EscPosEmulator.Tests;

public class EscPosConverterTests
{
    [Fact]
    public void Convert_ShouldRenderLines()
    {
        byte[] data = [
            (byte)'A', (byte)'B', (byte)'C', 0x0A,
            (byte)'D', (byte)'E', (byte)'F', 0x0A
        ];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var lines = doc.DocumentNode.SelectNodes("//div[contains(@class,'line')]");
        Assert.NotNull(lines);
        Assert.Equal(3, lines!.Count); // two LFs produce three lines
    }

    [Fact]
    public void Convert_ShouldApplyBold()
    {
        byte[] data = [0x1B, (byte)'E', 1, (byte)'B', (byte)'o', (byte)'l', (byte)'d', 0x1B, (byte)'E', 0, 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var span = doc.DocumentNode.SelectSingleNode("//span[contains(@style,'font-weight:bold')]");
        Assert.NotNull(span);
    }

    [Fact]
    public void Convert_ShouldCenterJustification()
    {
        byte[] data = [0x1B, (byte)'a', 1, (byte)'H', (byte)'i', 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var line = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'center')]");
        Assert.NotNull(line);
    }

    [Fact]
    public void Convert_ShouldUnderline()
    {
        byte[] data = [0x1B, (byte)'-', 1, (byte)'U', (byte)'n', (byte)'d', (byte)'e', (byte)'r', 0x1B, (byte)'-', 0, 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var span = doc.DocumentNode.SelectSingleNode("//span[contains(@style,'text-decoration:underline')]");
        Assert.NotNull(span);
    }

    [Fact]
    public void Convert_ShouldUpsideDown()
    {
        byte[] data = [0x1B, (byte)'{', 1, (byte)'T', (byte)'e', (byte)'x', (byte)'t', 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var span = doc.DocumentNode.SelectSingleNode("//span[contains(@style,'rotate(180deg)')]");
        Assert.NotNull(span);
    }

    [Fact]
    public void Convert_ShouldInvert()
    {
        byte[] data = [0x1D, (byte)'B', 1, (byte)'I', (byte)'n', (byte)'v', (byte)'e', (byte)'r', (byte)'t', 0x1D, (byte)'B', 0, 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var span = doc.DocumentNode.SelectSingleNode("//span[contains(@style,'filter:invert(1)')]");
        Assert.NotNull(span);
    }

    [Fact]
    public void Convert_ShouldScaleFromGsExclamation()
    {
        byte[] data = [0x1D, (byte)'!', 0x11, (byte)'S', (byte)'z', 0x0A];
        EscPosHtmlConverter conv = new();
        string html = conv.Convert(data);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var span = doc.DocumentNode.SelectSingleNode("//span[contains(@style,'scale(2,2)')]");
        Assert.NotNull(span);
    }
}
