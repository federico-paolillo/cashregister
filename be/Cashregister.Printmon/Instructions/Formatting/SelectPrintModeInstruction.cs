namespace Cashregister.Printmon.Instructions.Formatting;

[Flags]
public enum FormatMode
{
    None = 0,
    Emphasized = 0x08,
    DoubleHeight = 0x10,
    DoubleWidth = 0x20,
    Underline = 0x80,
}

public sealed record SelectPrintModeInstruction(bool UseFontB, FormatMode Flags) : Instruction;
