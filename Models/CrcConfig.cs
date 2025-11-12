namespace SerialProtocolAssistant.Models;

/// <summary>
/// CRC校验配置
/// </summary>
public class CrcConfig
{
    /// <summary>
    /// 是否启用CRC校验
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// CRC类型
    /// </summary>
    public CrcType Type { get; set; } = CrcType.CRC16_MODBUS;

    /// <summary>
    /// CRC在数据帧中的位置（从末尾计算）
    /// 例如: -2 表示倒数第2个字节开始
    /// </summary>
    public int Position { get; set; } = -2;

    /// <summary>
    /// CRC字节长度（1, 2, 或 4）
    /// </summary>
    public int ByteLength { get; set; } = 2;

    /// <summary>
    /// CRC字节序（仅用于CRC16和CRC32）
    /// </summary>
    public ByteOrder ByteOrder { get; set; } = ByteOrder.LittleEndian;

    /// <summary>
    /// 是否在发送前自动计算并替换CRC
    /// </summary>
    public bool AutoCalculate { get; set; } = true;

    /// <summary>
    /// 是否在接收后自动验证CRC
    /// </summary>
    public bool AutoVerify { get; set; } = true;
}
