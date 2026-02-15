using Microsoft.Data.Sqlite;
using ToadCapture.Core.Models;
using ToadCapture.Persistence.Db;

namespace ToadCapture.Persistence.Repositories;

public sealed class ToadRepository : IToadRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ToadRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EventInfo> CreateEventAsync(string team, string? sourceXlsxPath, DateTime nowLocal, DateTime nowUtc)
    {
        var eventInfo = new EventInfo
        {
            EventId = Guid.NewGuid().ToString(),
            EventDate = nowLocal.ToString("yyyy-MM-dd"),
            Team = team,
            SourceXlsxPath = sourceXlsxPath,
            CreatedAt = nowUtc.ToString("O")
        };

        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Events (EventId, EventDate, Team, SourceXlsxPath, CreatedAt)
VALUES ($eventId, $eventDate, $team, $sourceXlsxPath, $createdAt);";
        cmd.Parameters.AddWithValue("$eventId", eventInfo.EventId);
        cmd.Parameters.AddWithValue("$eventDate", eventInfo.EventDate);
        cmd.Parameters.AddWithValue("$team", eventInfo.Team);
        cmd.Parameters.AddWithValue("$sourceXlsxPath", (object?)eventInfo.SourceXlsxPath ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$createdAt", eventInfo.CreatedAt);

        await cmd.ExecuteNonQueryAsync();
        return eventInfo;
    }

    public async Task<Individual?> GetIndividualAsync(string chipId)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT ChipId, Sex, KnownFromYears, IndividualNote, FirstSeenYear,
       MaleMeasuredOnce, MaleMeasureWeight, MaleMeasureLength, MaleMeasureEventId
FROM Individuals WHERE ChipId = $chipId;";
        cmd.Parameters.AddWithValue("$chipId", chipId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Individual
        {
            ChipId = reader.GetString(0),
            Sex = reader.GetString(1),
            KnownFromYears = reader.IsDBNull(2) ? null : reader.GetString(2),
            IndividualNote = reader.IsDBNull(3) ? null : reader.GetString(3),
            FirstSeenYear = reader.GetInt32(4),
            MaleMeasuredOnce = reader.GetInt64(5) == 1,
            MaleMeasureWeight = reader.IsDBNull(6) ? null : reader.GetDouble(6),
            MaleMeasureLength = reader.IsDBNull(7) ? null : reader.GetDouble(7),
            MaleMeasureEventId = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }

    public async Task UpsertIndividualAsync(Individual individual)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Individuals (ChipId, Sex, KnownFromYears, IndividualNote, FirstSeenYear,
                         MaleMeasuredOnce, MaleMeasureWeight, MaleMeasureLength, MaleMeasureEventId)
VALUES ($chipId, $sex, $knownFromYears, $individualNote, $firstSeenYear,
        $maleMeasuredOnce, $maleMeasureWeight, $maleMeasureLength, $maleMeasureEventId)
ON CONFLICT(ChipId) DO UPDATE SET
    Sex = excluded.Sex,
    KnownFromYears = excluded.KnownFromYears,
    IndividualNote = excluded.IndividualNote,
    FirstSeenYear = excluded.FirstSeenYear,
    MaleMeasuredOnce = excluded.MaleMeasuredOnce,
    MaleMeasureWeight = excluded.MaleMeasureWeight,
    MaleMeasureLength = excluded.MaleMeasureLength,
    MaleMeasureEventId = excluded.MaleMeasureEventId;";

        cmd.Parameters.AddWithValue("$chipId", individual.ChipId);
        cmd.Parameters.AddWithValue("$sex", individual.Sex);
        cmd.Parameters.AddWithValue("$knownFromYears", (object?)individual.KnownFromYears ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$individualNote", (object?)individual.IndividualNote ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$firstSeenYear", individual.FirstSeenYear);
        cmd.Parameters.AddWithValue("$maleMeasuredOnce", individual.MaleMeasuredOnce ? 1 : 0);
        cmd.Parameters.AddWithValue("$maleMeasureWeight", (object?)individual.MaleMeasureWeight ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$maleMeasureLength", (object?)individual.MaleMeasureLength ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$maleMeasureEventId", (object?)individual.MaleMeasureEventId ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AddObservationAsync(Observation observation)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Observations (ObservationId, EventId, ChipId, TempChipPlaceholder, Captured,
                          Weight, Length, PartnerChipId, Remark, RecordedAt, Recorder)
VALUES ($observationId, $eventId, $chipId, $tempChipPlaceholder, $captured,
        $weight, $length, $partnerChipId, $remark, $recordedAt, $recorder);";
        cmd.Parameters.AddWithValue("$observationId", observation.ObservationId);
        cmd.Parameters.AddWithValue("$eventId", observation.EventId);
        cmd.Parameters.AddWithValue("$chipId", (object?)observation.ChipId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$tempChipPlaceholder", (object?)observation.TempChipPlaceholder ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$captured", observation.Captured ? 1 : 0);
        cmd.Parameters.AddWithValue("$weight", (object?)observation.Weight ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$length", (object?)observation.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$partnerChipId", (object?)observation.PartnerChipId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$remark", (object?)observation.Remark ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$recordedAt", observation.RecordedAt);
        cmd.Parameters.AddWithValue("$recorder", (object?)observation.Recorder ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateObservationPartnerAsync(string observationId, string partnerChipId)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Observations SET PartnerChipId = $partnerChipId WHERE ObservationId = $observationId;";
        cmd.Parameters.AddWithValue("$partnerChipId", partnerChipId);
        cmd.Parameters.AddWithValue("$observationId", observationId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Observation>> GetRecentObservationsAsync(string eventId, int count = 30)
    {
        var result = new List<Observation>();
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT ObservationId, EventId, ChipId, TempChipPlaceholder, Captured,
       Weight, Length, PartnerChipId, Remark, RecordedAt, Recorder
FROM Observations
WHERE EventId = $eventId
ORDER BY RecordedAt DESC
LIMIT $count;";
        cmd.Parameters.AddWithValue("$eventId", eventId);
        cmd.Parameters.AddWithValue("$count", count);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Observation
            {
                ObservationId = reader.GetString(0),
                EventId = reader.GetString(1),
                ChipId = reader.IsDBNull(2) ? null : reader.GetString(2),
                TempChipPlaceholder = reader.IsDBNull(3) ? null : reader.GetString(3),
                Captured = reader.GetInt64(4) == 1,
                Weight = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                Length = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                PartnerChipId = reader.IsDBNull(7) ? null : reader.GetString(7),
                Remark = reader.IsDBNull(8) ? null : reader.GetString(8),
                RecordedAt = reader.GetString(9),
                Recorder = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }

        return result;
    }

    public async Task<(int newM, int newW, int existing, int pairs, int total)> GetKpiAsync(string eventId)
    {
        await using var connection = _connectionFactory.CreateOpenConnection();

        static async Task<int> ScalarIntAsync(SqliteConnection conn, string query, (string, object)[] parameters)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            foreach (var (name, value) in parameters)
            {
                cmd.Parameters.AddWithValue(name, value);
            }

            var scalar = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(scalar);
        }

        var p = new[] { ("$eventId", (object)eventId) };
        var newM = await ScalarIntAsync(connection, @"
SELECT COUNT(*) FROM Observations o
JOIN Individuals i ON i.ChipId = o.ChipId
WHERE o.EventId = $eventId AND i.Sex = 'm' AND i.MaleMeasureEventId = o.EventId;", p);

        var newW = await ScalarIntAsync(connection, @"
SELECT COUNT(*) FROM Observations o
JOIN Individuals i ON i.ChipId = o.ChipId
WHERE o.EventId = $eventId AND i.Sex = 'f' AND o.Weight IS NOT NULL AND o.Length IS NOT NULL;", p);

        var pairs = await ScalarIntAsync(connection, "SELECT COUNT(*) FROM Observations WHERE EventId = $eventId AND PartnerChipId IS NOT NULL;", p);
        var total = await ScalarIntAsync(connection, "SELECT COUNT(*) FROM Observations WHERE EventId = $eventId;", p);
        var existing = Math.Max(0, total - newM - newW);

        return (newM, newW, existing, pairs, total);
    }

    public async Task<List<(Observation observation, Individual? individual)>> GetEventExportRowsAsync(string eventId)
    {
        var rows = new List<(Observation, Individual?)>();
        await using var connection = _connectionFactory.CreateOpenConnection();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT o.ObservationId, o.EventId, o.ChipId, o.TempChipPlaceholder, o.Captured,
       o.Weight, o.Length, o.PartnerChipId, o.Remark, o.RecordedAt, o.Recorder,
       i.ChipId, i.Sex, i.KnownFromYears, i.IndividualNote, i.FirstSeenYear,
       i.MaleMeasuredOnce, i.MaleMeasureWeight, i.MaleMeasureLength, i.MaleMeasureEventId
FROM Observations o
LEFT JOIN Individuals i ON i.ChipId = o.ChipId
WHERE o.EventId = $eventId
ORDER BY o.RecordedAt ASC;";
        cmd.Parameters.AddWithValue("$eventId", eventId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var observation = new Observation
            {
                ObservationId = reader.GetString(0),
                EventId = reader.GetString(1),
                ChipId = reader.IsDBNull(2) ? null : reader.GetString(2),
                TempChipPlaceholder = reader.IsDBNull(3) ? null : reader.GetString(3),
                Captured = reader.GetInt64(4) == 1,
                Weight = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                Length = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                PartnerChipId = reader.IsDBNull(7) ? null : reader.GetString(7),
                Remark = reader.IsDBNull(8) ? null : reader.GetString(8),
                RecordedAt = reader.GetString(9),
                Recorder = reader.IsDBNull(10) ? null : reader.GetString(10)
            };

            Individual? individual = null;
            if (!reader.IsDBNull(11))
            {
                individual = new Individual
                {
                    ChipId = reader.GetString(11),
                    Sex = reader.GetString(12),
                    KnownFromYears = reader.IsDBNull(13) ? null : reader.GetString(13),
                    IndividualNote = reader.IsDBNull(14) ? null : reader.GetString(14),
                    FirstSeenYear = reader.GetInt32(15),
                    MaleMeasuredOnce = reader.GetInt64(16) == 1,
                    MaleMeasureWeight = reader.IsDBNull(17) ? null : reader.GetDouble(17),
                    MaleMeasureLength = reader.IsDBNull(18) ? null : reader.GetDouble(18),
                    MaleMeasureEventId = reader.IsDBNull(19) ? null : reader.GetString(19)
                };
            }

            rows.Add((observation, individual));
        }

        return rows;
    }
}
