using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 报警服务接口
/// </summary>
public interface IAlarmService
{
    /// <summary>
    /// 检查并处理报警
    /// </summary>
    /// <param name="device">设备定义</param>
    /// <param name="channel">通道定义</param>
    /// <param name="value">当前值</param>
    void CheckAlarm(DeviceDefinition device, ChannelDefinition channel, double value);

    /// <summary>
    /// 获取活动报警列表
    /// </summary>
    List<AlarmRecord> GetActiveAlarms();

    /// <summary>
    /// 确认报警
    /// </summary>
    void AcknowledgeAlarm(long alarmId);

    /// <summary>
    /// 报警触发事件
    /// </summary>
    event EventHandler<AlarmTriggeredEventArgs>? AlarmTriggered;

    /// <summary>
    /// 报警恢复事件
    /// </summary>
    event EventHandler<AlarmResolvedEventArgs>? AlarmResolved;
}

/// <summary>
/// 报警触发事件参数
/// </summary>
public class AlarmTriggeredEventArgs : EventArgs
{
    public AlarmRecord Alarm { get; set; } = new();
}

/// <summary>
/// 报警恢复事件参数
/// </summary>
public class AlarmResolvedEventArgs : EventArgs
{
    public AlarmRecord Alarm { get; set; } = new();
}
