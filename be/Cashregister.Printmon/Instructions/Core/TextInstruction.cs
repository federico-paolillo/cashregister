namespace Cashregister.Printmon.Instructions.Core;

public sealed record TextInstruction : Instruction
{
    public TextInstruction(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        Text = text;
    }

    public string Text { get; }
}