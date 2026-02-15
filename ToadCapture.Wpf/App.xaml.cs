using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ToadCapture.ImportExport.Excel;
using ToadCapture.Persistence.Db;
using ToadCapture.Persistence.Journaling;
using ToadCapture.Persistence.Repositories;
using ToadCapture.Wpf.Services;
using ToadCapture.Wpf.ViewModels;
using ToadCapture.Wpf.Views;

namespace ToadCapture.Wpf;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var dbInitializer = _serviceProvider.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();

        var startWindow = _serviceProvider.GetRequiredService<StartWindow>();
        var startVm = _serviceProvider.GetRequiredService<StartVm>();
        startVm.StartCompleted += OnStartCompleted;
        startWindow.DataContext = startVm;
        startWindow.Show();
    }

    private void OnStartCompleted()
    {
        if (_serviceProvider is null)
        {
            return;
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainVm>();
        MainWindow = mainWindow;
        mainWindow.Show();

        foreach (Window window in Current.Windows)
        {
            if (window is StartWindow)
            {
                window.Close();
                break;
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SqliteConnectionFactory>();
        services.AddSingleton<DbInitializer>();
        services.AddSingleton<IToadRepository, ToadRepository>();
        services.AddSingleton<IJournalWriter, JournalWriter>();
        services.AddSingleton<IExcelImportService, ExcelImportService>();
        services.AddSingleton<IExcelExportService, ExcelExportService>();
        services.AddSingleton<ScannerInputService>();
        services.AddSingleton<Clock>();
        services.AddSingleton<SessionContext>();

        services.AddTransient<StartVm>();
        services.AddSingleton<MainVm>();
        services.AddTransient<StartWindow>();
        services.AddTransient<MainWindow>();
    }
}
