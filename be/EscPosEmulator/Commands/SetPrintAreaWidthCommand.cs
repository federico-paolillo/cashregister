namespace EscPosEmulator.Commands;

internal sealed class SetPrintAreaWidthCommand : IEscPosCommand
{
    private readonly int _width;
    public SetPrintAreaWidthCommand(int width) => _width = width;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.PrintAreaWidth = _width;
    }
}
