using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SerialProtocolAssistant.ViewModels;

public partial class ProtocolControlViewModel : ObservableObject
{
    private readonly IProtocolService _protocolService;
    private readonly ISerialPortService _serialPortService;
    private readonly ILoggingService _loggingService;
    private DataDisplayViewModel? _dataDisplayViewModel;
    private DispatcherTimer? _autoSendTimer;

    [ObservableProperty]
    private string? _protocolFilePath;

    [ObservableProperty]
    private ObservableCollection<string> _commandNames = new();

    [ObservableProperty]
    private string? _selectedCommand;

    [ObservableProperty]
    private string _sendDataText = string.Empty;

    [ObservableProperty]
    private int _autoSendInterval = 1000;

    [ObservableProperty]
    private bool _isAutoSending;

    // CRC 校验相关属性
    [ObservableProperty]
    private bool _crcEnabled = false;

    [ObservableProperty]
    private CrcTypeOption? _selectedCrcTypeOption;

    [ObservableProperty]
    private int _crcOffset = 0;

    /// <summary>
    /// CRC 类型选项列表
    /// </summary>
    public ObservableCollection<CrcTypeOption> CrcTypeOptions { get; } = new()
    {
        new CrcTypeOption { Type = CrcType.CRC16_Modbus, DisplayName = "CRC16-Modbus (2字节)" },
        new CrcTypeOption { Type = CrcType.CRC16_IBM, DisplayName = "CRC16-IBM (2字节)" },
        new CrcTypeOption { Type = CrcType.CRC32, DisplayName = "CRC32 (4字节)" },
        new CrcTypeOption { Type = CrcType.Checksum, DisplayName = "Checksum (1字节)" }
    };

    public ProtocolControlViewModel(
        IProtocolService protocolService,
        ISerialPortService serialPortService,
        ILoggingService loggingService)
    {
        _protocolService = protocolService;
        _serialPortService = serialPortService;
        _loggingService = loggingService;

        // 默认选择 CRC16-Modbus
        SelectedCrcTypeOption = CrcTypeOptions[0];
    }

    public void SetDataDisplayViewModel(DataDisplayViewModel dataDisplayViewModel)
    {
        _dataDisplayViewModel = dataDisplayViewModel;
    }

    /// <summary>
    /// 获取当前选中的 CRC 类型
    /// </summary>
    public CrcType GetSelectedCrcType()
    {
        return CrcEnabled ? (SelectedCrcTypeOption?.Type ?? CrcType.None) : CrcType.None;
    }

    [RelayCommand]
    private void LoadProtocol()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "选择协议文件"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadProtocolFromFile(dialog.FileName);
        }
    }

    public void LoadProtocolFromFile(string filePath)
    {
        try
        {
            ProtocolFilePath = filePath;
            _protocolService.LoadProtocol(ProtocolFilePath);

            CommandNames.Clear();
            var commands = _protocolService.GetCommandNames();
            foreach (var cmd in commands)
            {
                CommandNames.Add(cmd);
            }

            if (CommandNames.Count > 0)
            {
                SelectedCommand = CommandNames[0];
            }

            _loggingService.Information($"已加载协议文件: {System.IO.Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载协议文件失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SendData()
    {
        if (string.IsNullOrWhiteSpace(SendDataText))
        {
            _loggingService.Warning("请输入要发送的数据");
            return;
        }

        try
        {
            // 解析十六进制字符串
            var hexString = SendDataText.Replace(" ", "").Replace("-", "");
            if (hexString.Length % 2 != 0)
            {
                _loggingService.Error("十六进制数据长度必须是偶数");
                return;
            }

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            _serialPortService.SendData(bytes);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"发送数据失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleAutoSend()
    {
        if (IsAutoSending)
        {
            StopAutoSend();
        }
        else
        {
            StartAutoSend();
        }
    }

    private void StartAutoSend()
    {
        if (string.IsNullOrWhiteSpace(SendDataText))
        {
            _loggingService.Warning("请输入要发送的数据");
            return;
        }

        if (AutoSendInterval < 100)
        {
            _loggingService.Warning("自动发送间隔不能小于100毫秒");
            return;
        }

        _autoSendTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(AutoSendInterval)
        };
        _autoSendTimer.Tick += (s, e) => SendData();
        _autoSendTimer.Start();

        IsAutoSending = true;
        _loggingService.Information($"开始自动发送，间隔 {AutoSendInterval} 毫秒");
    }

    private void StopAutoSend()
    {
        if (_autoSendTimer != null)
        {
            _autoSendTimer.Stop();
            _autoSendTimer = null;
        }

        IsAutoSending = false;
        _loggingService.Information("停止自动发送");
    }

    partial void OnAutoSendIntervalChanged(int value)
    {
        // 如果正在自动发送且间隔改变，重启定时器
        if (IsAutoSending && _autoSendTimer != null)
        {
            _autoSendTimer.Stop();
            if (value >= 100)
            {
                _autoSendTimer.Interval = TimeSpan.FromMilliseconds(value);
                _autoSendTimer.Start();
                _loggingService.Information($"自动发送间隔已更新为 {value} 毫秒");
            }
        }
    }

    partial void OnSelectedCommandChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SendDataText = string.Empty;
            _dataDisplayViewModel?.InitializeFieldsTemplate(string.Empty);
            return;
        }

        var command = _protocolService.GetCommand(value);
        if (command?.Request?.Data != null)
        {
            SendDataText = command.Request.Data;
            _loggingService.Information($"已加载命令 '{value}' 的请求数据");
        }
        else
        {
            SendDataText = string.Empty;
        }

        // 初始化字段模板
        _dataDisplayViewModel?.InitializeFieldsTemplate(value);
    }

    partial void OnCrcEnabledChanged(bool value)
    {
        if (value)
        {
            _loggingService.Information($"CRC校验已启用: {SelectedCrcTypeOption?.DisplayName ?? "未选择"}");
        }
        else
        {
            _loggingService.Information("CRC校验已禁用");
        }
    }

    partial void OnSelectedCrcTypeOptionChanged(CrcTypeOption? value)
    {
        if (value != null && CrcEnabled)
        {
            _loggingService.Information($"CRC校验类型已切换到: {value.DisplayName}");
        }
    }
}
