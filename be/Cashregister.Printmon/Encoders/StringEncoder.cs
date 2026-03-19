using System.Globalization;
using System.Text;

using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

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
                case SelectPrintModeInstruction selectPrintMode:
                    sb.Append("[PRINT_MODE:");
                    sb.Append(selectPrintMode.UseFontB ? "FONT_B" : "FONT_A");
                    if (selectPrintMode.Flags.HasFlag(FormatMode.Emphasized))
                        sb.Append(",EMPHASIZED");
                    if (selectPrintMode.Flags.HasFlag(FormatMode.DoubleHeight))
                        sb.Append(",DOUBLE_HEIGHT");
                    if (selectPrintMode.Flags.HasFlag(FormatMode.DoubleWidth))
                        sb.Append(",DOUBLE_WIDTH");
                    if (selectPrintMode.Flags.HasFlag(FormatMode.Underline))
                        sb.Append(",UNDERLINE");
                    sb.Append(']');
                    break;
                case UnderlineInstruction underline:
                    if (!underline.Enabled)
                        sb.Append("[UNDERLINE:OFF]");
                    else
                        sb.Append(underline.Thickness == Thickness.OneDot ? "[UNDERLINE:1DOT]" : "[UNDERLINE:2DOT]");
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
                case FontSizeInstruction fontSize:
                    int w = (fontSize.Size >> 4) + 1;
                    int h = (fontSize.Size & 0x0F) + 1;
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
                case TextInstruction text:
                    sb.Append(text.Text);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(sb.ToString());
    }
}
