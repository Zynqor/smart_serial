namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备定义
/// </summary>
public class DeviceDefinition
{
    /// <summary>
    /// 设备唯一标识符（例如: "DEV001"）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称（例如: "1号楼电流表"）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// RS-485总线地址（通常 0x01-0xF7，0x00为广播地址）
    /// </summary>
    public byte Address { get; set; }

    /// <summary>
    /// 是否启用该设备
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 轮询间隔（毫秒）
    /// </summary>
    public int PollingInterval { get; set; } = 1000;

    /// <summary>
    /// 读取命令定义
    /// </summary>
    public ReadCommand ReadCommand { get; set; } = new();

    /// <summary>
    /// 通道列表（设备的所有数据字段）
    /// </summary>
    public List<ChannelDefinition> Channels { get; set; } = new();

    /// <summary>
    /// 响应超时时间（毫秒）
    /// </summary>
    public int ResponseTimeout { get; set; } = 1000;

    /// <summary>
    /// 离线阈值（连续超时次数，超过此值标记为离线）
    /// </summary>
    public int OfflineThreshold { get; set; } = 3;

    /// <summary>
    /// 设备类型/型号（可选）
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 设备厂商（可选）
    /// </summary>
    public string? Manufacturer { get; set; }
}
