using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;
using ScottPlot;
using ScottPlot.Plottables;
using System.Collections.Concurrent;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 实时监控页面ViewModel
/// </summary>
public partial class MonitorViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IDeviceManagerService _deviceManager;
    private readonly ISerialPortService _serialPort;
    private readonly IPollingService _pollingService;
    private readonly IFrameParserService _frameParser;
    private readonly IAlarmService _alarmService;
    private readonly IDatabaseService _databaseService;

    /// <summary>
    /// 设备运行时信息列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceRuntimeInfo> _devices = new();

    /// <summary>
    /// 是否正在监控
    /// </summary>
    [ObservableProperty]
    private bool _isMonitoring;

    /// <summary>
    /// 串口是否已连接
    /// </summary>
    [ObservableProperty]
    private bool _isSerialConnected;

    /// <summary>
    /// 协议是否已加载
    /// </summary>
    [ObservableProperty]
    private bool _isProtocolLoaded;

    /// <summary>
    /// 状态文本
    /// </summary>
    [ObservableProperty]
    private string _statusText = "未连接";

    /// <summary>
    /// 数据接收速率（条/秒）
    /// </summary>
    [ObservableProperty]
    private double _dataRate;

    /// <summary>
    /// 可用串口列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new();

    /// <summary>
    /// 选中的串口
    /// </summary>
    [ObservableProperty]
    private string _selectedPort = string.Empty;

    /// <summary>
    /// 波特率列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<int> _baudRates = new() { 9600, 19200, 38400, 57600, 115200 };

    /// <summary>
    /// 选中的波特率
    /// </summary>
    [ObservableProperty]
    private int _selectedBaudRate = 9600;

    private DateTime _lastDataTime = DateTime.Now;
    private int _dataCount;

    // ScottPlot相关
    private ScottPlot.WPF.WpfPlot? _chartPlot;
    private readonly ConcurrentDictionary<string, Scatter> _scatterPlots = new();
    private readonly ConcurrentDictionary<string, List<Coordinates>> _chartData = new();
    private readonly DateTime _startTime = DateTime.Now;
    private const int MaxDataPoints = 1000; // 最多显示1000个数据点

    public MonitorViewModel(
        ILoggingService loggingService,
        IDeviceManagerService deviceManager,
        ISerialPortService serialPort,
        IPollingService pollingService,
        IFrameParserService frameParser,
        IAlarmService alarmService,
        IDatabaseService databaseService)
    {
        _loggingService = loggingService;
        _deviceManager = deviceManager;
        _serialPort = serialPort;
        _pollingService = pollingService;
        _frameParser = frameParser;
        _alarmService = alarmService;
        _databaseService = databaseService;

        // 订阅串口事件
        _serialPort.DataReceived += OnSerialDataReceived;

        // 订阅轮询事件
        _pollingService.SendRequest += OnPollingSendRequest;
        _pollingService.StateChanged += OnPollingStateChanged;

        // 订阅报警事件
        _alarmService.AlarmTriggered += OnAlarmTriggered;
        _alarmService.AlarmResolved += OnAlarmResolved;

        // 初始化
        UpdateProtocolStatus();
        RefreshPorts();
    }

    /// <summary>
    /// 刷新串口列表
    /// </summary>
    [RelayCommand]
    private void RefreshPorts()
    {
        try
        {
            var ports = System.IO.Ports.SerialPort.GetPortNames();
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts.Clear();
                foreach (var port in ports.OrderBy(p => p))
                {
                    AvailablePorts.Add(port);
                }

                // 选中第一个串口
                if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(SelectedPort))
                {
                    SelectedPort = AvailablePorts[0];
                }
            });
            _loggingService.Information($"刷新串口列表，找到 {ports.Length} 个串口");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新串口列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 打开串口
    /// </summary>
    [RelayCommand]
    private void OpenPort()
    {
        if (string.IsNullOrEmpty(SelectedPort))
        {
            MessageBox.Show("请选择串口！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _serialPort.Open(SelectedPort, SelectedBaudRate, 8, "None", "1");
            IsSerialConnected = true;
            StatusText = $"已连接 {SelectedPort}";
            _loggingService.Information($"串口打开成功: {SelectedPort} @ {SelectedBaudRate}");
            MessageBox.Show($"串口连接成功！\n串口: {SelectedPort}\n波特率: {SelectedBaudRate}", "成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"打开串口失败: {ex.Message}");
            MessageBox.Show($"串口连接失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    [RelayCommand]
    private async Task ClosePort()
    {
        try
        {
            // 先停止监控（异步执行避免UI卡顿）
            if (IsMonitoring)
            {
                await Task.Run(() => _pollingService.Stop());
                IsMonitoring = false;
            }

            // 异步关闭串口
            await Task.Run(() => _serialPort.Close());
            IsSerialConnected = false;
            StatusText = "未连接";
            _loggingService.Information("串口已关闭");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"关闭串口失败: {ex.Message}");
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"关闭串口失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    /// <summary>
    /// 加载协议并初始化设备
    /// </summary>
    public void LoadProtocol(string filePath)
    {
        try
        {
            _deviceManager.LoadProtocol(filePath);
            InitializeDevices();
            UpdateProtocolStatus();
            _loggingService.Information($"协议加载成功: {_deviceManager.CurrentProtocol?.ProtocolName}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载协议失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 初始化设备列表
    /// </summary>
    private void InitializeDevices()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Devices.Clear();

            var enabledDevices = _deviceManager.GetEnabledDevices();
            foreach (var deviceDef in enabledDevices)
            {
                var runtimeInfo = new DeviceRuntimeInfo
                {
                    Definition = deviceDef,
                    Status = DeviceStatus.Offline,
                    ChannelData = new ObservableCollection<ChannelRuntimeData>()
                };

                // 初始化通道数据
                foreach (var channel in deviceDef.Channels)
                {
                    var channelRuntime = new ChannelRuntimeData
                    {
                        Definition = channel,
                        IsVisible = channel.DefaultVisible,
                        FormattedValue = "--"
                    };

                    // 订阅可见性变化
                    channelRuntime.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ChannelRuntimeData.IsVisible))
                        {
                            OnChannelVisibilityChanged();
                        }
                    };

                    runtimeInfo.ChannelData.Add(channelRuntime);
                }

                Devices.Add(runtimeInfo);
            }

            _loggingService.Information($"已初始化 {Devices.Count} 个设备");
        });
    }

    /// <summary>
    /// 通道可见性变化处理
    /// </summary>
    private void OnChannelVisibilityChanged()
    {
        // 重新初始化图表以反映可见性变化
        InitializeChart();
    }

    /// <summary>
    /// 开始监控
    /// </summary>
    [RelayCommand]
    private void StartMonitoring()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                MessageBox.Show("请先连接串口！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_deviceManager.CurrentProtocol == null || _deviceManager.GetEnabledDevices().Count == 0)
            {
                MessageBox.Show("请先加载协议配置！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _pollingService.Start();
            IsMonitoring = true;
            StatusText = "监控中...";
            _loggingService.Information("开始监控");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"启动监控失败: {ex.Message}");
            MessageBox.Show($"启动监控失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 停止监控
    /// </summary>
    [RelayCommand]
    private async Task StopMonitoring()
    {
        try
        {
            // 异步停止轮询服务避免UI卡顿
            await Task.Run(() => _pollingService.Stop());
            IsMonitoring = false;
            StatusText = "已停止";
            _loggingService.Information("停止监控");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"停止监控失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 刷新设备列表
    /// </summary>
    [RelayCommand]
    private void RefreshDevices()
    {
        if (_deviceManager.CurrentProtocol != null)
        {
            InitializeDevices();
        }
    }

    /// <summary>
    /// 设置图表控件
    /// </summary>
    public void SetChartPlot(ScottPlot.WPF.WpfPlot chartPlot)
    {
        _chartPlot = chartPlot;
        InitializeChart();
    }

    /// <summary>
    /// 初始化图表
    /// </summary>
    private void InitializeChart()
    {
        if (_chartPlot == null) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            _chartPlot.Plot.Clear();
            _scatterPlots.Clear();
            _chartData.Clear();

            // 为每个可见的通道创建散点图
            foreach (var device in Devices)
            {
                foreach (var channel in device.ChannelData.Where(c => c.IsVisible))
                {
                    var key = $"{device.Definition.Id}_{channel.Definition.Id}";
                    var dataList = new List<Coordinates>();
                    _chartData[key] = dataList;

                    // 解析颜色
                    var color = ParseColor(channel.Definition.Color);

                    // 创建散点图
                    var scatter = _chartPlot.Plot.Add.Scatter(dataList);
                    scatter.Color = color;
                    scatter.LineWidth = 2;
                    scatter.MarkerSize = 0; // 不显示标记点
                    scatter.LegendText = $"{device.Definition.Name} - {channel.Definition.Name}";

                    _scatterPlots[key] = scatter;
                }
            }

            _chartPlot.Refresh();
        });
    }

    /// <summary>
    /// 解析颜色字符串为ScottPlot.Color
    /// </summary>
    private ScottPlot.Color ParseColor(string colorString)
    {
        try
        {
            // 移除#号
            colorString = colorString.TrimStart('#');

            // 解析RGB
            if (colorString.Length == 6)
            {
                byte r = Convert.ToByte(colorString.Substring(0, 2), 16);
                byte g = Convert.ToByte(colorString.Substring(2, 2), 16);
                byte b = Convert.ToByte(colorString.Substring(4, 2), 16);
                return new ScottPlot.Color(r, g, b);
            }
        }
        catch
        {
            // 解析失败返回默认颜色
        }

        return ScottPlot.Colors.Blue;
    }

    /// <summary>
    /// 更新图表数据
    /// </summary>
    private void UpdateChart(DeviceDefinition deviceDef, Dictionary<string, (double value, string rawBytes)> channelValues)
    {
        if (_chartPlot == null) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var currentTime = (DateTime.Now - _startTime).TotalSeconds;
                bool needsRefresh = false;

                foreach (var kvp in channelValues)
                {
                    var key = $"{deviceDef.Id}_{kvp.Key}";

                    // 检查该通道是否可见
                    var deviceRuntime = Devices.FirstOrDefault(d => d.Definition.Id == deviceDef.Id);
                    var channelRuntime = deviceRuntime?.ChannelData.FirstOrDefault(c => c.Definition.Id == kvp.Key);
                    if (channelRuntime == null || !channelRuntime.IsVisible)
                        continue;

                    // 获取或创建数据列表
                    if (!_chartData.TryGetValue(key, out var dataList))
                    {
                        dataList = new List<Coordinates>();
                        _chartData[key] = dataList;

                        // 创建新的散点图
                        var color = ParseColor(channelRuntime.Definition.Color);
                        var scatter = _chartPlot.Plot.Add.Scatter(dataList);
                        scatter.Color = color;
                        scatter.LineWidth = 2;
                        scatter.MarkerSize = 0;
                        scatter.LegendText = $"{deviceDef.Name} - {channelRuntime.Definition.Name}";
                        _scatterPlots[key] = scatter;
                    }

                    // 添加数据点
                    dataList.Add(new Coordinates(currentTime, kvp.Value.value));

                    // 限制数据点数量
                    if (dataList.Count > MaxDataPoints)
                    {
                        dataList.RemoveAt(0);
                    }

                    // 标记需要刷新（散点图已经引用了dataList，修改dataList会自动更新scatter）
                    if (_scatterPlots.ContainsKey(key))
                    {
                        needsRefresh = true;
                    }
                }

                // 自动调整坐标轴并刷新
                if (needsRefresh)
                {
                    _chartPlot.Plot.Axes.AutoScale();
                    _chartPlot.Refresh();
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"更新图表失败: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 轮询服务发送请求事件处理
    /// </summary>
    private void OnPollingSendRequest(object? sender, SendRequestEventArgs e)
    {
        try
        {
            // 发送到串口
            _serialPort.SendData(e.RequestData);
            _loggingService.Debug($"发送轮询请求: {e.Device.Name}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"发送轮询请求失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 轮询状态变化事件处理
    /// </summary>
    private void OnPollingStateChanged(object? sender, PollingStateChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (e.CurrentDevice != null)
            {
                StatusText = $"正在轮询: {e.CurrentDevice.Name}";
            }
        });
    }

    /// <summary>
    /// 串口数据接收事件处理
    /// </summary>
    private void OnSerialDataReceived(object? sender, byte[] data)
    {
        try
        {
            // 提取设备地址
            var address = _deviceManager.ExtractAddress(data);
            var device = _deviceManager.GetDeviceByAddress(address);

            if (device == null)
            {
                _loggingService.Warning($"未找到地址为 0x{address:X2} 的设备");
                return;
            }

            // 验证CRC
            if (!_deviceManager.ValidateReceivedData(device, data))
            {
                _loggingService.Warning($"设备 {device.Name} 数据CRC验证失败");
                return;
            }

            // 解析数据
            var channelValues = _frameParser.ParseDeviceFrame(data, device);

            // 更新设备运行时信息
            UpdateDeviceData(device, channelValues);

            // 更新图表
            UpdateChart(device, channelValues);

            // 检查报警
            foreach (var kvp in channelValues)
            {
                var channel = device.Channels.FirstOrDefault(c => c.Id == kvp.Key);
                if (channel != null && channel.AlarmEnabled)
                {
                    _alarmService.CheckAlarm(device, channel, kvp.Value.value);
                }
            }

            // 保存到数据库
            SaveDataToDatabase(device, channelValues);

            // 更新数据速率
            UpdateDataRate();
        }
        catch (Exception ex)
        {
            _loggingService.Error($"处理接收数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新设备数据
    /// </summary>
    private void UpdateDeviceData(DeviceDefinition deviceDef, Dictionary<string, (double value, string rawBytes)> channelValues)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var deviceRuntime = Devices.FirstOrDefault(d => d.Definition.Id == deviceDef.Id);
            if (deviceRuntime == null) return;

            // 更新状态
            deviceRuntime.Status = DeviceStatus.Online;
            deviceRuntime.LastUpdateTime = DateTime.Now;
            deviceRuntime.ConsecutiveTimeouts = 0;
            deviceRuntime.SuccessfulResponses++;

            // 更新通道数据
            foreach (var kvp in channelValues)
            {
                var channelRuntime = deviceRuntime.ChannelData.FirstOrDefault(c => c.Definition.Id == kvp.Key);
                if (channelRuntime != null)
                {
                    channelRuntime.Value = kvp.Value.value;
                    channelRuntime.RawBytes = kvp.Value.rawBytes;
                    channelRuntime.FormattedValue = kvp.Value.value.ToString($"F{channelRuntime.Definition.DecimalPlaces}");
                    channelRuntime.LastUpdateTime = DateTime.Now;
                }
            }
        });
    }

    /// <summary>
    /// 保存数据到数据库
    /// </summary>
    private void SaveDataToDatabase(DeviceDefinition device, Dictionary<string, (double value, string rawBytes)> channelValues)
    {
        try
        {
            foreach (var kvp in channelValues)
            {
                var channel = device.Channels.FirstOrDefault(c => c.Id == kvp.Key);
                if (channel != null)
                {
                    _databaseService.SaveDeviceData(
                        device.Id,
                        device.Name,
                        channel.Id,
                        channel.Name,
                        kvp.Value.value,
                        channel.Unit,
                        DateTime.Now);
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存数据到数据库失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 报警触发事件处理
    /// </summary>
    private void OnAlarmTriggered(object? sender, AlarmTriggeredEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var device = Devices.FirstOrDefault(d => d.Definition.Id == e.Alarm.DeviceId);
            if (device != null)
            {
                var channel = device.ChannelData.FirstOrDefault(c => c.Definition.Id == e.Alarm.ChannelId);
                if (channel != null)
                {
                    channel.IsAlarming = true;
                    channel.AlarmType = e.Alarm.AlarmType;
                }
            }
        });
    }

    /// <summary>
    /// 报警恢复事件处理
    /// </summary>
    private void OnAlarmResolved(object? sender, AlarmResolvedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var device = Devices.FirstOrDefault(d => d.Definition.Id == e.Alarm.DeviceId);
            if (device != null)
            {
                var channel = device.ChannelData.FirstOrDefault(c => c.Definition.Id == e.Alarm.ChannelId);
                if (channel != null)
                {
                    channel.IsAlarming = false;
                    channel.AlarmType = null;
                }
            }
        });
    }

    /// <summary>
    /// 更新协议状态
    /// </summary>
    private void UpdateProtocolStatus()
    {
        IsProtocolLoaded = _deviceManager.CurrentProtocol != null;
        IsSerialConnected = _serialPort.IsOpen;
    }

    /// <summary>
    /// 更新数据速率
    /// </summary>
    private void UpdateDataRate()
    {
        _dataCount++;
        var elapsed = (DateTime.Now - _lastDataTime).TotalSeconds;
        if (elapsed >= 1.0)
        {
            DataRate = _dataCount / elapsed;
            _dataCount = 0;
            _lastDataTime = DateTime.Now;
        }
    }
}
