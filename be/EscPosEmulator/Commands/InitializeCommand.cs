namespace EscPosEmulator.Commands;

internal sealed class InitializeCommand : IEscPosCommand
{
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Reset();
    }
}
