using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class Protocol
{
    [JsonProperty("protocolName", Required = Required.Always)]
    public string ProtocolName { get; set; } = string.Empty;

    [JsonProperty("version", Required = Required.Always)]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 设备地址提取配置
    /// </summary>
    [JsonProperty("addressConfig")]
    public AddressConfig AddressConfig { get; set; } = new();

    /// <summary>
    /// 设备列表（多设备支持）
    /// </summary>
    [JsonProperty("devices")]
    public List<DeviceDefinition> Devices { get; set; } = new();

    /// <summary>
    /// 命令定义列表（保留向下兼容）
    /// </summary>
    [JsonProperty("commands")]
    public List<CommandDefinition> Commands { get; set; } = new();
}
