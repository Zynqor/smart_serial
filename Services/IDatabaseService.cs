using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 数据库服务接口
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// 初始化数据库（创建表结构）
    /// </summary>
    void Initialize();

    /// <summary>
    /// 保存设备数据
    /// </summary>
    void SaveDeviceData(string deviceId, string deviceName, string channelId, string channelName,
                       double value, string? unit, DateTime timestamp);

    /// <summary>
    /// 批量保存设备数据
    /// </summary>
    void SaveDeviceDataBatch(List<DeviceDataRecord> records);

    /// <summary>
    /// 查询设备数据
    /// </summary>
    /// <param name="deviceId">设备ID（null表示全部）</param>
    /// <param name="channelId">通道ID（null表示全部）</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="limit">返回记录数限制（0表示不限制）</param>
    List<DeviceDataRecord> QueryData(string? deviceId, string? channelId,
                                     DateTime startTime, DateTime endTime, int limit = 1000);

    /// <summary>
    /// 保存报警记录
    /// </summary>
    void SaveAlarmRecord(AlarmRecord alarm);

    /// <summary>
    /// 更新报警记录（用于标记已恢复）
    /// </summary>
    void UpdateAlarmRecord(AlarmRecord alarm);

    /// <summary>
    /// 查询报警记录
    /// </summary>
    /// <param name="deviceId">设备ID（null表示全部）</param>
    /// <param name="status">报警状态（null表示全部）</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    List<AlarmRecord> QueryAlarms(string? deviceId, AlarmStatus? status,
                                  DateTime startTime, DateTime endTime);

    /// <summary>
    /// 获取活动报警列表
    /// </summary>
    List<AlarmRecord> GetActiveAlarms();

    /// <summary>
    /// 获取报警统计
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>字典：设备ID -> 报警次数</returns>
    Dictionary<string, int> GetAlarmStatistics(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 获取报警统计（按类型）
    /// </summary>
    Dictionary<AlarmType, int> GetAlarmStatisticsByType(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 清理历史数据
    /// </summary>
    /// <param name="olderThan">清理此时间之前的数据</param>
    /// <returns>删除的记录数</returns>
    int CleanupOldData(DateTime olderThan);

    /// <summary>
    /// 获取数据库统计信息
    /// </summary>
    DatabaseStatistics GetStatistics();

    /// <summary>
    /// 优化数据库（VACUUM）
    /// </summary>
    void OptimizeDatabase();

    /// <summary>
    /// 备份数据库
    /// </summary>
    /// <param name="backupPath">备份文件路径</param>
    void BackupDatabase(string backupPath);
}

/// <summary>
/// 数据库统计信息
/// </summary>
public class DatabaseStatistics
{
    public long TotalDataRecords { get; set; }
    public long TotalAlarmRecords { get; set; }
    public long DatabaseSizeBytes { get; set; }
    public DateTime? OldestDataTimestamp { get; set; }
    public DateTime? NewestDataTimestamp { get; set; }
}
