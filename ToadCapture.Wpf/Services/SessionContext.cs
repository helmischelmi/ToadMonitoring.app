using ToadCapture.Core.Models;

namespace ToadCapture.Wpf.Services;

public sealed class SessionContext
{
    public EventInfo? CurrentEvent { get; set; }
    public string Team { get; set; } = string.Empty;
    public bool ScannerEnabled { get; set; }
}
