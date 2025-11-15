using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;
using ScottPlot.WPF;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 实时监控视图模型
/// </summary>
public partial class MonitorViewModel : ObservableObject
{
    private readonly ISerialPortService _serialPortService;
    private readonly IProtocolService _protocolService;
    private readonly ILoggingService _loggingService;
    private WpfPlot? _chartPlot;
    private System.Timers.Timer? _updateTimer;
    private System.Timers.Timer? _statusCheckTimer;

    #region 串口相关属性

    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new();

    [ObservableProperty]
    private string? _selectedPort;

    [ObservableProperty]
    private ObservableCollection<int> _baudRates = new() { 9600, 19200, 38400, 57600, 115200 };

    [ObservableProperty]
    private int _selectedBaudRate = 115200;

    [ObservableProperty]
    private bool _isSerialConnected;

    #endregion

    #region 监控相关属性

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private ObservableCollection<DeviceRuntimeInfo> _devices = new();

    [ObservableProperty]
    private double _dataRate;

    [ObservableProperty]
    private bool _isProtocolLoaded;

    #endregion

    public MonitorViewModel(
        ISerialPortService serialPortService,
        IProtocolService protocolService,
        ILoggingService loggingService)
    {
        _serialPortService = serialPortService;
        _protocolService = protocolService;
        _loggingService = loggingService;

        // 初始化串口列表
        RefreshPorts();

        // 订阅数据接收事件
        _serialPortService.DataReceived += OnDataReceived;

        // 初始化定时器（用于更新图表和数据速率）
        _updateTimer = new System.Timers.Timer(100); // 100ms 更新一次
        _updateTimer.Elapsed += OnUpdateTimerElapsed;

        // 初始化状态检查定时器（用于检查串口连接状态）
        _statusCheckTimer = new System.Timers.Timer(500); // 500ms 检查一次
        _statusCheckTimer.Elapsed += OnStatusCheckTimerElapsed;
        _statusCheckTimer.Start();
    }

    /// <summary>
    /// 设置图表控件引用
    /// </summary>
    public void SetChartPlot(WpfPlot chartPlot)
    {
        _chartPlot = chartPlot;
        _loggingService.Information("图表控件已初始化");
    }

    #region 串口命令

    [RelayCommand]
    private void RefreshPorts()
    {
        try
        {
            AvailablePorts.Clear();
            var ports = _serialPortService.GetAvailablePorts();
            foreach (var port in ports)
            {
                AvailablePorts.Add(port);
            }

            if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(SelectedPort))
            {
                SelectedPort = AvailablePorts[0];
            }

            _loggingService.Information($"刷新串口列表，找到 {AvailablePorts.Count} 个端口");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新串口列表失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenPort()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                _loggingService.Warning("请先选择串口");
                return;
            }

            // 调用实际的 Open 方法，传入默认参数
            _serialPortService.Open(SelectedPort, SelectedBaudRate, 8, "None", "1");
            IsSerialConnected = _serialPortService.IsOpen;
            _loggingService.Information($"串口 {SelectedPort} 已打开，波特率: {SelectedBaudRate}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"打开串口失败: {ex.Message}");
            MessageBox.Show($"打开串口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ClosePort()
    {
        try
        {
            _serialPortService.Close();
            IsSerialConnected = _serialPortService.IsOpen;
            _loggingService.Information("串口已关闭");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"关闭串口失败: {ex.Message}");
        }
    }

    #endregion

    #region 监控命令

    [RelayCommand]
    private void StartMonitoring()
    {
        try
        {
            if (!IsSerialConnected)
            {
                _loggingService.Warning("请先连接串口");
                MessageBox.Show("请先连接串口", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsMonitoring = true;
            StatusText = "监控中";
            _updateTimer?.Start();
            _loggingService.Information("开始监控");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"开始监控失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        try
        {
            IsMonitoring = false;
            StatusText = "已停止";
            _updateTimer?.Stop();
            _loggingService.Information("停止监控");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"停止监控失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        try
        {
            // TODO: 实现设备刷新逻辑
            _loggingService.Information("刷新设备列表");

            // 示例：创建测试设备
            if (Devices.Count == 0)
            {
                var device = new DeviceRuntimeInfo
                {
                    Definition = new DeviceDefinition
                    {
                        Name = "测试设备",
                        DeviceId = 1
                    },
                    Status = "Online"
                };

                // 添加测试通道
                device.ChannelData.Add(new ChannelDataPoint
                {
                    Definition = new ChannelDefinition
                    {
                        Name = "温度",
                        Unit = "°C",
                        Color = "#F44336"
                    },
                    Value = 25.5
                });

                device.ChannelData.Add(new ChannelDataPoint
                {
                    Definition = new ChannelDefinition
                    {
                        Name = "湿度",
                        Unit = "%",
                        Color = "#2196F3"
                    },
                    Value = 60.0
                });

                Devices.Add(device);
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新设备失败: {ex.Message}");
        }
    }

    #endregion

    #region 事件处理

    private void OnStatusCheckTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var wasConnected = IsSerialConnected;
                IsSerialConnected = _serialPortService.IsOpen;

                // 检测断开连接
                if (wasConnected && !IsSerialConnected && IsMonitoring)
                {
                    _loggingService.Warning("串口连接已断开，停止监控");
                    StopMonitoring();
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"检查串口状态失败: {ex.Message}");
            }
        });
    }

    private void OnDataReceived(object? sender, byte[] data)
    {
        if (!IsMonitoring)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                // TODO: 解析数据并更新设备状态
                _loggingService.Debug($"接收到数据: {BitConverter.ToString(data)}");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"处理接收数据失败: {ex.Message}");
            }
        });
    }

    private void OnUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                // TODO: 更新图表
                // TODO: 计算数据速率
            }
            catch (Exception ex)
            {
                _loggingService.Error($"更新图表失败: {ex.Message}");
            }
        });
    }

    #endregion
}
