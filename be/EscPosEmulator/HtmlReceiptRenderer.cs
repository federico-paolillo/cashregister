using System.Globalization;
using System.Text;
using HtmlAgilityPack;

namespace EscPosEmulator;

/// <summary>
/// Builds an HTML representation of a receipt.
/// </summary>
internal sealed class HtmlReceiptRenderer
{
    private readonly HtmlDocument _document = new();
    private readonly HtmlNode _container;
    private HtmlNode _currentLine;
    private readonly StringBuilder _buffer = new();

    public HtmlReceiptRenderer()
    {
        _container = HtmlNode.CreateNode("<div class='escpos-receipt'></div>");
        _document.DocumentNode.AppendChild(_container);
        _currentLine = CreateLine(new FormattingState());
        _container.AppendChild(_currentLine);
    }

    public void AppendText(string text)
    {
        _buffer.Append(text);
    }

    public void AppendChar(char c)
    {
        _buffer.Append(c);
    }

    public void FlushBuffer(FormattingState state)
    {
        if (_buffer.Length == 0) return;
        HtmlNode span = CreateSpan(state, _buffer.ToString());
        _currentLine.AppendChild(span);
        _buffer.Clear();
    }

    public void LineFeed(FormattingState state, int count)
    {
        FlushBuffer(state);
        for (int i = 0; i < count; i++)
        {
            _currentLine = CreateLine(state);
            _container.AppendChild(_currentLine);
        }
    }

    public void NewLine(FormattingState state)
    {
        LineFeed(state, 1);
    }

    public string GetHtml(FormattingState state)
    {
        FlushBuffer(state);
        return _document.DocumentNode.OuterHtml;
    }

    private HtmlNode CreateLine(FormattingState state)
    {
        HtmlNode node = _document.CreateElement("div");
        node.SetAttributeValue("class", $"line {GetJust(state.Justification)}");
        StringBuilder style = new();
        style.Append(FormattableString.Invariant($"line-height:{state.LineSpacing}px;"));
        int padding = state.AbsolutePosition + state.LeftMargin + state.RelativePosition;
        if (padding > 0)
        {
            style.Append(FormattableString.Invariant($"padding-left:{padding}px;"));
        }
        node.SetAttributeValue("style", style.ToString());
        state.AbsolutePosition = 0;
        state.RelativePosition = 0;
        return node;
    }

    private HtmlNode CreateSpan(FormattingState state, string text)
    {
        HtmlNode span = _document.CreateElement("span");
        StringBuilder style = new();
        if (state.Bold) style.Append("font-weight:bold;");
        if (state.Underline) style.Append("text-decoration:underline;");
        if (state.CharacterSpacing > 0) style.Append(FormattableString.Invariant($"margin-right:{state.CharacterSpacing}px;"));
        double sx = state.DoubleWidth ? 2 : state.WidthMultiplier;
        double sy = state.DoubleHeight ? 2 : state.HeightMultiplier;
        string? transform = null;
        if (sx > 1 || sy > 1)
        {
            transform = FormattableString.Invariant($"scale({sx},{sy})");
        }
        if (state.UpsideDown)
        {
            transform = transform is null ? "rotate(180deg)" : $"{transform} rotate(180deg)";
        }
        if (state.Rotation != 0)
        {
            transform = transform is null ? $"rotate({state.Rotation}deg)" : $"{transform} rotate({state.Rotation}deg)";
        }
        if (transform is not null)
        {
            style.Append(FormattableString.Invariant($"transform:{transform};display:inline-block;"));
        }
        if (state.Invert)
        {
            style.Append("filter:invert(1);");
        }
        if (state.Font == 1)
        {
            style.Append("font-size:smaller;");
        }
        else if (state.Font == 2)
        {
            style.Append("font-size:small;");
        }
        span.SetAttributeValue("style", style.ToString());
        span.InnerHtml = HtmlEntity.Entitize(text);
        return span;
    }

    private static string GetJust(Justification just) => just switch
    {
        Justification.Center => "center",
        Justification.Right => "right",
        _ => "left"
    };
}
