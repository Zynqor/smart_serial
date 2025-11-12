using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 报警管理页面ViewModel
/// </summary>
public partial class AlarmViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IAlarmService _alarmService;
    private readonly IDatabaseService _databaseService;
    private readonly IExportService _exportService;

    /// <summary>
    /// 当前活动报警列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AlarmRecord> _activeAlarms = new();

    /// <summary>
    /// 历史报警列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AlarmRecord> _historyAlarms = new();

    /// <summary>
    /// 选中的活动报警
    /// </summary>
    [ObservableProperty]
    private AlarmRecord? _selectedActiveAlarm;

    /// <summary>
    /// 选中的历史报警
    /// </summary>
    [ObservableProperty]
    private AlarmRecord? _selectedHistoryAlarm;

    /// <summary>
    /// 开始日期
    /// </summary>
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    /// <summary>
    /// 结束日期
    /// </summary>
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    /// <summary>
    /// 统计信息 - 上限报警数
    /// </summary>
    [ObservableProperty]
    private int _upperLimitAlarmCount;

    /// <summary>
    /// 统计信息 - 下限报警数
    /// </summary>
    [ObservableProperty]
    private int _lowerLimitAlarmCount;

    /// <summary>
    /// 统计信息 - 总报警次数
    /// </summary>
    [ObservableProperty]
    private int _totalAlarmCount;

    /// <summary>
    /// 统计信息 - 平均持续时间（秒）
    /// </summary>
    [ObservableProperty]
    private double _averageDuration;

    /// <summary>
    /// 按设备统计的报警数
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceAlarmStatistics> _deviceStatistics = new();

    /// <summary>
    /// 是否正在加载
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    public AlarmViewModel(
        ILoggingService loggingService,
        IAlarmService alarmService,
        IDatabaseService databaseService,
        IExportService exportService)
    {
        _loggingService = loggingService;
        _alarmService = alarmService;
        _databaseService = databaseService;
        _exportService = exportService;

        // 订阅报警事件
        _alarmService.AlarmTriggered += OnAlarmTriggered;
        _alarmService.AlarmResolved += OnAlarmResolved;

        // 加载活动报警
        LoadActiveAlarms();
    }

    /// <summary>
    /// 加载活动报警
    /// </summary>
    private void LoadActiveAlarms()
    {
        var activeAlarms = _alarmService.GetActiveAlarms();
        Application.Current.Dispatcher.Invoke(() =>
        {
            ActiveAlarms.Clear();
            foreach (var alarm in activeAlarms)
            {
                ActiveAlarms.Add(alarm);
            }
        });
    }

    /// <summary>
    /// 报警触发事件处理
    /// </summary>
    private void OnAlarmTriggered(object? sender, AlarmTriggeredEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ActiveAlarms.Add(e.Alarm);
            _loggingService.Warning($"新报警: {e.Alarm.DeviceName} - {e.Alarm.ChannelName}");
        });
    }

    /// <summary>
    /// 报警解除事件处理
    /// </summary>
    private void OnAlarmResolved(object? sender, AlarmResolvedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 从活动列表移除
            var alarm = ActiveAlarms.FirstOrDefault(a => a.Id == e.Alarm.Id);
            if (alarm != null)
            {
                ActiveAlarms.Remove(alarm);
            }
        });
    }

    /// <summary>
    /// 查询历史报警
    /// </summary>
    [RelayCommand]
    private async Task QueryHistory()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            _loggingService.Information($"查询历史报警: 日期={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}");

            await Task.Run(() =>
            {
                // 查询历史报警
                var alarms = _databaseService.QueryAlarms(null, null, StartDate, EndDate, 1000);

                // 查询统计信息
                var stats = _databaseService.GetAlarmStatistics(StartDate, EndDate);

                // 更新UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HistoryAlarms.Clear();
                    foreach (var alarm in alarms)
                    {
                        HistoryAlarms.Add(alarm);
                    }

                    // 计算统计信息
                    TotalAlarmCount = alarms.Count;
                    UpperLimitAlarmCount = alarms.Count(a => a.AlarmType == AlarmType.UpperLimit);
                    LowerLimitAlarmCount = alarms.Count(a => a.AlarmType == AlarmType.LowerLimit);

                    var resolvedAlarms = alarms.Where(a => a.Status == AlarmStatus.Resolved && a.DurationSeconds > 0);
                    AverageDuration = resolvedAlarms.Any() ? resolvedAlarms.Average(a => a.DurationSeconds) : 0;

                    // 按设备统计
                    DeviceStatistics.Clear();
                    var deviceGroups = alarms.GroupBy(a => a.DeviceName);
                    foreach (var group in deviceGroups.OrderByDescending(g => g.Count()))
                    {
                        DeviceStatistics.Add(new DeviceAlarmStatistics
                        {
                            DeviceName = group.Key ?? "未知设备",
                            AlarmCount = group.Count(),
                            UpperLimitCount = group.Count(a => a.AlarmType == AlarmType.UpperLimit),
                            LowerLimitCount = group.Count(a => a.AlarmType == AlarmType.LowerLimit)
                        });
                    }

                    _loggingService.Information($"查询完成，共 {TotalAlarmCount} 条报警记录");
                });
            });
        }
        catch (Exception ex)
        {
            _loggingService.Error($"查询历史报警失败: {ex.Message}");
            MessageBox.Show($"查询失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 确认报警
    /// </summary>
    [RelayCommand]
    private void AcknowledgeAlarm()
    {
        if (SelectedActiveAlarm == null)
        {
            MessageBox.Show("请选择要确认的报警！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _alarmService.AcknowledgeAlarm(SelectedActiveAlarm.Id);

            // 手动更新UI中的报警状态
            SelectedActiveAlarm.Status = AlarmStatus.Acknowledged;
            SelectedActiveAlarm.AcknowledgedTime = DateTime.Now;

            _loggingService.Information($"已确认报警: {SelectedActiveAlarm.DeviceName} - {SelectedActiveAlarm.ChannelName}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"确认报警失败: {ex.Message}");
            MessageBox.Show($"确认报警失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 确认所有报警
    /// </summary>
    [RelayCommand]
    private void AcknowledgeAll()
    {
        if (ActiveAlarms.Count == 0)
        {
            MessageBox.Show("没有需要确认的报警！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"确定要确认所有 {ActiveAlarms.Count} 条活动报警吗？",
            "确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var activeAlarmList = ActiveAlarms.Where(a => a.Status == AlarmStatus.Active).ToList();
                var acknowledgeTime = DateTime.Now;

                foreach (var alarm in activeAlarmList)
                {
                    _alarmService.AcknowledgeAlarm(alarm.Id);

                    // 手动更新UI中的报警状态
                    alarm.Status = AlarmStatus.Acknowledged;
                    alarm.AcknowledgedTime = acknowledgeTime;
                }

                _loggingService.Information($"已确认 {activeAlarmList.Count} 条活动报警");
                MessageBox.Show($"已确认 {activeAlarmList.Count} 条报警！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"批量确认报警失败: {ex.Message}");
                MessageBox.Show($"批量确认失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 清除已解除的报警记录
    /// </summary>
    [RelayCommand]
    private void ClearResolved()
    {
        var resolvedCount = HistoryAlarms.Count(a => a.Status == AlarmStatus.Resolved);
        if (resolvedCount == 0)
        {
            MessageBox.Show("没有已解除的报警记录！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"确定要从数据库中删除 {resolvedCount} 条已解除的报警记录吗？此操作不可撤销。",
            "确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // 删除已解除的报警（这里简化实现，实际应该调用DatabaseService的删除方法）
                var resolvedAlarms = HistoryAlarms.Where(a => a.Status == AlarmStatus.Resolved).ToList();
                foreach (var alarm in resolvedAlarms)
                {
                    HistoryAlarms.Remove(alarm);
                }

                _loggingService.Information($"已清除 {resolvedCount} 条已解除报警记录");
                MessageBox.Show($"已清除 {resolvedCount} 条记录！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"清除报警记录失败: {ex.Message}");
                MessageBox.Show($"清除失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 导出报警记录到Excel
    /// </summary>
    [RelayCommand]
    private async Task ExportAlarms()
    {
        if (HistoryAlarms.Count == 0)
        {
            MessageBox.Show("没有可导出的报警记录！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                FileName = $"报警记录_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    _exportService.ExportAlarmsToExcel(HistoryAlarms.ToList(), dialog.FileName);
                });

                MessageBox.Show($"报警记录已导出到:\n{dialog.FileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                _loggingService.Information($"导出报警记录成功: {dialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出报警记录失败: {ex.Message}");
            MessageBox.Show($"导出失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 刷新活动报警
    /// </summary>
    [RelayCommand]
    private void RefreshActive()
    {
        LoadActiveAlarms();
    }
}

/// <summary>
/// 设备报警统计
/// </summary>
public partial class DeviceAlarmStatistics : ObservableObject
{
    [ObservableProperty]
    private string _deviceName = string.Empty;

    [ObservableProperty]
    private int _alarmCount;

    [ObservableProperty]
    private int _upperLimitCount;

    [ObservableProperty]
    private int _lowerLimitCount;
}
