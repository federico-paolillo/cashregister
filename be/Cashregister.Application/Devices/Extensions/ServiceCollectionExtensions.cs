using Cashregister.Application.Devices.Defaults;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Encoders;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Application.Devices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashregisterDevices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<FileDeviceTargetStore>();
        serviceCollection.AddScoped<IPrinterDeviceCatalog, FilePrinterDeviceCatalog>();
        serviceCollection.AddScoped<FileDeviceTargetSelector>();
        serviceCollection.AddScoped<IEncoder<byte[]>, BinaryEncoder>();

        return serviceCollection;
    }

    public static IServiceCollection AddFileDevice(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        serviceCollection.Configure<FileDeviceSettings>(configuration.GetSection(FileDeviceSettings.Section));

        serviceCollection.AddScoped<IDevice, FileDevice>();

        return serviceCollection;
    }

    public static IServiceCollection AddRandomFileDevice(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        serviceCollection.Configure<RandomFileDeviceSettings>(configuration.GetSection(RandomFileDeviceSettings.Section));

        serviceCollection.AddScoped<IDevice, RandomFileDevice>();

        return serviceCollection;
    }
}