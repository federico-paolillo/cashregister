namespace Cashregister.Printmon.Instructions.Core;

public sealed record TextInstruction : Instruction
{
    public string Text { get; }

    public TextInstruction(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        Text = text;
    }
}
