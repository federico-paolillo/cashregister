using Cashregister.Application.Devices.Models;

namespace Cashregister.Application.Devices.Services.Defaults;

public sealed class FilePrinterDeviceCatalog : IPrinterDeviceCatalog
{
    private static readonly string[] SearchPatterns =
    [
        "/dev/usb/lp*",
        "/dev/lp*"
    ];

    public Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken)
    {
        var devices = SearchPatterns
            .SelectMany(pattern => EnumerateDevicePaths(pattern, cancellationToken))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(path => new PrinterDevice(
                DeviceId.FromTarget(path),
                Path.GetFileName(path),
                path,
                "Printer file device"
            ))
            .ToArray();

        return Task.FromResult<IReadOnlyList<PrinterDevice>>(devices);
    }

    private static IEnumerable<string> EnumerateDevicePaths(string pattern, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(pattern);
        var searchPattern = Path.GetFileName(pattern);

        if (directory is null || searchPattern.Length == 0 || !Directory.Exists(directory))
        {
            yield break;
        }

        IEnumerable<string> paths;

        try
        {
            paths = Directory.EnumerateFiles(directory, searchPattern);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            yield break;
        }

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (HasWritePermissionBit(path))
            {
                yield return path;
            }
        }
    }

    private static bool HasWritePermissionBit(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            var mode = File.GetUnixFileMode(path);

            return mode.HasFlag(UnixFileMode.UserWrite)
                || mode.HasFlag(UnixFileMode.GroupWrite)
                || mode.HasFlag(UnixFileMode.OtherWrite);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            return false;
        }
    }
}