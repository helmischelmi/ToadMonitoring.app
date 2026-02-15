namespace ToadCapture.ImportExport.Excel;

public interface IExcelImportService
{
    Task<int> ImportIndividualsAsync(string xlsxPath);
}
