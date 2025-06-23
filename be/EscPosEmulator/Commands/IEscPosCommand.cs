namespace EscPosEmulator.Commands;

/// <summary>
/// Represents a single ESC/POS command.
/// </summary>
internal interface IEscPosCommand
{
    /// <summary>
    /// Executes the command using the provided context.
    /// </summary>
    /// <param name="context">Execution context.</param>
    void Execute(CommandContext context);
}
