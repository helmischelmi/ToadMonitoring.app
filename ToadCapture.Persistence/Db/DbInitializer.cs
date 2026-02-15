using ToadCapture.Persistence.Db;

namespace ToadCapture.Persistence.Db;

public sealed class DbInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public DbInitializer(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        await using var connection = _connectionFactory.CreateOpenConnection();

        var sql = @"
CREATE TABLE IF NOT EXISTS Individuals (
    ChipId TEXT PRIMARY KEY,
    Sex TEXT NOT NULL,
    KnownFromYears TEXT NULL,
    IndividualNote TEXT NULL,
    FirstSeenYear INTEGER NOT NULL,
    MaleMeasuredOnce INTEGER NOT NULL DEFAULT 0,
    MaleMeasureWeight REAL NULL,
    MaleMeasureLength REAL NULL,
    MaleMeasureEventId TEXT NULL
);

CREATE TABLE IF NOT EXISTS Events (
    EventId TEXT PRIMARY KEY,
    EventDate TEXT NOT NULL,
    Team TEXT NOT NULL,
    SourceXlsxPath TEXT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Observations (
    ObservationId TEXT PRIMARY KEY,
    EventId TEXT NOT NULL,
    ChipId TEXT NULL,
    TempChipPlaceholder TEXT NULL,
    Captured INTEGER NOT NULL,
    Weight REAL NULL,
    Length REAL NULL,
    PartnerChipId TEXT NULL,
    Remark TEXT NULL,
    RecordedAt TEXT NOT NULL,
    Recorder TEXT NULL,
    FOREIGN KEY(EventId) REFERENCES Events(EventId)
);

CREATE INDEX IF NOT EXISTS IX_Observations_EventId ON Observations(EventId);
CREATE INDEX IF NOT EXISTS IX_Observations_ChipId ON Observations(ChipId);
CREATE INDEX IF NOT EXISTS IX_Individuals_Sex ON Individuals(Sex);
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
