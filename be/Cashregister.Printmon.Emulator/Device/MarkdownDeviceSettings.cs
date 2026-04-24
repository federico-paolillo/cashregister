namespace Cashregister.Printmon.Emulator.Device;

/// <summary>
///     Configures where <see cref="MarkdownDevice" /> writes rendered markdown receipts.
/// </summary>
public sealed class MarkdownDeviceSettings
{
    public const string Section = "MarkdownDevice";

    public required string RootFolder { get; set; } = "/tmp";
}