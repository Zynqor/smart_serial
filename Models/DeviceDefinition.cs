namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备定义
/// </summary>
public class DeviceDefinition
{
    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    public byte DeviceId { get; set; }

    /// <summary>
    /// 通道定义列表
    /// </summary>
    public List<ChannelDefinition> Channels { get; set; } = new();
}
