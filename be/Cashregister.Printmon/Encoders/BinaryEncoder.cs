using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;

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
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(stream.ToArray());
    }
}
