namespace Cashregister.Printmon.Instructions.Formatting;

public sealed record FontSizeInstruction(byte Size) : Instruction
{
    public byte Size { get; } = (Size & 0x88) != 0
        ? throw new ArgumentOutOfRangeException(nameof(Size), Size,
            "Width (bits 4-7) and height (bits 0-3) multipliers must each be 0-7.")
        : Size;
}
