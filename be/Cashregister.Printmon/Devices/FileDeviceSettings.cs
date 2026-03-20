namespace Cashregister.Printmon.Devices;

public sealed class FileDeviceSettings
{
    public const string Section = "FileDevice";

    public required string Target { get; set; }
}
