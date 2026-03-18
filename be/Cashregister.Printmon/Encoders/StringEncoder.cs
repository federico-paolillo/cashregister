using System.Text;

using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;

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
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(sb.ToString());
    }
}
