using CommunityToolkit.Mvvm.ComponentModel;

namespace SerialProtocolAssistant.Models;

/// <summary>
/// 报警记录
/// </summary>
public partial class AlarmRecord : ObservableObject
{
    /// <summary>
    /// 记录ID（数据库主键）
    /// </summary>
    [ObservableProperty]
    private long _id;

    /// <summary>
    /// 报警开始时间
    /// </summary>
    [ObservableProperty]
    private DateTime _startTime;

    /// <summary>
    /// 报警结束时间（如果已恢复）
    /// </summary>
    [ObservableProperty]
    private DateTime? _endTime;

    /// <summary>
    /// 设备ID
    /// </summary>
    [ObservableProperty]
    private string _deviceId = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    [ObservableProperty]
    private string _deviceName = string.Empty;

    /// <summary>
    /// 通道ID
    /// </summary>
    [ObservableProperty]
    private string _channelId = string.Empty;

    /// <summary>
    /// 通道名称
    /// </summary>
    [ObservableProperty]
    private string _channelName = string.Empty;

    /// <summary>
    /// 报警类型
    /// </summary>
    [ObservableProperty]
    private AlarmType _alarmType;

    /// <summary>
    /// 触发报警时的数值
    /// </summary>
    [ObservableProperty]
    private double _triggerValue;

    /// <summary>
    /// 限值（上限或下限）
    /// </summary>
    [ObservableProperty]
    private double _limitValue;

    /// <summary>
    /// 报警状态
    /// </summary>
    [ObservableProperty]
    private AlarmStatus _status;

    /// <summary>
    /// 持续时长（秒）
    /// </summary>
    [ObservableProperty]
    private int? _duration;

    /// <summary>
    /// 单位
    /// </summary>
    [ObservableProperty]
    private string? _unit;

    /// <summary>
    /// 备注
    /// </summary>
    [ObservableProperty]
    private string? _notes;
}

/// <summary>
/// 报警类型
/// </summary>
public enum AlarmType
{
    /// <summary>
    /// 超上限
    /// </summary>
    UpperLimit,

    /// <summary>
    /// 低于下限
    /// </summary>
    LowerLimit
}

/// <summary>
/// 报警状态
/// </summary>
public enum AlarmStatus
{
    /// <summary>
    /// 活动中（未恢复）
    /// </summary>
    Active,

    /// <summary>
    /// 已确认（用户已查看）
    /// </summary>
    Acknowledged,

    /// <summary>
    /// 已恢复（数值回到正常范围）
    /// </summary>
    Resolved
}
