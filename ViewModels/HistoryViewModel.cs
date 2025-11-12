using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 历史数据查询页面ViewModel
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IDatabaseService _databaseService;
    private readonly IDeviceManagerService _deviceManager;
    private readonly IExportService _exportService;

    /// <summary>
    /// 查询结果列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceDataRecord> _dataRecords = new();

    /// <summary>
    /// 设备列表（用于筛选）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceFilterItem> _devices = new();

    /// <summary>
    /// 通道列表（用于筛选）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChannelFilterItem> _channels = new();

    /// <summary>
    /// 选中的设备
    /// </summary>
    [ObservableProperty]
    private DeviceFilterItem? _selectedDevice;

    /// <summary>
    /// 选中的通道
    /// </summary>
    [ObservableProperty]
    private ChannelFilterItem? _selectedChannel;

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
    /// 当前页码
    /// </summary>
    [ObservableProperty]
    private int _currentPage = 1;

    /// <summary>
    /// 每页记录数
    /// </summary>
    [ObservableProperty]
    private int _pageSize = 100;

    /// <summary>
    /// 总记录数
    /// </summary>
    [ObservableProperty]
    private int _totalRecords;

    /// <summary>
    /// 总页数
    /// </summary>
    [ObservableProperty]
    private int _totalPages;

    /// <summary>
    /// 是否正在查询
    /// </summary>
    [ObservableProperty]
    private bool _isQuerying;

    /// <summary>
    /// 统计信息 - 平均值
    /// </summary>
    [ObservableProperty]
    private double _averageValue;

    /// <summary>
    /// 统计信息 - 最大值
    /// </summary>
    [ObservableProperty]
    private double _maxValue;

    /// <summary>
    /// 统计信息 - 最小值
    /// </summary>
    [ObservableProperty]
    private double _minValue;

    /// <summary>
    /// 是否显示统计信息
    /// </summary>
    [ObservableProperty]
    private bool _hasStatistics;

    public HistoryViewModel(
        ILoggingService loggingService,
        IDatabaseService databaseService,
        IDeviceManagerService deviceManager,
        IExportService exportService)
    {
        _loggingService = loggingService;
        _databaseService = databaseService;
        _deviceManager = deviceManager;
        _exportService = exportService;

        // 初始化筛选器
        InitializeFilters();
    }

    /// <summary>
    /// 初始化设备和通道筛选器
    /// </summary>
    private void InitializeFilters()
    {
        // 添加"全部"选项
        Devices.Add(new DeviceFilterItem { Id = null, Name = "全部设备" });
        Channels.Add(new ChannelFilterItem { Id = null, Name = "全部通道" });

        // 从协议配置加载设备和通道
        if (_deviceManager.CurrentProtocol != null)
        {
            foreach (var device in _deviceManager.CurrentProtocol.Devices)
            {
                Devices.Add(new DeviceFilterItem { Id = device.Id, Name = device.Name });
            }
        }

        // 默认选择"全部"
        SelectedDevice = Devices.FirstOrDefault();
        SelectedChannel = Channels.FirstOrDefault();
    }

    /// <summary>
    /// 设备选择变化，更新通道列表
    /// </summary>
    partial void OnSelectedDeviceChanged(DeviceFilterItem? value)
    {
        // 清空通道列表
        Channels.Clear();
        Channels.Add(new ChannelFilterItem { Id = null, Name = "全部通道" });

        if (value?.Id != null && _deviceManager.CurrentProtocol != null)
        {
            var device = _deviceManager.CurrentProtocol.Devices.FirstOrDefault(d => d.Id == value.Id);
            if (device != null)
            {
                foreach (var channel in device.Channels)
                {
                    Channels.Add(new ChannelFilterItem { Id = channel.Id, Name = channel.Name });
                }
            }
        }

        SelectedChannel = Channels.FirstOrDefault();
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    [RelayCommand]
    private async Task QueryData()
    {
        if (IsQuerying) return;

        try
        {
            IsQuerying = true;
            _loggingService.Information($"查询历史数据: 设备={SelectedDevice?.Name}, 通道={SelectedChannel?.Name}, 日期={StartDate:yyyy-MM-dd}~{EndDate:yyyy-MM-dd}");

            await Task.Run(() =>
            {
                // 查询数据
                var records = _databaseService.QueryData(
                    SelectedDevice?.Id,
                    SelectedChannel?.Id,
                    StartDate,
                    EndDate,
                    PageSize);

                // 更新UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataRecords.Clear();
                    foreach (var record in records)
                    {
                        DataRecords.Add(record);
                    }

                    TotalRecords = records.Count;
                    TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);
                    CurrentPage = 1;

                    // 计算统计信息
                    CalculateStatistics();

                    _loggingService.Information($"查询完成，共 {TotalRecords} 条记录");
                });
            });
        }
        catch (Exception ex)
        {
            _loggingService.Error($"查询数据失败: {ex.Message}");
            MessageBox.Show($"查询数据失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsQuerying = false;
        }
    }

    /// <summary>
    /// 计算统计信息
    /// </summary>
    private void CalculateStatistics()
    {
        if (DataRecords.Count == 0)
        {
            HasStatistics = false;
            return;
        }

        AverageValue = DataRecords.Average(r => r.Value);
        MaxValue = DataRecords.Max(r => r.Value);
        MinValue = DataRecords.Min(r => r.Value);
        HasStatistics = true;
    }

    /// <summary>
    /// 上一页
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoPreviousPage))]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            // 在实际应用中，这里应该重新查询数据
            // 目前简化实现，只支持单页显示
        }
    }

    private bool CanGoPreviousPage() => CurrentPage > 1;

    /// <summary>
    /// 下一页
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoNextPage))]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            // 在实际应用中，这里应该重新查询数据
            // 目前简化实现，只支持单页显示
        }
    }

    private bool CanGoNextPage() => CurrentPage < TotalPages;

    /// <summary>
    /// 导出Excel
    /// </summary>
    [RelayCommand]
    private async Task ExportExcel()
    {
        if (DataRecords.Count == 0)
        {
            MessageBox.Show("没有可导出的数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                FileName = $"历史数据_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    _exportService.ExportDeviceDataToExcel(DataRecords.ToList(), dialog.FileName);
                });

                MessageBox.Show($"数据已导出到:\n{dialog.FileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                _loggingService.Information($"导出Excel成功: {dialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出Excel失败: {ex.Message}");
            MessageBox.Show($"导出失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 导出CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportCsv()
    {
        if (DataRecords.Count == 0)
        {
            MessageBox.Show("没有可导出的数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV文件 (*.csv)|*.csv",
                FileName = $"历史数据_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    _exportService.ExportDeviceDataToCsv(DataRecords.ToList(), dialog.FileName);
                });

                MessageBox.Show($"数据已导出到:\n{dialog.FileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                _loggingService.Information($"导出CSV成功: {dialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出CSV失败: {ex.Message}");
            MessageBox.Show($"导出失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 刷新协议配置（在协议加载后调用）
    /// </summary>
    public void RefreshProtocol()
    {
        InitializeFilters();
    }
}

/// <summary>
/// 设备筛选项
/// </summary>
public class DeviceFilterItem
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 通道筛选项
/// </summary>
public class ChannelFilterItem
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
