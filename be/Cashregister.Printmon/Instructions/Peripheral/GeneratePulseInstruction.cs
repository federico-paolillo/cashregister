namespace Cashregister.Printmon.Instructions.Peripheral;

public enum ConnectorPin
{
    Pin2 = 0,
    Pin5 = 1
}

public sealed record GeneratePulseInstruction(ConnectorPin Pin, byte OnTime, byte OffTime) : Instruction
{
    public ConnectorPin Pin { get; } = !Enum.IsDefined(Pin)
        ? throw new ArgumentOutOfRangeException(nameof(Pin), Pin, "Pin must be Pin2 (0) or Pin5 (1).")
        : Pin;
}
