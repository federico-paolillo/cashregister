using Microsoft.Extensions.Logging;

namespace Cashregister.Database.Extensions;

public static partial class LoggerMessages
{
    [LoggerMessage(LogLevel.Debug, Message = "Issued PRAGMAs after connection open. Command used '{Command}'")]
    public static partial void ConnectionPragmasApplied(this ILogger logger, string command);
}