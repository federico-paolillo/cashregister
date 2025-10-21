
namespace EscPosEmulator.Commands;

/// <summary>
/// Context data shared among commands during interpretation.
/// </summary>
internal sealed class CommandContext
{
    public FormattingState State { get; }
    public HtmlReceiptRenderer Renderer { get; }

    public CommandContext(FormattingState state, HtmlReceiptRenderer renderer)
    {
        State = state;
        Renderer = renderer;
    }
}
