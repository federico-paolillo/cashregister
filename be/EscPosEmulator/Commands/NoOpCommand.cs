namespace EscPosEmulator.Commands;

/// <summary>
/// Represents a command that performs no action.
/// </summary>
internal sealed class NoOpCommand : IEscPosCommand
{
    public static readonly NoOpCommand Instance = new();
    private NoOpCommand() { }
    public void Execute(CommandContext context) { }
}
