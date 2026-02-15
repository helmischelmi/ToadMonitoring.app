using ClosedXML.Excel;
using ToadCapture.Core.Models;
using ToadCapture.Persistence.Repositories;

namespace ToadCapture.ImportExport.Excel;

public sealed class ExcelImportService : IExcelImportService
{
    private readonly IToadRepository _repository;

    public ExcelImportService(IToadRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> ImportIndividualsAsync(string xlsxPath)
    {
        if (!File.Exists(xlsxPath))
        {
            throw new FileNotFoundException("XLSX not found.", xlsxPath);
        }

        using var workbook = new XLWorkbook(xlsxPath);
        var ws = workbook.Worksheets.First();

        var headerRow = ws.FirstRowUsed();
        if (headerRow is null)
        {
            return 0;
        }

        var headers = headerRow.CellsUsed().ToDictionary(
            c => c.GetString().Trim(),
            c => c.Address.ColumnNumber,
            StringComparer.OrdinalIgnoreCase);

        if (!headers.TryGetValue("ChipId", out var chipCol))
        {
            throw new InvalidOperationException("Missing 'ChipId' column in import XLSX.");
        }

        headers.TryGetValue("KnownFromYears", out var yearsCol);
        headers.TryGetValue("Sex", out var sexCol);
        headers.TryGetValue("IndividualNote", out var noteCol);

        var imported = 0;
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var chipId = row.Cell(chipCol).GetString().Trim();
            if (string.IsNullOrWhiteSpace(chipId))
            {
                continue;
            }

            var existing = await _repository.GetIndividualAsync(chipId);
            var individual = existing ?? new Individual
            {
                ChipId = chipId,
                FirstSeenYear = DateTime.Now.Year
            };

            if (sexCol > 0)
            {
                var sex = row.Cell(sexCol).GetString().Trim().ToLowerInvariant();
                individual.Sex = sex is "m" or "f" ? sex : "u";
            }

            individual.KnownFromYears = yearsCol > 0 ? row.Cell(yearsCol).GetString().Trim() : individual.KnownFromYears;
            individual.IndividualNote = noteCol > 0 ? row.Cell(noteCol).GetString().Trim() : individual.IndividualNote;

            await _repository.UpsertIndividualAsync(individual);
            imported++;
        }

        return imported;
    }
}
