namespace Cashregister.Printmon.Instructions.CodePage;

public enum CharacterCodePage
{
    OEM437 = 0,
    Katakana = 1,
    OEM850 = 2,
    OEM860 = 3,
    OEM863 = 4,
    OEM865 = 5,
    WPC1252 = 16,
    OEM866 = 17,
    OEM852 = 18,
    OEM858 = 19,
    Thai = 255
}

public sealed record SelectCodePageInstruction(CharacterCodePage Page) : Instruction
{
    public CharacterCodePage Page { get; } = !Enum.IsDefined(Page)
        ? throw new ArgumentOutOfRangeException(nameof(Page), Page, "Invalid code page.")
        : Page;
}