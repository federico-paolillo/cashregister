namespace Cashregister.Printmon.Instructions.Layout;

public enum Justification
{
    Left = 0,
    Center = 1,
    Right = 2,
}

public sealed record JustifyInstruction(Justification Justification) : Instruction
{
    public Justification Justification { get; } = !Enum.IsDefined(Justification)
        ? throw new ArgumentOutOfRangeException(nameof(Justification), Justification, "Justification must be Left (0), Center (1), or Right (2).")
        : Justification;
}
