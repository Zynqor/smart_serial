using CommunityToolkit.Mvvm.ComponentModel;

namespace SerialProtocolAssistant.Models;

/// <summary>
/// 通道数据点（运行时数据）
/// </summary>
public partial class ChannelDataPoint : ObservableObject
{
    /// <summary>
    /// 通道定义
    /// </summary>
    public ChannelDefinition Definition { get; set; } = new();

    /// <summary>
    /// 当前值
    /// </summary>
    [ObservableProperty]
    private double _value;

    /// <summary>
    /// 格式化后的值
    /// </summary>
    public string FormattedValue => Value.ToString($"F{Definition.DecimalPlaces}");

    /// <summary>
    /// 是否可见（用于图表显示）
    /// </summary>
    [ObservableProperty]
    private bool _isVisible = true;
}
