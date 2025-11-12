using SerialProtocolAssistant.Models;
using System.Collections.Concurrent;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 报警服务实现
/// </summary>
public class AlarmService : IAlarmService
{
    private readonly ILoggingService _loggingService;
    private readonly IDatabaseService _databaseService;
    private readonly IAudioService _audioService;

    // 跟踪活动报警 - Key: DeviceId_ChannelId
    private readonly ConcurrentDictionary<string, AlarmRecord> _activeAlarms = new();

    public event EventHandler<AlarmTriggeredEventArgs>? AlarmTriggered;
    public event EventHandler<AlarmResolvedEventArgs>? AlarmResolved;

    public AlarmService(ILoggingService loggingService, IDatabaseService databaseService, IAudioService audioService)
    {
        _loggingService = loggingService;
        _databaseService = databaseService;
        _audioService = audioService;
    }

    public void CheckAlarm(DeviceDefinition device, ChannelDefinition channel, double value)
    {
        if (!channel.AlarmEnabled)
        {
            return;
        }

        var alarmKey = $"{device.Id}_{channel.Id}";
        var isCurrentlyAlarming = false;
        AlarmType? alarmType = null;
        double limitValue = 0;

        // 检查是否超限
        if (value > channel.UpperLimit)
        {
            isCurrentlyAlarming = true;
            alarmType = AlarmType.UpperLimit;
            limitValue = channel.UpperLimit;
        }
        else if (value < channel.LowerLimit)
        {
            isCurrentlyAlarming = true;
            alarmType = AlarmType.LowerLimit;
            limitValue = channel.LowerLimit;
        }

        // 检查是否有活动报警
        var hasActiveAlarm = _activeAlarms.TryGetValue(alarmKey, out var existingAlarm);

        if (isCurrentlyAlarming)
        {
            if (!hasActiveAlarm)
            {
                // 新报警触发
                TriggerAlarm(device, channel, value, alarmType!.Value, limitValue);
            }
            else
            {
                // 已有报警，检查类型是否改变
                if (existingAlarm!.AlarmType != alarmType!.Value)
                {
                    // 报警类型改变，先恢复旧报警，再触发新报警
                    ResolveAlarm(alarmKey, existingAlarm, value);
                    TriggerAlarm(device, channel, value, alarmType.Value, limitValue);
                }
                // 否则报警持续中，不需要操作
            }
        }
        else
        {
            if (hasActiveAlarm)
            {
                // 值恢复正常，解除报警
                ResolveAlarm(alarmKey, existingAlarm!, value);
            }
            // 否则一切正常，不需要操作
        }
    }

    private void TriggerAlarm(DeviceDefinition device, ChannelDefinition channel, double value, AlarmType alarmType, double limitValue)
    {
        var alarm = new AlarmRecord
        {
            StartTime = DateTime.Now,
            DeviceId = device.Id,
            DeviceName = device.Name,
            ChannelId = channel.Id,
            ChannelName = channel.Name,
            AlarmType = alarmType,
            TriggerValue = value,
            LimitValue = limitValue,
            Status = AlarmStatus.Active,
            Unit = channel.Unit
        };

        // 保存到数据库
        _databaseService.SaveAlarmRecord(alarm);

        // 添加到活动报警列表
        var alarmKey = $"{device.Id}_{channel.Id}";
        _activeAlarms[alarmKey] = alarm;

        // 播放报警音效
        _audioService.PlayAlarmSound();

        // 记录日志
        var typeText = alarmType == AlarmType.UpperLimit ? "超上限" : "低于下限";
        _loggingService.Warning(
            $"⚠ 报警触发: {device.Name} - {channel.Name} {typeText}, " +
            $"当前值: {value:F2}{channel.Unit}, 限值: {limitValue:F2}{channel.Unit}");

        // 触发事件
        AlarmTriggered?.Invoke(this, new AlarmTriggeredEventArgs { Alarm = alarm });
    }

    private void ResolveAlarm(string alarmKey, AlarmRecord alarm, double currentValue)
    {
        // 计算持续时间
        alarm.EndTime = DateTime.Now;
        alarm.Duration = (int)(alarm.EndTime.Value - alarm.StartTime).TotalSeconds;
        alarm.Status = AlarmStatus.Resolved;

        // 更新数据库
        _databaseService.UpdateAlarmRecord(alarm);

        // 从活动报警列表移除
        _activeAlarms.TryRemove(alarmKey, out _);

        // 记录日志
        _loggingService.Information(
            $"✓ 报警恢复: {alarm.DeviceName} - {alarm.ChannelName}, " +
            $"持续时间: {alarm.Duration}秒, 当前值: {currentValue:F2}{alarm.Unit}");

        // 触发事件
        AlarmResolved?.Invoke(this, new AlarmResolvedEventArgs { Alarm = alarm });
    }

    public List<AlarmRecord> GetActiveAlarms()
    {
        return _activeAlarms.Values.OrderByDescending(a => a.StartTime).ToList();
    }

    public void AcknowledgeAlarm(long alarmId)
    {
        var alarm = _activeAlarms.Values.FirstOrDefault(a => a.Id == alarmId);
        if (alarm != null)
        {
            alarm.Status = AlarmStatus.Acknowledged;
            _databaseService.UpdateAlarmRecord(alarm);

            _loggingService.Information($"报警已确认: {alarm.DeviceName} - {alarm.ChannelName}");
        }
    }
}
