namespace EscPosEmulator.Commands;

internal sealed class SetCharacterSizeCommand : IEscPosCommand
{
    private readonly byte _value;
    public SetCharacterSizeCommand(byte value) => _value = value;
    public void Execute(CommandContext context)
    {
        context.Renderer.FlushBuffer(context.State);
        context.State.WidthMultiplier = (_value & 0x0F) + 1;
        context.State.HeightMultiplier = ((_value >> 4) & 0x0F) + 1;
    }
}
