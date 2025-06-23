namespace EscPosEmulator.Commands;

internal sealed class SetLeftMarginCommand : IEscPosCommand
{
    private readonly int _margin;
    public SetLeftMarginCommand(int margin) => _margin = margin;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.LeftMargin = _margin;
    }
}
