namespace Cashregister.Printmon.Devices;

public sealed class RandomFileDeviceSettings
{
    public const string Section = "RandomDevice";

    public required string RootFolder { get; set; } = "/tmp";
}