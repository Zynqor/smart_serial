namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备地址提取配置
/// </summary>
public class AddressConfig
{
    /// <summary>
    /// 地址字节在数据帧中的偏移量（0-based）
    /// 通常Modbus RTU协议中地址是第一个字节，offset = 0
    /// </summary>
    public int Offset { get; set; } = 0;

    /// <summary>
    /// 地址字节长度（通常为1字节）
    /// </summary>
    public int ByteLength { get; set; } = 1;

    /// <summary>
    /// 地址字节序（仅当ByteLength > 1时有效）
    /// </summary>
    public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;
}
