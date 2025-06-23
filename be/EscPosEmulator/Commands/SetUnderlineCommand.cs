namespace EscPosEmulator.Commands;

internal sealed class SetUnderlineCommand : IEscPosCommand
{
    private readonly bool _on;
    public SetUnderlineCommand(bool on) => _on = on;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Underline = _on;
    }
}
