using Cashregister.Factories;

namespace Cashregister.Printmon.Emulator.Problems;

public sealed record UnsupportedInstructionProblem(string InstructionType) : Problem;