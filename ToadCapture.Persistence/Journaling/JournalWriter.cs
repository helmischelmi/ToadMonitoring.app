using System.Text.Json;
using ToadCapture.Core.Models;

namespace ToadCapture.Persistence.Journaling;

public sealed class JournalWriter : IJournalWriter
{
    private readonly string _baseDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JournalWriter(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
    }

    public async Task<string> WriteObservationJournalAsync(EventInfo eventInfo, Observation observation, DateTime utcNow)
    {
        var dir = Path.Combine(_baseDirectory, "Data", "Journal", eventInfo.EventDate);
        Directory.CreateDirectory(dir);

        var stamp = utcNow.ToString("yyyy-MM-ddTHHmmss.fff");
        var fileName = $"{stamp}__OBS__{observation.ObservationId}.journal";
        var targetPath = Path.Combine(dir, fileName);
        var tempPath = targetPath + ".tmp";

        var payload = new
        {
            schema = "toad.journal.v1",
            writtenAtUtc = utcNow.ToString("O"),
            @event = eventInfo,
            observation
        };

        await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(payload, JsonOptions));

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        File.Move(tempPath, targetPath);
        return targetPath;
    }
}
