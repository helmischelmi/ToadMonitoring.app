using Microsoft.Data.Sqlite;

namespace ToadCapture.Persistence.Db;

public sealed class SqliteConnectionFactory
{
    private readonly string _dbPath;

    public SqliteConnectionFactory(string? baseDirectory = null)
    {
        baseDirectory ??= AppContext.BaseDirectory;
        var dataDir = Path.Combine(baseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        _dbPath = Path.Combine(dataDir, "toad.db");
    }

    public string DatabasePath => _dbPath;

    public SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }
}
