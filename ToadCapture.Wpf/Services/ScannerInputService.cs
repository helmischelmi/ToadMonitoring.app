namespace ToadCapture.Wpf.Services;

public sealed class ScannerInputService
{
    private readonly List<char> _buffer = new();

    public event Action<string>? ChipScanned;

    public void ProcessKey(char c)
    {
        if (c == '\r' || c == '\n')
        {
            Flush();
            return;
        }

        _buffer.Add(c);
    }

    public void SubmitBuffer(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ChipScanned?.Invoke(value.Trim());
        }
    }

    private void Flush()
    {
        if (_buffer.Count == 0)
        {
            return;
        }

        var value = new string(_buffer.ToArray());
        _buffer.Clear();

        if (!string.IsNullOrWhiteSpace(value))
        {
            ChipScanned?.Invoke(value.Trim());
        }
    }
}
