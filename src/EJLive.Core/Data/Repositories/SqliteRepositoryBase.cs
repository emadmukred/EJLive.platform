using System;
using System.Data;
using System.Globalization;
using EJLive.Core.Services;
using Microsoft.Data.Sqlite;

namespace EJLive.Core.Data.Repositories;

/// <summary>
/// Shared plumbing for the SQLite repositories: every repository writes through the one
/// <see cref="DatabaseManager"/> instance (single provider per assembly, debt D-06 closed
/// in Wave 1) so connection pooling, WAL pragmas and busy handling stay uniform. Synchronous
/// by design — callers are background pipeline stages (SS-12); UI threads never resolve a
/// repository.
/// </summary>
public abstract class SqliteRepositoryBase
{
    protected SqliteRepositoryBase(DatabaseManager? database)
    {
        Database = database ?? DatabaseManager.Instance;
    }

    protected DatabaseManager Database { get; }

    protected int Execute(string sql, params SqliteParameter[] parameters)
        => Database.ExecuteNonQuery(sql, parameters);

    protected DataTable Query(string sql, params SqliteParameter[] parameters)
        => Database.Query(sql, parameters);

    /// <summary>First column of the first row as a long (0 for empty result / NULL).</summary>
    protected long ScalarLong(string sql, params SqliteParameter[] parameters)
    {
        var table = Query(sql, parameters);
        if (table.Rows.Count == 0)
            return 0;
        var value = table.Rows[0][0];
        return value is null or DBNull ? 0 : Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    /// <summary>First column of the first row as text ("" for empty result / NULL).</summary>
    protected string ScalarText(string sql, params SqliteParameter[] parameters)
    {
        var table = Query(sql, parameters);
        if (table.Rows.Count == 0)
            return string.Empty;
        var value = table.Rows[0][0];
        return value is null or DBNull ? string.Empty : value.ToString() ?? string.Empty;
    }

    protected static SqliteParameter P(string name, object? value)
        => new SqliteParameter(name, value ?? DBNull.Value);

    protected static string Stamp(DateTime? utc)
        => utc.HasValue ? utc.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) : string.Empty;

    protected static DateTime? ParseStamp(object? value)
        => value is null || value == DBNull.Value || !DateTime.TryParse(
               value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? null
            : parsed.ToUniversalTime();

    protected static DateTime ParseStampRequired(object? value, string fallbackContext)
        => ParseStamp(value) ?? DateTime.MinValue;
}
