using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SerialProtocolAssistant.Models;

/// <summary>
/// CRC校验类型枚举
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum CrcType
{
    /// <summary>
    /// 无CRC校验
    /// </summary>
    [EnumMember(Value = "none")]
    None,

    /// <summary>
    /// CRC8 - 多项式: 0x07
    /// </summary>
    [EnumMember(Value = "crc8")]
    CRC8,

    /// <summary>
    /// CRC8-ITU - 多项式: 0x07, 初始值: 0x00, 结果异或: 0x55
    /// </summary>
    [EnumMember(Value = "crc8_itu")]
    CRC8_ITU,

    /// <summary>
    /// CRC8-MAXIM - 多项式: 0x31, 初始值: 0x00, 结果异或: 0x00
    /// </summary>
    [EnumMember(Value = "crc8_maxim")]
    CRC8_MAXIM,

    /// <summary>
    /// CRC16-MODBUS - 最常用于Modbus RTU协议
    /// 多项式: 0x8005, 初始值: 0xFFFF, 结果异或: 0x0000, 低字节在前
    /// </summary>
    [EnumMember(Value = "crc16_modbus")]
    CRC16_MODBUS,

    /// <summary>
    /// CRC16-CCITT - 多项式: 0x1021, 初始值: 0xFFFF
    /// </summary>
    [EnumMember(Value = "crc16_ccitt")]
    CRC16_CCITT,

    /// <summary>
    /// CRC16-CCITT-FALSE - 多项式: 0x1021, 初始值: 0xFFFF, 结果异或: 0x0000
    /// </summary>
    [EnumMember(Value = "crc16_ccitt_false")]
    CRC16_CCITT_FALSE,

    /// <summary>
    /// CRC16-XMODEM - 多项式: 0x1021, 初始值: 0x0000
    /// </summary>
    [EnumMember(Value = "crc16_xmodem")]
    CRC16_XMODEM,

    /// <summary>
    /// CRC16-X25 - 多项式: 0x1021, 初始值: 0xFFFF, 结果异或: 0xFFFF
    /// </summary>
    [EnumMember(Value = "crc16_x25")]
    CRC16_X25,

    /// <summary>
    /// CRC16-USB - 多项式: 0x8005, 初始值: 0xFFFF, 结果异或: 0xFFFF
    /// </summary>
    [EnumMember(Value = "crc16_usb")]
    CRC16_USB,

    /// <summary>
    /// CRC16-IBM (CRC16-ANSI) - 多项式: 0x8005, 初始值: 0x0000
    /// </summary>
    [EnumMember(Value = "crc16_ibm")]
    CRC16_IBM,

    /// <summary>
    /// CRC16-DNP - 多项式: 0x3D65, 初始值: 0x0000, 结果异或: 0xFFFF
    /// </summary>
    [EnumMember(Value = "crc16_dnp")]
    CRC16_DNP,

    /// <summary>
    /// CRC32 - 标准CRC32，用于以太网、ZIP等
    /// 多项式: 0x04C11DB7, 初始值: 0xFFFFFFFF, 结果异或: 0xFFFFFFFF
    /// </summary>
    [EnumMember(Value = "crc32")]
    CRC32,

    /// <summary>
    /// CRC32-MPEG2 - 多项式: 0x04C11DB7, 初始值: 0xFFFFFFFF, 结果异或: 0x00000000
    /// </summary>
    [EnumMember(Value = "crc32_mpeg2")]
    CRC32_MPEG2
}
