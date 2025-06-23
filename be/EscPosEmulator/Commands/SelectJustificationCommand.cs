namespace EscPosEmulator.Commands;

internal sealed class SelectJustificationCommand : IEscPosCommand
{
    private readonly byte _value;
    public SelectJustificationCommand(byte value) => _value = value;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Justification = _value switch
        {
            1 or 49 => Justification.Center,
            2 or 50 => Justification.Right,
            _ => Justification.Left
        };
    }
}
