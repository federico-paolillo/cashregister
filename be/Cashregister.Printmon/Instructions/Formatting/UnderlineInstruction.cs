namespace Cashregister.Printmon.Instructions.Formatting;

public enum Thickness
{
    None = 0,
    OneDot = 1,
    TwoDots = 2,
}

public sealed record UnderlineInstruction(bool Enabled, Thickness Thickness) : Instruction
{
    public Thickness Thickness { get; } = Enabled && !Enum.IsDefined(Thickness)
        ? throw new ArgumentOutOfRangeException(nameof(Thickness), Thickness, "Thickness must be OneDot (1) or TwoDots (2).")
        : Thickness;
}
