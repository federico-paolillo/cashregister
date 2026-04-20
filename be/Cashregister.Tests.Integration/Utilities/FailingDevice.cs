using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

namespace Cashregister.Tests.Integration.Utilities;

public sealed class FailingDevice : IDevice
{
    public Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        return Task.FromResult(Result.Error(new TestDeviceProblem()));
    }
}

public sealed record TestDeviceProblem : Problem;