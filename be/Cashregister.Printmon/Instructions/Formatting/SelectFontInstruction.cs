namespace Cashregister.Printmon.Instructions.Formatting;

public enum CharacterFont
{
    A = 0,
    B = 1
}

public sealed record SelectFontInstruction(CharacterFont Font) : Instruction
{
    public CharacterFont Font { get; } = !Enum.IsDefined(Font)
        ? throw new ArgumentOutOfRangeException(nameof(Font), Font, "Font must be A (0) or B (1).")
        : Font;
}