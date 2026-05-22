namespace Cashregister.Printmon.Devices;

public sealed class FileDeviceTargetStore
{
    private string? _target;

    public string? CurrentTarget => Volatile.Read(ref _target);

    public void Select(string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        Interlocked.Exchange(ref _target, target);
    }
}