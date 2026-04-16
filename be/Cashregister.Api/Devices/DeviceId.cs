using System.Buffers.Text;
using System.Text;

namespace Cashregister.Api.Devices;

internal static class DeviceId
{
    public static string FromTarget(string target)
    {
        var bytes = Encoding.UTF8.GetBytes(target);

        return Base64Url.EncodeToString(bytes);
    }
}