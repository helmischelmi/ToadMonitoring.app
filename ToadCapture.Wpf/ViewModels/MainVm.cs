using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToadCapture.Core.Models;
using ToadCapture.Core.Rules;
using ToadCapture.ImportExport.Excel;
using ToadCapture.Persistence.Journaling;
using ToadCapture.Persistence.Repositories;
using ToadCapture.Wpf.Services;

namespace ToadCapture.Wpf.ViewModels;

public partial class MainVm : ObservableObject
{
    private readonly IToadRepository _repository;
    private readonly IJournalWriter _journalWriter;
    private readonly IExcelExportService _excelExportService;
    private readonly Clock _clock;
    private readonly SessionContext _sessionContext;

    private Individual? _currentIndividual;
    private string? _pairBufferChipId;
    private string? _pairBufferObservationId;

    public ObservableCollection<ObservationRowVm> RecentRows { get; } = new();

    [ObservableProperty]
    private ObservationRowVm? selectedRow;

    [ObservableProperty]
    private string chipInput = string.Empty;

    [ObservableProperty]
    private string weightInput = string.Empty;

    [ObservableProperty]
    private string lengthInput = string.Empty;

    [ObservableProperty]
    private string remark = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Erfassungsbereit";

    [ObservableProperty]
    private string statusColor = "#2C8B3F";

    [ObservableProperty]
    private bool isMaleSelected;

    [ObservableProperty]
    private bool isFemaleSelected;

    [ObservableProperty]
    private int mode = 1;

    [ObservableProperty]
    private int kpiNewM;

    [ObservableProperty]
    private int kpiNewW;

    [ObservableProperty]
    private int kpiExisting;

    [ObservableProperty]
    private int kpiPairs;

    [ObservableProperty]
    private int kpiTotal;

    public MainVm(
        IToadRepository repository,
        IJournalWriter journalWriter,
        IExcelExportService excelExportService,
        Clock clock,
        SessionContext sessionContext)
    {
        _repository = repository;
        _journalWriter = journalWriter;
        _excelExportService = excelExportService;
        _clock = clock;
        _sessionContext = sessionContext;

        _ = RefreshAsync();
    }

    partial void OnIsMaleSelectedChanged(bool value)
    {
        if (value)
        {
            IsFemaleSelected = false;
        }
    }

    partial void OnIsFemaleSelectedChanged(bool value)
    {
        if (value)
        {
            IsMaleSelected = false;
        }
    }

    [RelayCommand]
    private void SetMode1()
    {
        Mode = 1;
        SetStatus("Mode 1 aktiv: Vorsortierung solo Männchen", "#57D163");
    }

    [RelayCommand]
    private void SetMode2()
    {
        Mode = 2;
        SetStatus("Mode 2 aktiv: Erfassung M/W/Paar", "#57D163");
    }

    [RelayCommand]
    private async Task LookupChipAsync()
    {
        var chipId = ChipInput.Trim();
        if (string.IsNullOrWhiteSpace(chipId))
        {
            SetStatus("Kein Chip eingegeben.", "#E05555");
            return;
        }

        _currentIndividual = await _repository.GetIndividualAsync(chipId);

        if (Mode == 1)
        {
            EvaluateMode1(chipId, _currentIndividual);
            return;
        }

        if (_currentIndividual is null)
        {
            SetStatus("Unbekanntes Tier: Sex wählen und erfassen.", "#E05555");
            IsMaleSelected = false;
            IsFemaleSelected = false;
            return;
        }

        IsMaleSelected = _currentIndividual.Sex == "m";
        IsFemaleSelected = _currentIndividual.Sex == "f";

        if (_currentIndividual.Sex == "m" && _currentIndividual.MaleMeasuredOnce)
        {
            SetStatus("Männchen bereits gemessen: nur Anwesenheit/Partner/Bemerkung.", "#57D163");
        }
        else
        {
            SetStatus("Tier bereit zur Erfassung.", "#57D163");
        }
    }

    [RelayCommand]
    private void NoChip()
    {
        SetStatus("ROT: Kein Chip - zuerst chippen, danach erfassen.", "#E05555");
    }

