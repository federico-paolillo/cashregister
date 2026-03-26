namespace Cashregister.Printmon.Instructions.Formatting;

public enum Thickness
{
    None = 0,
    Thin = 1,
    Thick = 2
}

public sealed record UnderlineInstruction(bool Enabled, Thickness Thickness) : Instruction
{
    public Thickness Thickness { get; } = Enabled && !Enum.IsDefined(Thickness)
        ? throw new ArgumentOutOfRangeException(nameof(Thickness), Thickness,
            "Thickness must be Thin (1) or Thick (2).")
        : Thickness;
}
