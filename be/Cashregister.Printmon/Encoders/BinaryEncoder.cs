using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

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
                case EmphasizeInstruction emphasize:
                    byte emphasizeN = emphasize.Enabled ? (byte)0x01 : (byte)0x00;
                    stream.Write([0x1B, 0x45, emphasizeN]); // ESC E n
                    break;
                case DoubleStrikeInstruction doubleStrike:
                    byte doubleStrikeN = doubleStrike.Enabled ? (byte)0x01 : (byte)0x00;
                    stream.Write([0x1B, 0x47, doubleStrikeN]); // ESC G n
                    break;
                case SelectFontInstruction selectFont:
                    stream.Write([0x1B, 0x4D, (byte)selectFont.Font]); // ESC M n
                    break;
                case RotationInstruction rotation:
                    byte rotationN = rotation.Enabled ? (byte)0x01 : (byte)0x00;
                    stream.Write([0x1B, 0x56, rotationN]); // ESC V n
                    break;
                case UpsideDownInstruction upsideDown:
                    byte upsideDownN = upsideDown.Enabled ? (byte)0x01 : (byte)0x00;
                    stream.Write([0x1B, 0x7B, upsideDownN]); // ESC { n
                    break;
                case FontSizeInstruction fontSize:
                    stream.Write([0x1D, 0x21, fontSize.Size]); // GS ! n
                    break;
                case JustifyInstruction justify:
                    stream.Write([0x1B, 0x61, (byte)justify.Justification]); // ESC a n
                    break;
                case AbsolutePositionInstruction absPos:
                    stream.Write([0x1B, 0x24, (byte)(absPos.Position & 0xFF), (byte)(absPos.Position >> 8)]); // ESC $ nL nH
                    break;
                case RelativePositionInstruction relPos:
                    stream.Write([0x1B, 0x5C, (byte)(relPos.Offset & 0xFF), (byte)(relPos.Offset >> 8)]); // ESC \ nL nH
                    break;
                case LeftMarginInstruction leftMargin:
                    stream.Write([0x1D, 0x4C, (byte)(leftMargin.Margin & 0xFF), (byte)(leftMargin.Margin >> 8)]); // GS L nL nH
                    break;
                case TextInstruction text:
                    stream.Write(System.Text.Encoding.ASCII.GetBytes(text.Text));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(stream.ToArray());
    }
}
