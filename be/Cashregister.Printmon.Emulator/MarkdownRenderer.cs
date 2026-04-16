using System.Text;

using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator;

public interface IMarkdownRenderer
{
    string Render(Receipt receipt);
}

public sealed class MarkdownRenderer : IMarkdownRenderer
{
    public string Render(Receipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        var sb = new StringBuilder();

        foreach (var element in receipt.Elements)
        {
            switch (element)
            {
                case TextSpan span:
                    sb.Append(FormatSpan(span));
                    break;

                case LineBreak:
                    sb.AppendLine();
                    break;

                case FeedLines feed:
                    for (var k = 0; k < feed.Count; k++)
                        sb.AppendLine();
                    break;

                case HorizontalRule:
                    sb.AppendLine("---");
                    break;

                case Barcode:
                    sb.AppendLine("[BARCODE]");
                    break;

                case QrCode:
                    sb.AppendLine("[QR]");
                    break;

                case Image:
                    sb.AppendLine("[IMAGE]");
                    break;

                default:
                    throw new NotSupportedException(
                        $"Document element {element.GetType().Name} is not supported by this renderer.");
            }
        }

        return sb.ToString();
    }

    private static string FormatSpan(TextSpan span)
    {
        var text = span.Text;
        var style = span.Style;

        // Apply inline formatting inside-out: reverse → underline → bold/double-strike
        if (style.Reverse)
            text = $"`{text}`";

        if (style.Underline != Thickness.None)
            text = $"<u>{text}</u>";

        if (style.Bold || style.DoubleStrike)
            text = $"**{text}**";

        // Large text rendered as heading
        if (style.HeightMultiplier >= 2)
            text = $"## {text}";

        // Alignment wrappers
        return style.Justification switch
        {
            Alignment.Center => $"<p align=\"center\">{text}</p>",
            Alignment.Right => $"<p align=\"right\">{text}</p>",
            _ => text
        };
    }
}