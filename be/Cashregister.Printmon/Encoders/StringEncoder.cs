using System.Globalization;
using System.Text;

using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

namespace Cashregister.Printmon.Encoders;

public sealed class StringEncoder : IEncoder<string>
{
    public Result<string> Encode(PrintProgram printProgram)
    {
        ArgumentNullException.ThrowIfNull(printProgram);

        var sb = new StringBuilder();

        foreach (var instruction in printProgram.Instructions)
        {
            switch (instruction)
            {
                case NoOpInstruction:
                    sb.Append("[NOP]");
                    break;
                case InitializeInstruction:
                    sb.Append("[INIT]");
                    break;
                case ResetPrintModeInstruction:
                    sb.Append("[RESET_PRINT_MODE]");
                    break;
                case SelectPrintModeInstruction selectPrintMode:
                    sb.Append("[PRINT_MODE:");
                    sb.Append(selectPrintMode.UseFontB ? "FONT_B" : "FONT_A");
                    if (selectPrintMode.Flags.HasFlag(FormatMode.Emphasized))
                    {
                        sb.Append(",EMPHASIZED");
                    }

                    if (selectPrintMode.Flags.HasFlag(FormatMode.DoubleHeight))
                    {
                        sb.Append(",DOUBLE_HEIGHT");
                    }

                    if (selectPrintMode.Flags.HasFlag(FormatMode.DoubleWidth))
                    {
                        sb.Append(",DOUBLE_WIDTH");
                    }

                    if (selectPrintMode.Flags.HasFlag(FormatMode.Underline))
                    {
                        sb.Append(",UNDERLINE");
                    }

                    sb.Append(']');
                    break;
                case UnderlineInstruction underline:
                    if (!underline.Enabled)
                    {
                        sb.Append("[UNDERLINE:OFF]");
                    }
                    else
                    {
                        sb.Append(underline.Thickness == Thickness.OneDot ? "[UNDERLINE:1DOT]" : "[UNDERLINE:2DOT]");
                    }

                    break;
                case EmphasizeInstruction emphasize:
                    sb.Append(emphasize.Enabled ? "[BOLD:ON]" : "[BOLD:OFF]");
                    break;
                case DoubleStrikeInstruction doubleStrike:
                    sb.Append(doubleStrike.Enabled ? "[DOUBLE_STRIKE:ON]" : "[DOUBLE_STRIKE:OFF]");
                    break;
                case SelectFontInstruction selectFont:
                    sb.Append(selectFont.Font == CharacterFont.A ? "[FONT:A]" : "[FONT:B]");
                    break;
                case RotationInstruction rotation:
                    sb.Append(rotation.Enabled ? "[ROTATE_90:ON]" : "[ROTATE_90:OFF]");
                    break;
                case UpsideDownInstruction upsideDown:
                    sb.Append(upsideDown.Enabled ? "[UPSIDE_DOWN:ON]" : "[UPSIDE_DOWN:OFF]");
                    break;
                case ReverseInstruction reverse:
                    sb.Append(reverse.Enabled ? "[REVERSE:ON]" : "[REVERSE:OFF]");
                    break;
                case FontSizeInstruction fontSize:
                    var w = (fontSize.Size >> 4) + 1;
                    var h = (fontSize.Size & 0x0F) + 1;
                    sb.Append(CultureInfo.InvariantCulture, $"[FONT_SIZE:{w}x{h}]");
                    break;
                case JustifyInstruction justify:
                    switch (justify.Justification)
                    {
                        case Justification.Left:
                            sb.Append("[ALIGN:LEFT]");
                            break;
                        case Justification.Center:
                            sb.Append("[ALIGN:CENTER]");
                            break;
                        case Justification.Right:
                            sb.Append("[ALIGN:RIGHT]");
                            break;
                    }

                    break;
                case AbsolutePositionInstruction absPos:
                    sb.Append(CultureInfo.InvariantCulture, $"[ABS_POS:{absPos.Position}]");
                    break;
                case RelativePositionInstruction relPos:
                    sb.Append(CultureInfo.InvariantCulture, $"[REL_POS:{relPos.Offset}]");
                    break;
                case LeftMarginInstruction leftMargin:
                    sb.Append(CultureInfo.InvariantCulture, $"[LEFT_MARGIN:{leftMargin.Margin}]");
                    break;
                case RightSpacingInstruction rightSpacing:
                    sb.Append(CultureInfo.InvariantCulture, $"[RIGHT_SPACING:{rightSpacing.Spacing}]");
                    break;
                case TextInstruction text:
                    sb.Append(text.Text);
                    break;
                case SelectCodeTableInstruction:
                    sb.Append("[CODE_TABLE:STD_EUROPE]");
                    break;
                case SetHorizontalTabsInstruction setTabs:
                    if (setTabs.Positions.IsEmpty)
                    {
                        sb.Append("[SET_TABS:CLEAR]");
                    }
                    else
                    {
                        sb.Append("[SET_TABS:");
                        for (var i = 0; i < setTabs.Positions.Length; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(',');
                            }

                            sb.Append(CultureInfo.InvariantCulture, $"{setTabs.Positions[i]}");
                        }

                        sb.Append(']');
                    }

                    break;
                case HorizontalTabInstruction:
                    sb.Append("[HT]");
                    break;
                case ResetLineSpacingInstruction:
                    sb.Append("[LINE_SPACING:DEFAULT]");
                    break;
                case SetLineSpacingInstruction setLineSpacing:
                    sb.Append(CultureInfo.InvariantCulture, $"[LINE_SPACING:{setLineSpacing.Spacing}]");
                    break;
                case FeedLinesInstruction feedLines:
                    sb.Append(CultureInfo.InvariantCulture, $"[FEED_LINES:{feedLines.Lines}]");
                    break;
                case FeedPaperInstruction feedPaper:
                    sb.Append(CultureInfo.InvariantCulture, $"[FEED_PAPER:{feedPaper.Amount}]");
                    break;
                case LineFeedInstruction:
                    sb.Append("[LF]");
                    break;
                case PartialCutInstruction:
                    sb.Append("[CUT:PARTIAL]");
                    break;
                case CutAfterInstruction cutAfter:
                    sb.Append(CultureInfo.InvariantCulture, $"[CUT_AFTER:{cutAfter.Distance}]");
                    break;
                case GeneratePulseInstruction pulse:
                    sb.Append(CultureInfo.InvariantCulture, $"[PULSE:PIN{(pulse.Pin == ConnectorPin.Pin2 ? 2 : 5)},ON={pulse.OnTime},OFF={pulse.OffTime}]");
                    break;
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(sb.ToString());
    }
}