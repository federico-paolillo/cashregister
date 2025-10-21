namespace EscPosEmulator.Commands;

internal sealed class SetUpsideDownCommand : IEscPosCommand
{
    private readonly bool _on;
    public SetUpsideDownCommand(bool on) => _on = on;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.UpsideDown = _on;
    }
}
