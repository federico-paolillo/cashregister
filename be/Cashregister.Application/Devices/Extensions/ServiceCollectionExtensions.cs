using Cashregister.Application.Devices.Services;
using Cashregister.Application.Devices.Services.Defaults;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Emulator.Device;
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

            serviceCollection.AddOptions<FileDeviceSettings>()
                .BindConfiguration(FileDeviceSettings.Section);
        
        serviceCollection.AddScoped<IDevice, FileDevice>();

        return serviceCollection;
    }

    public static IServiceCollection AddMarkdownDevice(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        serviceCollection.AddOptions<MarkdownDeviceSettings>()
            .BindConfiguration(MarkdownDeviceSettings.Section);

        serviceCollection.AddScoped<IInstructionDecoder, InstructionDecoder>();
        serviceCollection.AddScoped<IInstructionExecutor, InstructionExecutor>();
        serviceCollection.AddScoped<IPrinterEmulator, PrinterEmulator>();
        serviceCollection.AddScoped<IMarkdownRenderer, MarkdownRenderer>();
        serviceCollection.AddScoped<IDevice, MarkdownDevice>();

        return serviceCollection;
    }
}