using Cashregister.Printmon.Devices;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cashregister.Tests.Integration.Utilities;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDevice(this IServiceCollection serviceCollection, IDevice device)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        serviceCollection.RemoveAll<IDevice>();
        serviceCollection.AddSingleton(device);

        return serviceCollection;
    }
}