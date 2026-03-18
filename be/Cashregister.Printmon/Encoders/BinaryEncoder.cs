using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;

namespace Cashregister.Printmon.Encoders;

public sealed class BinaryEncoder : IEncoder<byte[]>
{
    public Result<byte[]> Encode(PrintProgram printProgram)
    {
        ArgumentNullException.ThrowIfNull(printProgram);

        using var stream = new MemoryStream();

        foreach (var instruction in printProgram.Instructions)
        {
            switch (instruction)
            {
                case NoOpInstruction:
                    break;
                case InitializeInstruction:
                    stream.Write([0x1B, 0x40]); // ESC @
                    break;
                case SelectPrintModeInstruction selectPrintMode:
                    byte printModeN = (byte)((selectPrintMode.UseFontB ? 0x01 : 0x00) | (int)selectPrintMode.Flags);
                    stream.Write([0x1B, 0x21, printModeN]); // ESC ! n
                    break;
                case UnderlineInstruction underline:
                    byte underlineN = underline.Enabled ? (byte)underline.Thickness : (byte)0;
                    stream.Write([0x1B, 0x2D, underlineN]); // ESC - n
                    break;
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(stream.ToArray());
    }
}
