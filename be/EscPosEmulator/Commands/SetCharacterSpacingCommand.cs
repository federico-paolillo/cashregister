namespace EscPosEmulator.Commands;

internal sealed class SetCharacterSpacingCommand : IEscPosCommand
{
    private readonly int _spacing;
    public SetCharacterSpacingCommand(int spacing) => _spacing = spacing;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.CharacterSpacing = _spacing;
    }
}
