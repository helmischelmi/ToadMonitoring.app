using ToadCapture.Core.Models;

namespace ToadCapture.Persistence.Repositories;

public interface IToadRepository
{
    Task<EventInfo> CreateEventAsync(string team, string? sourceXlsxPath, DateTime nowLocal, DateTime nowUtc);
    Task<Individual?> GetIndividualAsync(string chipId);
    Task UpsertIndividualAsync(Individual individual);
    Task AddObservationAsync(Observation observation);
    Task UpdateObservationPartnerAsync(string observationId, string partnerChipId);
    Task<List<Observation>> GetRecentObservationsAsync(string eventId, int count = 30);
    Task<(int newM, int newW, int existing, int pairs, int total)> GetKpiAsync(string eventId);
    Task<List<(Observation observation, Individual? individual)>> GetEventExportRowsAsync(string eventId);
}
