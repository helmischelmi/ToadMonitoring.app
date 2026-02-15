namespace ToadCapture.Core.Models;

public sealed class Observation
{
    public string ObservationId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string? ChipId { get; set; }
    public string? TempChipPlaceholder { get; set; }
    public bool Captured { get; set; } = true;
    public double? Weight { get; set; }
    public double? Length { get; set; }
    public string? PartnerChipId { get; set; }
    public string? Remark { get; set; }
    public string RecordedAt { get; set; } = string.Empty;
    public string? Recorder { get; set; }
}
