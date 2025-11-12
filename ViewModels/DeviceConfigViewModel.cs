using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO.Ports;

namespace SerialProtocolAssistant.ViewModels;

/// <summary>
/// 设备配置页面ViewModel
/// </summary>
public partial class DeviceConfigViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly ISerialPortService _serialPort;
    private readonly IDeviceManagerService _deviceManager;

    /// <summary>
    /// 可用串口列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new();

    /// <summary>
    /// 选中的串口
    /// </summary>
    [ObservableProperty]
    private string? _selectedPort;

    /// <summary>
    /// 波特率列表
    /// </summary>
    public ObservableCollection<int> BaudRates { get; } = new()
    {
        9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600
    };

    /// <summary>
    /// 选中的波特率
    /// </summary>
    [ObservableProperty]
    private int _selectedBaudRate = 9600;

    /// <summary>
    /// 数据位列表
    /// </summary>
    public ObservableCollection<int> DataBits { get; } = new() { 5, 6, 7, 8 };

    /// <summary>
    /// 选中的数据位
    /// </summary>
    [ObservableProperty]
    private int _selectedDataBits = 8;

    /// <summary>
    /// 停止位列表
    /// </summary>
    public ObservableCollection<string> StopBits { get; } = new() { "None", "One", "Two", "OnePointFive" };

    /// <summary>
    /// 选中的停止位
    /// </summary>
    [ObservableProperty]
    private string _selectedStopBits = "One";

    /// <summary>
    /// 校验位列表
    /// </summary>
    public ObservableCollection<string> Parities { get; } = new() { "None", "Odd", "Even", "Mark", "Space" };

    /// <summary>
    /// 选中的校验位
    /// </summary>
    [ObservableProperty]
    private string _selectedParity = "None";

    /// <summary>
    /// 是否已连接
    /// </summary>
    [ObservableProperty]
    private bool _isConnected;

    /// <summary>
    /// 协议信息
    /// </summary>
    [ObservableProperty]
    private string _protocolInfo = "未加载协议";

    /// <summary>
    /// 设备列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceDefinition> _devices = new();

    public DeviceConfigViewModel(
        ILoggingService loggingService,
        ISerialPortService serialPort,
        IDeviceManagerService deviceManager)
    {
        _loggingService = loggingService;
        _serialPort = serialPort;
        _deviceManager = deviceManager;

        // 刷新串口列表
        RefreshPorts();

        // 加载当前串口配置
        LoadCurrentSettings();

        // 加载设备列表
        RefreshDevices();
    }

    /// <summary>
    /// 刷新串口列表
    /// </summary>
    [RelayCommand]
    private void RefreshPorts()
    {
        try
        {
            AvailablePorts.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                AvailablePorts.Add(port);
            }

            if (AvailablePorts.Count > 0 && SelectedPort == null)
            {
                SelectedPort = AvailablePorts[0];
            }

            _loggingService.Information($"刷新串口列表，共 {AvailablePorts.Count} 个端口");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新串口列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载当前串口设置
    /// </summary>
    private void LoadCurrentSettings()
    {
        try
        {
            if (_serialPort.IsOpen)
            {
                SelectedPort = _serialPort.PortName;
                SelectedBaudRate = _serialPort.BaudRate;
                SelectedDataBits = _serialPort.DataBits;
                SelectedStopBits = _serialPort.StopBits.ToString();
                SelectedParity = _serialPort.Parity.ToString();
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载串口设置失败: {ex.Message}");
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
            _serialPort.Close();

            _serialPort.PortName = SelectedPort;
            _serialPort.BaudRate = SelectedBaudRate;
            _serialPort.DataBits = SelectedDataBits;
            _serialPort.StopBits = Enum.Parse<System.IO.Ports.StopBits>(SelectedStopBits);
            _serialPort.Parity = Enum.Parse<System.IO.Ports.Parity>(SelectedParity);

            _serialPort.Open();

            IsConnected = true;
            _loggingService.Information($"串口已打开: {SelectedPort}");
            MessageBox.Show("串口已打开！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"打开串口失败: {ex.Message}");
            MessageBox.Show($"打开串口失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 关闭串口
    /// </summary>
    [RelayCommand]
    private void ClosePort()
    {
        try
        {
            _serialPort.Close();
            IsConnected = false;
            _loggingService.Information("串口已关闭");
            MessageBox.Show("串口已关闭！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"关闭串口失败: {ex.Message}");
            MessageBox.Show($"关闭串口失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 刷新设备列表
    /// </summary>
    [RelayCommand]
    private void RefreshDevices()
    {
        try
        {
            Devices.Clear();

            if (_deviceManager.CurrentProtocol != null)
            {
                ProtocolInfo = $"协议: {_deviceManager.CurrentProtocol.ProtocolName} (版本: {_deviceManager.CurrentProtocol.Version})";

                foreach (var device in _deviceManager.CurrentProtocol.Devices)
                {
                    Devices.Add(device);
                }

                _loggingService.Information($"已加载 {Devices.Count} 个设备");
            }
            else
            {
                ProtocolInfo = "未加载协议";
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"刷新设备列表失败: {ex.Message}");
        }
    }
}
