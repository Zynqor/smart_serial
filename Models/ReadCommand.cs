namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备读取命令定义
/// </summary>
public class ReadCommand
{
    /// <summary>
    /// 命令数据（十六进制字符串，支持占位符）
    /// 例如: "XX0320010008XXXX"
    /// XX - 设备地址占位符
    /// XXXX - CRC占位符
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// 是否包含设备地址占位符
    /// </summary>
    public bool HasAddressPlaceholder { get; set; } = true;

    /// <summary>
    /// 设备地址在命令中的字节偏移量（0-based）
    /// </summary>
    public int AddressOffset { get; set; } = 0;

    /// <summary>
    /// CRC配置
    /// </summary>
    public CrcConfig CrcConfig { get; set; } = new()
    {
        Enabled = true,
        Type = CrcType.CRC16_MODBUS,
        Position = -2,
        ByteLength = 2,
        ByteOrder = ByteOrder.LittleEndian,
        AutoCalculate = true,
        AutoVerify = true
    };

    /// <summary>
    /// 命令描述
    /// </summary>
    public string? Description { get; set; }
}
