using System.Text;

using Cashregister.Factories;
using Cashregister.Printmon.Instructions.Core;

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
                default:
                    throw new NotSupportedException(
                        $"Instruction {instruction.GetType().Name} is not supported by this encoder.");
            }
        }

        return Result.Ok(sb.ToString());
    }
}
