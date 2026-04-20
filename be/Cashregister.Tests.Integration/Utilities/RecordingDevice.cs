using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

namespace Cashregister.Tests.Integration.Utilities;

public sealed class RecordingDevice : IDevice
{
    public int PrintCount { get; private set; }

    public PrintProgram? PrintedProgram { get; private set; }

    public Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        PrintCount++;
        PrintedProgram = printProgram;

        return Task.FromResult(Result.Void());
    }
}