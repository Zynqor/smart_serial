using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备运行时信息（用于UI绑定和状态跟踪）
/// </summary>
public partial class DeviceRuntimeInfo : ObservableObject
{
    /// <summary>
    /// 设备定义
    /// </summary>
    [ObservableProperty]
    private DeviceDefinition _definition = new();

    /// <summary>
    /// 通道实时数据
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ChannelRuntimeData> _channelData = new();

    /// <summary>
    /// 设备状态
    /// </summary>
    [ObservableProperty]
    private DeviceStatus _status = DeviceStatus.Offline;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    [ObservableProperty]
    private DateTime? _lastUpdateTime;

    /// <summary>
    /// 连续超时次数
    /// </summary>
    [ObservableProperty]
    private int _consecutiveTimeouts;

    /// <summary>
    /// 总请求次数
    /// </summary>
    [ObservableProperty]
    private long _totalRequests;

    /// <summary>
    /// 成功响应次数
    /// </summary>
    [ObservableProperty]
    private long _successfulResponses;

    /// <summary>
    /// 失败次数
    /// </summary>
    [ObservableProperty]
    private long _failedResponses;

    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    [ObservableProperty]
    private double _averageResponseTime;

    /// <summary>
    /// 最后一次错误信息
    /// </summary>
    [ObservableProperty]
    private string? _lastError;
}

/// <summary>
/// 通道实时数据
/// </summary>
public partial class ChannelRuntimeData : ObservableObject
{
    /// <summary>
    /// 通道定义
    /// </summary>
    [ObservableProperty]
    private ChannelDefinition _definition = new();

    /// <summary>
    /// 当前值
    /// </summary>
    [ObservableProperty]
    private double _value;

    /// <summary>
    /// 格式化后的值（带单位）
    /// </summary>
    [ObservableProperty]
    private string _formattedValue = "--";

    /// <summary>
    /// 原始字节（十六进制字符串）
    /// </summary>
    [ObservableProperty]
    private string _rawBytes = "--";

    /// <summary>
    /// 是否在图表中可见
    /// </summary>
    [ObservableProperty]
    private bool _isVisible;

    /// <summary>
    /// 是否处于报警状态
    /// </summary>
    [ObservableProperty]
    private bool _isAlarming;

    /// <summary>
    /// 报警类型（如果正在报警）
    /// </summary>
    [ObservableProperty]
    private AlarmType? _alarmType;

    /// <summary>
    /// 最近更新时间
    /// </summary>
    [ObservableProperty]
    private DateTime _lastUpdateTime;
}

/// <summary>
/// 设备状态
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// 离线
    /// </summary>
    Offline,

    /// <summary>
    /// 在线
    /// </summary>
    Online,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout
}
