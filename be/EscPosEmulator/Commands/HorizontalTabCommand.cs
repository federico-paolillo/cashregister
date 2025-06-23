namespace EscPosEmulator.Commands;

internal sealed class HorizontalTabCommand : IEscPosCommand
{
    public void Execute(CommandContext context)
    {
        context.Renderer.AppendText("    ");
    }
}
