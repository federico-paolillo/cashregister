using Microsoft.Extensions.Logging;

namespace Cashregister.Database.Extensions;

public static partial class LoggerMessages
{
    [LoggerMessage(LogLevel.Information, Message = "Issued PRAGMAs after connection open. Command is {Command}")]
    public static partial void ConnectionPragmasApplied(this ILogger logger, string command);
}