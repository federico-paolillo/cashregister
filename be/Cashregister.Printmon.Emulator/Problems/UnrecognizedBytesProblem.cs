using Cashregister.Factories;

namespace Cashregister.Printmon.Emulator.Problems;

public sealed record UnrecognizedBytesProblem(int Offset, string Context) : Problem;
