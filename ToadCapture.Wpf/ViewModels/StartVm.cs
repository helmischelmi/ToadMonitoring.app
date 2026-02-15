using System.Windows;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToadCapture.ImportExport.Excel;
using ToadCapture.Persistence.Repositories;
using ToadCapture.Wpf.Services;

namespace ToadCapture.Wpf.ViewModels;

public partial class StartVm : ObservableObject
{
    private readonly IToadRepository _repository;
    private readonly IExcelImportService _importService;
    private readonly ScannerInputService _scannerInputService;
    private readonly Clock _clock;
    private readonly SessionContext _sessionContext;

    public event Action? StartCompleted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string team = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string scannerMode = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string? sourceXlsxPath;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool isFirstEvening = true;

    [ObservableProperty]
    private string bannerMessage = "Bitte Team und Scanner-Modus festlegen.";

    public StartVm(
        IToadRepository repository,
        IExcelImportService importService,
        ScannerInputService scannerInputService,
        Clock clock,
        SessionContext sessionContext)
    {
        _repository = repository;
        _importService = importService;
        _scannerInputService = scannerInputService;
        _clock = clock;
        _sessionContext = sessionContext;
    }

    private bool CanStart()
    {
        if (string.IsNullOrWhiteSpace(Team) || string.IsNullOrWhiteSpace(ScannerMode))
        {
            return false;
        }

        if (!IsFirstEvening && !string.IsNullOrWhiteSpace(SourceXlsxPath) && !File.Exists(SourceXlsxPath))
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(SourceXlsxPath) && !File.Exists(SourceXlsxPath))
            {
                BannerMessage = "XLSX-Pfad nicht gefunden.";
                return;
            }

            var nowLocal = _clock.NowLocal();
            var nowUtc = _clock.NowUtc();
            var eventInfo = await _repository.CreateEventAsync(Team.Trim(), SourceXlsxPath, nowLocal, nowUtc);

            if (!string.IsNullOrWhiteSpace(SourceXlsxPath) && File.Exists(SourceXlsxPath))
            {
                var count = await _importService.ImportIndividualsAsync(SourceXlsxPath!);
                BannerMessage = $"Import erfolgreich: {count} Individuen.";
            }
            else
            {
                BannerMessage = "Erfassungsbereit.";
            }

            _sessionContext.CurrentEvent = eventInfo;
            _sessionContext.Team = Team.Trim();
            _sessionContext.ScannerEnabled = !string.Equals(ScannerMode, "No", StringComparison.OrdinalIgnoreCase);

            StartCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            BannerMessage = $"Startfehler: {ex.Message}";
        }
    }

    public void TriggerScannerTest()
    {
        _scannerInputService.ChipScanned += OnChipScanned;
        var result = Microsoft.VisualBasic.Interaction.InputBox("Scan auslösen und Wert einfügen:", "Scanner Test", "");
        if (!string.IsNullOrWhiteSpace(result))
        {
            _scannerInputService.SubmitBuffer(result);
        }

        _scannerInputService.ChipScanned -= OnChipScanned;
    }

    private static void OnChipScanned(string chip)
    {
        MessageBox.Show($"Scanner empfangen: {chip}", "Scanner Test", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}



