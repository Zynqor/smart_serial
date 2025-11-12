namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备数据记录（用于数据库存储和历史查询）
/// </summary>
public class DeviceDataRecord
{
    /// <summary>
    /// 记录ID（数据库主键）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 通道ID
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// 通道名称
    /// </summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>
    /// 数值
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }
}
