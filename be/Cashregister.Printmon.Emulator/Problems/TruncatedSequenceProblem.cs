using Cashregister.Factories;

namespace Cashregister.Printmon.Emulator.Problems;

public sealed record TruncatedSequenceProblem(int Offset, int Expected, int Available) : Problem;