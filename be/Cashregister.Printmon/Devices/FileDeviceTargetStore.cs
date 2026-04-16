using Microsoft.Extensions.Options;

namespace Cashregister.Printmon.Devices;

public sealed class FileDeviceTargetStore(
    IOptions<FileDeviceSettings> options
)
{
    private string _target = options.Value.Target;

    public string CurrentTarget => Volatile.Read(ref _target);

    public void Select(string target)
    {
        Interlocked.Exchange(ref _target, target);
    }
}