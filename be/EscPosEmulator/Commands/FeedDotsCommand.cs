namespace EscPosEmulator.Commands;

internal sealed class FeedDotsCommand : IEscPosCommand
{
    private readonly int _dots;
    public FeedDotsCommand(int dots) => _dots = dots;
    public void Execute(CommandContext context)
    {
        // Approximate dot feed as line feed when dots >= line spacing
        int lines = Math.Max(1, _dots / FormattingState.DefaultLineSpacing);
        context.Renderer.LineFeed(context.State, lines);
    }
}
