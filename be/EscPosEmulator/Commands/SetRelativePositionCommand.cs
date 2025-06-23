namespace EscPosEmulator.Commands;

internal sealed class SetRelativePositionCommand : IEscPosCommand
{
    private readonly int _position;
    public SetRelativePositionCommand(int position) => _position = position;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.RelativePosition = _position;
    }
}
