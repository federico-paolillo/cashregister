#pragma warning disable CA1812
using EscPosEmulator.Commands;

namespace EscPosEmulator;

/// <summary>
/// Parses ESC/POS byte streams and produces HTML using commands.
/// </summary>
public sealed class EscPosHtmlConverter
{
    private const byte ESC = 0x1B;
    private const byte GS = 0x1D;
    private const byte LF = 0x0A;
    private const byte CR = 0x0D;
    private const byte HT = 0x09;

    public string Convert(ReadOnlySpan<byte> data)
    {
        FormattingState state = new();
        HtmlReceiptRenderer renderer = new();
        CommandContext context = new(state, renderer);

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            if (b == ESC)
            {
                if (i + 1 >= data.Length) break;
                byte code = data[++i];
                switch (code)
                {
                    case (byte)'@':
                        new InitializeCommand().Execute(context);
                        break;
                    case (byte)'a':
                        if (i + 1 < data.Length)
                        {
                            new SelectJustificationCommand(data[++i]).Execute(context);
                        }
                        break;
                    case (byte)'E':
                        if (i + 1 < data.Length)
                        {
                            new SetBoldCommand(data[++i] == 1).Execute(context);
                        }
                        break;
                    case (byte)'-':
                        if (i + 1 < data.Length)
                        {
                            new SetUnderlineCommand(data[++i] != 0).Execute(context);
                        }
                        break;
                    case (byte)'d':
                        if (i + 1 < data.Length)
                        {
                            int n = data[++i];
                            new LineFeedCommand(n).Execute(context);
                        }
                        break;
                    case (byte)'J':
                        if (i + 1 < data.Length)
                        {
                            int n = data[++i];
                            new FeedDotsCommand(n).Execute(context);
                        }
                        break;
                    case (byte)'!':
                        if (i + 1 < data.Length)
                        {
                            byte mode = data[++i];
                            new SetBoldCommand((mode & 0x08) != 0).Execute(context);
                            context.State.DoubleHeight = (mode & 0x10) != 0;
                            context.State.DoubleWidth = (mode & 0x20) != 0;
                            context.State.Underline = (mode & 0x80) != 0;
                        }
                        break;
                    case (byte)'{':
                        if (i + 1 < data.Length)
                        {
                            new SetUpsideDownCommand(data[++i] == 1).Execute(context);
                        }
                        break;
                    case (byte)'G':
                        if (i + 1 < data.Length)
                        {
                            new SetBoldCommand(data[++i] == 1).Execute(context);
                        }
                        break;
                    case (byte)'M':
                        if (i + 1 < data.Length)
                        {
                            new SetFontCommand(data[++i]).Execute(context);
                        }
                        break;
                    case (byte)'2':
                        new SetLineSpacingCommand(-1).Execute(context);
                        break;
                    case (byte)'3':
                        if (i + 1 < data.Length)
                        {
                            new SetLineSpacingCommand(data[++i]).Execute(context);
                        }
                        break;
                    case (byte)'$':
                        if (i + 2 < data.Length)
                        {
                            int nL = data[++i];
                            int nH = data[++i];
                            new SetAbsolutePositionCommand((nH << 8) + nL).Execute(context);
                        }
                        break;
                    case (byte)'\\':
                        if (i + 2 < data.Length)
                        {
                            int nL = data[++i];
                            int nH = data[++i];
                            new SetRelativePositionCommand((nH << 8) + nL).Execute(context);
                        }
                        break;
                    case (byte)'V':
                        if (i + 1 < data.Length)
                        {
                            new SetRotationCommand(data[++i] == 1 ? 90 : 0).Execute(context);
                        }
                        break;
                    case (byte)' ': // ESC SP n
                        if (i + 1 < data.Length)
                        {
                            new SetCharacterSpacingCommand(data[++i]).Execute(context);
                        }
                        break;
                    default:
                        NoOpCommand.Instance.Execute(context);
                        break;
                }
            }
            else if (b == GS)
            {
                if (i + 1 >= data.Length) break;
                byte code = data[++i];
                switch (code)
                {
                    case (byte)'!':
                        if (i + 1 < data.Length)
                        {
                            new SetCharacterSizeCommand(data[++i]).Execute(context);
                        }
                        break;
                    case (byte)'B':
                        if (i + 1 < data.Length)
                        {
                            new SetInvertCommand(data[++i] == 1).Execute(context);
                        }
                        break;
                    case (byte)'L':
                        if (i + 2 < data.Length)
                        {
                            int nL = data[++i];
                            int nH = data[++i];
                            new SetLeftMarginCommand((nH << 8) + nL).Execute(context);
                        }
                        break;
                    case (byte)'W':
                        if (i + 2 < data.Length)
                        {
                            int nL = data[++i];
                            int nH = data[++i];
                            new SetPrintAreaWidthCommand((nH << 8) + nL).Execute(context);
                        }
                        break;
                    default:
                        NoOpCommand.Instance.Execute(context);
                        break;
                }
            }
            else if (b == LF)
            {
                new LineFeedCommand().Execute(context);
            }
            else if (b == CR)
            {
                // ignore
            }
            else if (b == HT)
            {
                new HorizontalTabCommand().Execute(context);
            }
            else if (b >= 0x20 || b == 0x00)
            {
                renderer.AppendChar((char)b);
            }
        }

        return renderer.GetHtml(state);
    }
}
