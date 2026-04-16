namespace Cashregister.Printmon.Instructions.Layout;

public enum Alignment
{
    Left = 0,
    Center = 1,
    Right = 2
}

public sealed record JustifyInstruction(Alignment Alignment) : Instruction
{
    public Alignment Alignment { get; } = !Enum.IsDefined(Alignment)
        ? throw new ArgumentOutOfRangeException(nameof(Alignment), Alignment,
            "Alignment must be Left (0), Center (1), or Right (2).")
        : Alignment;
}