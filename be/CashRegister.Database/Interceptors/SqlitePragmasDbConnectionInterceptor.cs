using System.Data;
using System.Data.Common;

using Cashregister.Database.Extensions;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cashregister.Database.Interceptors;

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
        ArgumentNullException.ThrowIfNull(connection);

        DbCommand? pragmaCommand = connection.CreateCommand();

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

        logger.ConnectionPragmasApplied(pragmaCommand.CommandText);

        await pragmaCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}