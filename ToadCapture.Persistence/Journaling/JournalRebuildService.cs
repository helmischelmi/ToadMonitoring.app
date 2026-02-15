using System.Text.Json;
using ToadCapture.Core.Models;
using ToadCapture.Persistence.Db;
using ToadCapture.Persistence.Repositories;

namespace ToadCapture.Persistence.Journaling;

public sealed class JournalRebuildService
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly DbInitializer _dbInitializer;
    private readonly IToadRepository _repository;

    public JournalRebuildService(SqliteConnectionFactory connectionFactory, DbInitializer dbInitializer, IToadRepository repository)
    {
        _connectionFactory = connectionFactory;
        _dbInitializer = dbInitializer;
        _repository = repository;
    }

    public async Task RebuildDatabaseFromJournals()
    {
        var dbPath = _connectionFactory.DatabasePath;
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        await _dbInitializer.InitializeAsync();

        var journalRoot = Path.Combine(AppContext.BaseDirectory, "Data", "Journal");
        if (!Directory.Exists(journalRoot))
        {
            return;
        }

        var files = Directory.EnumerateFiles(journalRoot, "*.journal", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var knownEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var doc = JsonDocument.Parse(json);
            var eventElement = doc.RootElement.GetProperty("event");
            var observationElement = doc.RootElement.GetProperty("observation");

            var eventInfo = JsonSerializer.Deserialize<EventInfo>(eventElement.GetRawText());
            var observation = JsonSerializer.Deserialize<Observation>(observationElement.GetRawText());

            if (eventInfo is null || observation is null)
            {
                continue;
            }

            if (!knownEvents.Contains(eventInfo.EventId))
            {
                try
                {
                    await _repository.CreateEventAsync(eventInfo.Team, eventInfo.SourceXlsxPath, DateTime.Parse(eventInfo.EventDate), DateTime.Parse(eventInfo.CreatedAt));
                }
                catch
                {
                    // event may already exist
                }

                knownEvents.Add(eventInfo.EventId);
            }

            try
            {
                await _repository.AddObservationAsync(observation);
            }
            catch
            {
                // skip duplicated/bad rows during rebuild
            }
        }
    }
}
