using System.Windows;
using Microsoft.Win32;
using SerialProtocolAssistant.ViewModels;
using SerialProtocolAssistant.Services;

namespace SerialProtocolAssistant;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly ISettingsService _settingsService;
    private readonly IDatabaseService _databaseService;
    private readonly IDeviceManagerService _deviceManager;
    private readonly IExportService _exportService;
    private readonly ILoggingService _loggingService;

    // 新的 ViewModels（多设备监控）
    public MonitorViewModel MonitorViewModel { get; }
    public HistoryViewModel HistoryViewModel { get; }
    public AlarmViewModel AlarmViewModel { get; }
    public DeviceConfigViewModel DeviceConfigViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        MonitorViewModel monitorViewModel,
        HistoryViewModel historyViewModel,
        AlarmViewModel alarmViewModel,
        DeviceConfigViewModel deviceConfigViewModel,
        SettingsViewModel settingsViewModel,
        ISettingsService settingsService,
        IDatabaseService databaseService,
        IDeviceManagerService deviceManager,
        IExportService exportService,
        ILoggingService loggingService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        MonitorViewModel = monitorViewModel;
        HistoryViewModel = historyViewModel;
        AlarmViewModel = alarmViewModel;
        DeviceConfigViewModel = deviceConfigViewModel;
        SettingsViewModel = settingsViewModel;
        _settingsService = settingsService;
        _databaseService = databaseService;
        _deviceManager = deviceManager;
        _exportService = exportService;
        _loggingService = loggingService;
        DataContext = viewModel;
    }

    #region 菜单事件处理

    private void LoadProtocol_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择协议配置文件",
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _deviceManager.LoadProtocol(dialog.FileName);

                // 刷新各个ViewModel
                MonitorViewModel.LoadProtocol(dialog.FileName);
                HistoryViewModel.RefreshProtocol();

                _loggingService.Information($"已加载协议: {dialog.FileName}");
                MessageBox.Show("协议加载成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"加载协议失败: {ex.Message}");
                MessageBox.Show($"加载协议失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveProtocol_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "保存协议配置文件",
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            DefaultExt = "json",
            FileName = "protocol.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _deviceManager.SaveProtocol(dialog.FileName);
                _loggingService.Information($"已保存协议: {dialog.FileName}");
                MessageBox.Show("协议保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"保存协议失败: {ex.Message}");
                MessageBox.Show($"保存协议失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExportCurrentData_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("导出功能开发中...", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // 打开设置窗口或对话框
        MessageBox.Show("系统设置功能开发中...\n\n当前可用设置：\n- 数据库优化\n- 数据备份\n- 数据清理",
            "系统设置", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _loggingService.Information("刷新界面");
    }

    private void FullScreen_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OptimizeDatabase_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _databaseService.OptimizeDatabase();
            MessageBox.Show("数据库优化成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"数据库优化失败: {ex.Message}");
            MessageBox.Show($"数据库优化失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BackupDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "选择备份位置",
            Filter = "数据库文件 (*.db)|*.db|所有文件 (*.*)|*.*",
            DefaultExt = "db",
            FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _databaseService.BackupDatabase(dialog.FileName);
                MessageBox.Show("数据库备份成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"数据库备份失败: {ex.Message}");
                MessageBox.Show($"数据库备份失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CleanupData_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "确定要清理30天前的历史数据吗？此操作不可撤销。",
            "确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var deletedCount = _databaseService.CleanupOldData(DateTime.Now.AddDays(-30));
                MessageBox.Show($"已清理 {deletedCount} 条历史数据。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"清理数据失败: {ex.Message}");
                MessageBox.Show($"清理数据失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var stats = _databaseService.GetStatistics();
        var message = $@"RS-485多设备监控系统 v2.0

数据库统计:
- 历史数据记录: {stats.TotalDataRecords:N0} 条
- 报警记录: {stats.TotalAlarmRecords:N0} 条
- 数据库大小: {stats.DatabaseSizeBytes / 1024.0 / 1024.0:F2} MB
- 最早数据: {stats.OldestDataTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无"}
- 最新数据: {stats.NewestDataTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无"}

© 2025 串口协议助手";

        MessageBox.Show(message, "关于", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            // 保存窗口大小和位置
            if (WindowState == WindowState.Normal)
            {
                _settingsService.SaveWindowBounds(Left, Top, Width, Height);
            }

            _loggingService.Information("应用程序关闭");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存设置失败: {ex.Message}");
        }
    }
}
