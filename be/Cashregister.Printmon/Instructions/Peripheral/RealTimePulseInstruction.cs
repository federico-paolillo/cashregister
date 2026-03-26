namespace Cashregister.Printmon.Instructions.Peripheral;

public sealed record RealTimePulseInstruction(ConnectorPin Pin, byte Duration) : Instruction
{
    public ConnectorPin Pin { get; } = !Enum.IsDefined(Pin)
        ? throw new ArgumentOutOfRangeException(nameof(Pin), Pin, "Pin must be Pin2 (0) or Pin5 (1).")
        : Pin;

    public byte Duration { get; } = Duration is < 1 or > 8
        ? throw new ArgumentOutOfRangeException(nameof(Duration), Duration, "Duration must be 1-8.")
        : Duration;
}
