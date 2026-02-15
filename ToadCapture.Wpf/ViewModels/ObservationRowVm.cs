namespace ToadCapture.Wpf.ViewModels;

public sealed class ObservationRowVm
{
    public string ObservationId { get; init; } = string.Empty;
    public string RecordedAt { get; init; } = string.Empty;
    public string ChipId { get; init; } = string.Empty;
    public string Sex { get; init; } = string.Empty;
    public string Weight { get; init; } = string.Empty;
    public string Length { get; init; } = string.Empty;
    public string PartnerChipId { get; init; } = string.Empty;
    public string Remark { get; init; } = string.Empty;
}
