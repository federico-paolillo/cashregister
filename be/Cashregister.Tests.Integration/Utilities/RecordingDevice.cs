using System.Collections.Immutable;

using Cashregister.Factories;
using Cashregister.Printmon;
using Cashregister.Printmon.Devices;

namespace Cashregister.Tests.Integration.Utilities;

public sealed class RecordingDevice : IDevice
{
    private readonly List<PrintProgram> _printedPrograms = [];

    public int PrintCount { get; private set; }

    public PrintProgram? PrintedProgram { get; private set; }

    public ImmutableArray<PrintProgram> PrintedPrograms => [.. _printedPrograms];

    public Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        PrintCount++;
        PrintedProgram = printProgram;
        _printedPrograms.Add(printProgram);

        return Task.FromResult(Result.Void());
    }
}