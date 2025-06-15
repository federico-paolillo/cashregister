using System.Data;
using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CashRegister.Database.Interceptors;

public sealed class SqlitePragmasDbConnectionInterceptor(
    ILogger<SqlitePragmasDbConnectionInterceptor> logger
) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        var pragmaCommand = connection.CreateCommand();

        pragmaCommand.CommandText =
            """
            PRAGMA journal_mode = WAL;
            PRAGMA busy_timeout = 1000;
            PRAGMA synchronous = NORMAL;
            PRAGMA cache_size = -20480; --20 Mib
            PRAGMA foreign_keys = true; 
            PRAGMA temp_store = memory;
            """;

        pragmaCommand.CommandType = CommandType.Text;
        
        logger.LogInformation("Issued PRAGMAs after connection open. Command is {Command}", pragmaCommand.CommandText);

        await pragmaCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}