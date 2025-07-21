namespace Cashregister.Factories.Problems;

public sealed record UnhandledExceptionProblem(Exception Exception) : Problem;