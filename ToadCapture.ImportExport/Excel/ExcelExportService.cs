using ClosedXML.Excel;
using ToadCapture.Persistence.Repositories;

namespace ToadCapture.ImportExport.Excel;

public sealed class ExcelExportService : IExcelExportService
{
    private readonly IToadRepository _repository;

    public ExcelExportService(IToadRepository repository)
    {
        _repository = repository;
    }

    public async Task<(string eveningXlsxPath, string printListPath)> ExportEventAsync(string eventId, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        var rows = await _repository.GetEventExportRowsAsync(eventId);

        var eveningXlsxPath = Path.Combine(targetDirectory, $"Abendliste_{eventId}.xlsx");
        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.AddWorksheet("Abendliste");
            var headers = new[]
            {
                "ObservationId", "ChipId", "Sex", "KnownFromYears", "IndividualNote", "Weight", "Length", "PartnerChipId", "Remark", "RecordedAt"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            var line = 2;
            foreach (var (observation, individual) in rows)
            {
                ws.Cell(line, 1).Value = observation.ObservationId;
                ws.Cell(line, 2).Value = observation.ChipId;
                ws.Cell(line, 3).Value = individual?.Sex;
                ws.Cell(line, 4).Value = individual?.KnownFromYears;
                ws.Cell(line, 5).Value = individual?.IndividualNote;
                ws.Cell(line, 6).Value = observation.Weight;
                ws.Cell(line, 7).Value = observation.Length;
                ws.Cell(line, 8).Value = observation.PartnerChipId;
                ws.Cell(line, 9).Value = observation.Remark;
                ws.Cell(line, 10).Value = observation.RecordedAt;
                line++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(eveningXlsxPath);
        }

        var printListPath = Path.Combine(targetDirectory, $"Druckliste_{eventId}.csv");
        await using (var writer = new StreamWriter(printListPath, false))
        {
            await writer.WriteLineAsync("ChipId,Sex,KnownFromYears,Note,letzteWerte");
            foreach (var (observation, individual) in rows)
            {
                var lastValues = $"{observation.Weight}/{observation.Length}";
                var line = string.Join(',',
                    Escape(observation.ChipId),
                    Escape(individual?.Sex),
                    Escape(individual?.KnownFromYears),
                    Escape(individual?.IndividualNote),
                    Escape(lastValues));
                await writer.WriteLineAsync(line);
            }
        }

        return (eveningXlsxPath, printListPath);
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
