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

        // 注册服务
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IProtocolService, ProtocolService>();
        services.AddSingleton<IFrameParserService, FrameParserService>();
        services.AddSingleton<ISerialPortService, SerialPortService>();

        // 注册 ViewModels
        services.AddSingleton<SerialSettingsViewModel>();
        services.AddSingleton<ProtocolControlViewModel>();
        services.AddSingleton<DataDisplayViewModel>();
        services.AddSingleton<LogViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        // 注册 Views
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
