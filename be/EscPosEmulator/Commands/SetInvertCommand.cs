namespace EscPosEmulator.Commands;

internal sealed class SetInvertCommand : IEscPosCommand
{
    private readonly bool _on;
    public SetInvertCommand(bool on) => _on = on;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Invert = _on;
    }
}
