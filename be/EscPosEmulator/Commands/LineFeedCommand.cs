namespace EscPosEmulator.Commands;

internal sealed class LineFeedCommand : IEscPosCommand
{
    private readonly int _count;

    public LineFeedCommand(int count = 1)
    {
        _count = count;
    }

    public void Execute(CommandContext context)
    {
        context.Renderer.LineFeed(context.State, _count);
    }
}
