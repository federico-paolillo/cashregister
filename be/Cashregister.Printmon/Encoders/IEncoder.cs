using Cashregister.Factories;

namespace Cashregister.Printmon.Encoders;

/// <summary>
/// Takes a <see cref="PrintProgram"/> and converts it to a specific format (e.g. an array of bytes or a string)
/// </summary>
public interface IEncoder<TOutput>
{
    Result<TOutput> Encode(PrintProgram printProgram);
}