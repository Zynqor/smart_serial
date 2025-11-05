using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;

namespace SerialProtocolAssistant.ViewModels;

public partial class SerialSettingsViewModel : ObservableObject
{
    private readonly ISerialPortService _serialPortService;
    private readonly ILoggingService _loggingService;

    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new();

    [ObservableProperty]
    private string? _selectedPort;

    [ObservableProperty]
    private ObservableCollection<int> _baudRates = new() { 9600, 19200, 38400, 57600, 115200 };

    [ObservableProperty]
    private int _selectedBaudRate = 115200;

    [ObservableProperty]
    private ObservableCollection<int> _dataBitsOptions = new() { 7, 8 };

    [ObservableProperty]
    private int _selectedDataBits = 8;

    [ObservableProperty]
    private ObservableCollection<string> _parityOptions = new() { "None", "Odd", "Even", "Mark", "Space" };

    [ObservableProperty]
    private string _selectedParity = "None";

    [ObservableProperty]
    private ObservableCollection<string> _stopBitsOptions = new() { "1", "1.5", "2" };

    [ObservableProperty]
    private string _selectedStopBits = "1";

    [ObservableProperty]
    private bool _isConnected;

    public SerialSettingsViewModel(ISerialPortService serialPortService, ILoggingService loggingService)
    {
        _serialPortService = serialPortService;
        _loggingService = loggingService;
        RefreshPorts();
    }

    [RelayCommand]
    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        var ports = _serialPortService.GetAvailablePorts();
        foreach (var port in ports)
        {
            AvailablePorts.Add(port);
        }

        if (AvailablePorts.Count > 0 && SelectedPort == null)
        {
            SelectedPort = AvailablePorts[0];
        }

        _loggingService.Information($"刷新串口列表，找到 {ports.Length} 个端口");
    }

    [RelayCommand]
    private void ToggleConnection()
    {
        if (IsConnected)
        {
            _serialPortService.Close();
            IsConnected = false;
        }
        else
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                _loggingService.Warning("请选择串口");
                return;
            }

            try
            {
                _serialPortService.Open(SelectedPort, SelectedBaudRate, SelectedDataBits, SelectedParity, SelectedStopBits);
                IsConnected = true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"连接失败: {ex.Message}");
                IsConnected = false;
            }
        }
    }
}
