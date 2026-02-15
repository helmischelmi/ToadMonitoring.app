namespace ToadCapture.Wpf.Services;

public sealed class Clock
{
    public DateTime NowUtc() => DateTime.UtcNow;

    public DateTime NowLocal() => DateTime.Now;
}
