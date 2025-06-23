namespace EscPosEmulator.Commands;

internal sealed class SetRotationCommand : IEscPosCommand
{
    private readonly int _rotation;
    public SetRotationCommand(int rotation) => _rotation = rotation;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Rotation = _rotation;
    }
}
