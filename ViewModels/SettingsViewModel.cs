using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Services;
using System.Windows;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 系统设置页面ViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IDatabaseService _databaseService;
    private readonly IAudioService _audioService;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// 是否启用报警音效
    /// </summary>
    [ObservableProperty]
    private bool _alarmSoundEnabled = true;

    /// <summary>
    /// 自动清理天数
    /// </summary>
    [ObservableProperty]
    private int _autoCleanupDays = 30;

    /// <summary>
    /// 数据库大小（MB）
    /// </summary>
    [ObservableProperty]
    private double _databaseSizeMB;

    /// <summary>
    /// 历史数据记录数
    /// </summary>
    [ObservableProperty]
    private long _totalDataRecords;

    /// <summary>
    /// 报警记录数
    /// </summary>
    [ObservableProperty]
    private long _totalAlarmRecords;

    /// <summary>
    /// 最早数据时间
    /// </summary>
    [ObservableProperty]
    private string _oldestDataTime = "无";

    /// <summary>
    /// 最新数据时间
    /// </summary>
    [ObservableProperty]
    private string _newestDataTime = "无";

    /// <summary>
    /// 应用版本
    /// </summary>
    public string AppVersion => "v2.0.0";

    /// <summary>
    /// 构建日期
    /// </summary>
    public string BuildDate => "2025-01-12";

    public SettingsViewModel(
        ILoggingService loggingService,
        IDatabaseService databaseService,
        IAudioService audioService,
        ISettingsService settingsService)
    {
        _loggingService = loggingService;
        _databaseService = databaseService;
        _audioService = audioService;
        _settingsService = settingsService;

        // 加载设置
        LoadSettings();

        // 加载数据库统计
        RefreshStatistics();
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            AlarmSoundEnabled = _settingsService.GetSetting("AlarmSoundEnabled", true);
            AutoCleanupDays = _settingsService.GetSetting("AutoCleanupDays", 30);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            _settingsService.SaveSetting("AlarmSoundEnabled", AlarmSoundEnabled);
            _settingsService.SaveSetting("AutoCleanupDays", AutoCleanupDays);

            _loggingService.Information("设置已保存");
            MessageBox.Show("设置已保存！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存设置失败: {ex.Message}");
            MessageBox.Show($"保存设置失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 刷新统计信息
    /// </summary>
    [RelayCommand]
    private void RefreshStatistics()
    {
        try
        {
            var stats = _databaseService.GetStatistics();

            DatabaseSizeMB = stats.DatabaseSizeBytes / 1024.0 / 1024.0;
            TotalDataRecords = stats.TotalDataRecords;
            TotalAlarmRecords = stats.TotalAlarmRecords;
            OldestDataTime = stats.OldestDataTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无";
            NewestDataTime = stats.NewestDataTimestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "无";

            _loggingService.Information("统计信息已刷新");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新统计信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试报警音效
    /// </summary>
    [RelayCommand]
    private void TestAlarmSound()
    {
        try
        {
            if (AlarmSoundEnabled)
            {
                _audioService.PlayAlarmSound();
                _loggingService.Information("播放测试音效");
            }
            else
            {
                MessageBox.Show("报警音效已禁用！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"播放音效失败: {ex.Message}");
            MessageBox.Show($"播放音效失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 优化数据库
    /// </summary>
    [RelayCommand]
    private void OptimizeDatabase()
    {
        try
        {
            _databaseService.OptimizeDatabase();
            RefreshStatistics();
            MessageBox.Show("数据库优化成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            _loggingService.Information("数据库优化成功");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"数据库优化失败: {ex.Message}");
            MessageBox.Show($"数据库优化失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 清理旧数据
    /// </summary>
    [RelayCommand]
    private void CleanupOldData()
    {
        var result = MessageBox.Show(
            $"确定要清理 {AutoCleanupDays} 天前的历史数据吗？此操作不可撤销。",
            "确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-AutoCleanupDays);
                var deletedCount = _databaseService.CleanupOldData(cutoffDate);

                RefreshStatistics();
                MessageBox.Show($"已清理 {deletedCount} 条历史数据。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                _loggingService.Information($"已清理 {deletedCount} 条历史数据");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"清理数据失败: {ex.Message}");
                MessageBox.Show($"清理数据失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
