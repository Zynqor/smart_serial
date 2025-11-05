using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class ResponseDefinition
{
    [JsonProperty("fields", Required = Required.Always)]
    public List<FieldDefinition> Fields { get; set; } = new();

    /// <summary>
    /// 是否启用CRC校验
    /// </summary>
    [JsonProperty("crcEnabled")]
    public bool CrcEnabled { get; set; } = false;

    /// <summary>
    /// CRC类型（如"CRC16-Modbus"）
    /// </summary>
    [JsonProperty("crcType")]
    public string? CrcType { get; set; }

    /// <summary>
    /// CRC字段在数据中的偏移位置
    /// </summary>
    [JsonProperty("crcOffset")]
    public int CrcOffset { get; set; }

    /// <summary>
    /// CRC校验的数据起始偏移（通常为0）
    /// </summary>
    [JsonProperty("crcDataStart")]
    public int CrcDataStart { get; set; } = 0;

    /// <summary>
    /// CRC校验的数据长度（不包含CRC本身）
    /// 如果为0或未设置，则自动计算为 crcOffset - crcDataStart
    /// </summary>
    [JsonProperty("crcDataLength")]
    public int CrcDataLength { get; set; }
}
