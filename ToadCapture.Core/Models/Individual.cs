namespace ToadCapture.Core.Models;

public sealed class Individual
{
    public string ChipId { get; set; } = string.Empty;
    public string Sex { get; set; } = "u";
    public string? KnownFromYears { get; set; }
    public string? IndividualNote { get; set; }
    public int FirstSeenYear { get; set; }
    public bool MaleMeasuredOnce { get; set; }
    public double? MaleMeasureWeight { get; set; }
    public double? MaleMeasureLength { get; set; }
    public string? MaleMeasureEventId { get; set; }
}