    [RelayCommand]
    private async Task SaveObservationAsync()
    {
        var currentEvent = _sessionContext.CurrentEvent;
        if (currentEvent is null)
        {
            SetStatus("Kein aktives Event.", "#E05555");
            return;
        }

        var chipId = ChipInput.Trim();
        if (string.IsNullOrWhiteSpace(chipId))
        {
            SetStatus("Chip-ID fehlt.", "#E05555");
            return;
        }

        _currentIndividual ??= await _repository.GetIndividualAsync(chipId) ?? new Individual
        {
            ChipId = chipId,
            FirstSeenYear = _clock.NowLocal().Year,
            Sex = "u"
        };

        if (_currentIndividual.Sex == "u")
        {
            _currentIndividual.Sex = IsMaleSelected ? "m" : IsFemaleSelected ? "f" : "u";
        }

        if (_currentIndividual.Sex == "u")
        {
            SetStatus("Bei neuem Tier muss Sex gesetzt werden (M/W).", "#E05555");
            return;
        }

        var requiresMeasure = ValidationRules.RequiresMeasure(_currentIndividual);
        var weight = ParseNullableDouble(WeightInput);
        var length = ParseNullableDouble(LengthInput);

        if (requiresMeasure)
        {
            if (!ValidationRules.IsValidWeight(weight) || !ValidationRules.IsValidLength(length))
            {
                SetStatus("Gewicht/Länge ungültig oder fehlend.", "#E05555");
                return;
            }
        }

        var observation = new Observation
        {
            ObservationId = Guid.NewGuid().ToString(),
            EventId = currentEvent.EventId,
            ChipId = chipId,
            Captured = true,
            Weight = requiresMeasure ? weight : null,
            Length = requiresMeasure ? length : null,
            Remark = string.IsNullOrWhiteSpace(Remark) ? null : Remark.Trim(),
            RecordedAt = _clock.NowUtc().ToString("O"),
            Recorder = _sessionContext.Team
        };

        if (!string.IsNullOrWhiteSpace(_pairBufferChipId) && !string.Equals(_pairBufferChipId, chipId, StringComparison.OrdinalIgnoreCase))
        {
            observation.PartnerChipId = _pairBufferChipId;
        }

        if (!ValidationRules.IsPartnerValid(chipId, observation.PartnerChipId))
        {
            SetStatus("Partner darf nicht identisch mit Chip sein.", "#E05555");
            return;
        }

        if (_currentIndividual.Sex == "m" && !_currentIndividual.MaleMeasuredOnce && requiresMeasure)
        {
            _currentIndividual.MaleMeasuredOnce = true;
            _currentIndividual.MaleMeasureWeight = weight;
            _currentIndividual.MaleMeasureLength = length;
            _currentIndividual.MaleMeasureEventId = currentEvent.EventId;
        }

        try
        {
            await _journalWriter.WriteObservationJournalAsync(currentEvent, observation, _clock.NowUtc());
            await _repository.UpsertIndividualAsync(_currentIndividual);
            await _repository.AddObservationAsync(observation);

            if (!string.IsNullOrWhiteSpace(_pairBufferObservationId) && !string.IsNullOrWhiteSpace(observation.ChipId))
            {
                await _repository.UpdateObservationPartnerAsync(_pairBufferObservationId, observation.ChipId);
                _pairBufferChipId = null;
                _pairBufferObservationId = null;
            }

            await RefreshAsync();
            ClearForm();
            SetStatus("Erfassung gespeichert.", "#57D163");
        }
        catch (Exception ex)
        {
            SetStatus($"DB Fehler - Papier weiter. ({ex.Message})", "#E05555");
        }
    }

    [RelayCommand]
    private void RememberPairPart1()
    {
        var row = SelectedRow ?? RecentRows.FirstOrDefault();
        if (row is null)
        {
            SetStatus("Keine Observation verfügbar für Paar Teil 1.", "#E0C655");
            return;
        }

        _pairBufferChipId = row.ChipId;
        _pairBufferObservationId = row.ObservationId;
        SetStatus($"Paar Teil 1 gemerkt: {_pairBufferChipId}", "#E0C655");
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var currentEvent = _sessionContext.CurrentEvent;
        if (currentEvent is null)
        {
            return;
        }

        var dir = Path.Combine(AppContext.BaseDirectory, "Data", "Export", currentEvent.EventDate);
        var (xlsx, print) = await _excelExportService.ExportEventAsync(currentEvent.EventId, dir);
        SetStatus($"Export erstellt: {Path.GetFileName(xlsx)} + {Path.GetFileName(print)}", "#57D163");
    }

    private async Task RefreshAsync()
    {
        var currentEvent = _sessionContext.CurrentEvent;
        if (currentEvent is null)
        {
            return;
        }

        var recent = await _repository.GetRecentObservationsAsync(currentEvent.EventId, 30);
        RecentRows.Clear();

        foreach (var obs in recent)
        {
            var sex = "u";
            if (!string.IsNullOrWhiteSpace(obs.ChipId))
            {
                var ind = await _repository.GetIndividualAsync(obs.ChipId);
                sex = ind?.Sex ?? "u";
            }

            RecentRows.Add(new ObservationRowVm
            {
                ObservationId = obs.ObservationId,
                RecordedAt = DateTime.TryParse(obs.RecordedAt, out var dt) ? dt.ToLocalTime().ToString("HH:mm:ss") : obs.RecordedAt,
                ChipId = obs.ChipId ?? "",
                Sex = sex,
                Weight = obs.Weight?.ToString("0.0") ?? "",
                Length = obs.Length?.ToString("0.0") ?? "",
                PartnerChipId = obs.PartnerChipId ?? "",
                Remark = obs.Remark ?? ""
            });
        }

        var (newM, newW, existing, pairs, total) = await _repository.GetKpiAsync(currentEvent.EventId);
        KpiNewM = newM;
        KpiNewW = newW;
        KpiExisting = existing;
        KpiPairs = pairs;
        KpiTotal = total;
    }

    private void EvaluateMode1(string chipId, Individual? individual)
    {
        if (individual is null)
        {
            SetStatus("ROT: zu bearbeiten (neu im Jahr / unbekannt)", "#E05555");
            return;
        }

        if (individual.Sex == "m")
        {
            if (individual.MaleMeasuredOnce)
            {
                SetStatus("GRUEN: bereits erfasst", "#57D163");
                return;
            }

            SetStatus("ROT: zu bearbeiten (noch nicht gemessen)", "#E05555");
            return;
        }

        if (individual.Sex == "f")
        {
            SetStatus("GELB: Weibchen im Solo-M-Workflow (pruefen)", "#E0C655");
            return;
        }

        SetStatus($"ROT: unklare Daten fuer {chipId}", "#E05555");
    }

    private void SetStatus(string message, string color)
    {
        StatusMessage = message;
        StatusColor = color;
    }

    private void ClearForm()
    {
        ChipInput = string.Empty;
        WeightInput = string.Empty;
        LengthInput = string.Empty;
        Remark = string.Empty;
        _currentIndividual = null;
    }

    private static double? ParseNullableDouble(string input)
    {
        if (double.TryParse(input?.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var invariant))
        {
            return invariant;
        }

        if (double.TryParse(input?.Trim(), out var current))
        {
            return current;
        }

        return null;
    }
}



