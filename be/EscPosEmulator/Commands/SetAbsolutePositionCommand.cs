namespace EscPosEmulator.Commands;

internal sealed class SetAbsolutePositionCommand : IEscPosCommand
{
    private readonly int _position;
    public SetAbsolutePositionCommand(int position) => _position = position;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.AbsolutePosition = _position;
    }
}
