using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SerialProtocolAssistant.Services;
using SerialProtocolAssistant.ViewModels;
using SerialProtocolAssistant.Views;

namespace SerialProtocolAssistant;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected void OnStartup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // 注册核心服务
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<ISerialPortService, SerialPortService>();
        services.AddSingleton<IFrameParserService, FrameParserService>();

        // 注册新的服务层
        services.AddSingleton<ICrcService, CrcService>();
        services.AddSingleton<IDatabaseService>(provider =>
        {
            var loggingService = provider.GetRequiredService<ILoggingService>();
            var dbService = new DatabaseService(loggingService);
            dbService.Initialize(); // 初始化数据库
            return dbService;
        });
        services.AddSingleton<IDeviceManagerService, DeviceManagerService>();
        services.AddSingleton<IPollingService, PollingService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<IAlarmService, AlarmService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<ISettingsService>(provider =>
        {
            var loggingService = provider.GetRequiredService<ILoggingService>();
            return new SettingsService(loggingService);
        });

        // 保留旧的服务（向下兼容）
        services.AddSingleton<IProtocolService, ProtocolService>();

        // 注册 ViewModels（旧的）
        services.AddSingleton<SerialSettingsViewModel>();
        services.AddSingleton<ProtocolControlViewModel>();
        services.AddSingleton<DataDisplayViewModel>();
        services.AddSingleton<LogViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // 初始化主窗口
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        // 恢复窗口大小和位置
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        var bounds = settingsService.LoadWindowBounds();
        if (bounds.HasValue)
        {
            mainWindow.Left = bounds.Value.left;
            mainWindow.Top = bounds.Value.top;
            mainWindow.Width = bounds.Value.width;
            mainWindow.Height = bounds.Value.height;
        }

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
