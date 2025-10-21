namespace EscPosEmulator.Commands;

internal sealed class SetBoldCommand : IEscPosCommand
{
    private readonly bool _on;
    public SetBoldCommand(bool on) => _on = on;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Bold = _on;
    }
}
