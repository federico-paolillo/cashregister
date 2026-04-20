namespace Cashregister.Printmon.Emulator;

public sealed record Printer(PrinterState State, Receipt Receipt)
{
    public static Printer Default { get; } = new(PrinterState.Default, new Receipt([]));
}