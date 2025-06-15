using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration;

public sealed class XUnitLoggerProvider(
    ITestOutputHelper testOutputHelper
) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(categoryName, testOutputHelper);
    }

    public void Dispose()
    {
    }
}

public sealed class XUnitLogger(
    string categoryName,
    ITestOutputHelper testOutputHelper
) : ILogger
{
    public void Log<TState>(
        LogLevel logLevel,
        EventId _,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        testOutputHelper.WriteLine(
            "[{0}] [{1}] {2}",
            categoryName,
            logLevel,
            formatter(state, exception)
        );
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}