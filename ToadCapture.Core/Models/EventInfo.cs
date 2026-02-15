namespace ToadCapture.Core.Models;

public sealed class EventInfo
{
    public string EventId { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string? SourceXlsxPath { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
