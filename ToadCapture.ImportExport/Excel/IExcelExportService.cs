namespace ToadCapture.ImportExport.Excel;

public interface IExcelExportService
{
    Task<(string eveningXlsxPath, string printListPath)> ExportEventAsync(string eventId, string targetDirectory);
}
