using ToadCapture.Core.Models;

namespace ToadCapture.Persistence.Journaling;

public interface IJournalWriter
{
    Task<string> WriteObservationJournalAsync(EventInfo eventInfo, Observation observation, DateTime utcNow);
}
