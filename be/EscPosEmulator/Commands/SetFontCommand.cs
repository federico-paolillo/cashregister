namespace EscPosEmulator.Commands;

internal sealed class SetFontCommand : IEscPosCommand
{
    private readonly int _font;
    public SetFontCommand(byte font) => _font = font;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.Font = _font;
    }
}
