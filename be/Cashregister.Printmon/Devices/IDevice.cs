using Cashregister.Factories;

namespace Cashregister.Printmon.Devices;

/// <summary>
///     Takes a <see cref="PrintProgram" /> and delivers it to a device that is able to execute and render it (to paper)
/// </summary>
public interface IDevice
{
    Task<Result<Unit>> PrintAsync(PrintProgram printProgram);
}