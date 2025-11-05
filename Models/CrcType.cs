namespace SerialProtocolAssistant.Models;

/// <summary>
/// CRC 校验类型枚举
/// 支持工业485常用的校验算法
/// </summary>
public enum CrcType
{
    /// <summary>
    /// 无校验
    /// </summary>
    None,

    /// <summary>
    /// CRC16-Modbus (最常用，2字节，小端序)
    /// 多项式: 0xA001, 初始值: 0xFFFF
    /// </summary>
    CRC16_Modbus,

    /// <summary>
    /// CRC16-IBM/ANSI (2字节)
    /// 多项式: 0x8005, 初始值: 0x0000
    /// </summary>
    CRC16_IBM,

    /// <summary>
    /// CRC32 (4字节)
    /// 标准 CRC32 算法，常用于文件校验
    /// </summary>
    CRC32,

    /// <summary>
    /// Checksum/LRC (纵向冗余校验，1字节)
    /// 简单的累加校验，对所有字节求和取低8位
    /// </summary>
    Checksum
}
