namespace EscPosEmulator.Commands;

internal sealed class SetLineSpacingCommand : IEscPosCommand
{
    private readonly int _spacing;
    public SetLineSpacingCommand(int spacing) => _spacing = spacing;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.LineSpacing = _spacing == -1 ? FormattingState.DefaultLineSpacing : _spacing;
    }
}
