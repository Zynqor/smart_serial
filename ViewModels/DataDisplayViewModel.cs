using CommunityToolkit.Mvvm.ComponentModel;
using SerialProtocolAssistant.Models;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace SerialProtocolAssistant.ViewModels;

public partial class DataDisplayViewModel : ObservableObject
{
    private readonly ISerialPortService _serialPortService;
    private readonly IProtocolService _protocolService;
    private readonly IFrameParserService _frameParserService;
    private readonly ILoggingService _loggingService;
    private readonly ProtocolControlViewModel _protocolControlViewModel;

    [ObservableProperty]
    private ObservableCollection<ParsedFieldResult> _parsedFields = new();

    public DataDisplayViewModel(
        ISerialPortService serialPortService,
        IProtocolService protocolService,
        IFrameParserService frameParserService,
        ILoggingService loggingService,
        ProtocolControlViewModel protocolControlViewModel)
    {
        _serialPortService = serialPortService;
        _protocolService = protocolService;
        _frameParserService = frameParserService;
        _loggingService = loggingService;
        _protocolControlViewModel = protocolControlViewModel;

        _serialPortService.DataReceived += OnDataReceived;

        // 注册自己到 ProtocolControlViewModel，以便接收命令选择变化通知
        _protocolControlViewModel.SetDataDisplayViewModel(this);

        // 初始化占位符空行，确保表格有固定高度
        InitializePlaceholderRows();
    }

    private void InitializePlaceholderRows()
    {
        // 添加10个占位符空行
        for (int i = 0; i < 10; i++)
        {
            ParsedFields.Add(new ParsedFieldResult
            {
                FieldName = "",
                FieldDescription = "",
                RawBytes = "",
                ParsedValue = "",
                Unit = ""
            });
        }
    }

    public void InitializeFieldsTemplate(string commandName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                ParsedFields.Clear();

                if (string.IsNullOrEmpty(commandName))
                {
                    return;
                }

                var command = _protocolService.GetCommand(commandName);
                if (command?.Response?.Fields == null)
                {
                    return;
                }

                // 根据字段定义创建空的字段结果
                foreach (var field in command.Response.Fields)
                {
                    ParsedFields.Add(new ParsedFieldResult
                    {
                        FieldName = field.Name,
                        FieldDescription = field.Description,
                        RawBytes = "--",
                        ParsedValue = "--",
                        Unit = field.Unit ?? ""
                    });
                }

                _loggingService.Information($"已初始化 {ParsedFields.Count} 个字段模板");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"初始化字段模板时出错: {ex.Message}");
            }
        });
    }

    private void OnDataReceived(object? sender, byte[] data)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var selectedCommand = _protocolControlViewModel.SelectedCommand;
                if (string.IsNullOrEmpty(selectedCommand))
                {
                    _loggingService.Warning("未选择命令，无法解析数据");
                    return;
                }

                var command = _protocolService.GetCommand(selectedCommand);
                if (command == null)
                {
                    _loggingService.Error($"找不到命令: {selectedCommand}");
                    return;
                }

                // 获取 CRC 配置（优先使用 UI 配置）
                var crcType = _protocolControlViewModel.GetSelectedCrcType();
                var crcOffset = _protocolControlViewModel.CrcOffset;

                // 使用动态 CRC 配置解析数据
                var results = _frameParserService.ParseFrame(data, command, crcType, crcOffset);

                // 更新现有字段的解析值，而不是清空重建
                foreach (var result in results)
                {
                    var existingField = ParsedFields.FirstOrDefault(f => f.FieldName == result.FieldName);
                    if (existingField != null)
                    {
                        existingField.RawBytes = result.RawBytes;
                        existingField.ParsedValue = result.ParsedValue;
                    }
                    else
                    {
                        // 如果字段不存在（可能是动态情况），则添加
                        ParsedFields.Add(result);
                    }
                }

                _loggingService.Information($"成功解析数据，共 {results.Count} 个字段");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"解析数据时出错: {ex.Message}");
            }
        });
    }
}
