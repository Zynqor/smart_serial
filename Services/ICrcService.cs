using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// CRC校验服务接口
/// </summary>
public interface ICrcService
{
    /// <summary>
    /// 计算CRC16-Modbus校验值
    /// </summary>
    /// <param name="data">待校验的数据</param>
    /// <param name="offset">起始偏移</param>
    /// <param name="length">数据长度</param>
    /// <returns>CRC16校验值（小端序，低字节在前）</returns>
    ushort CalculateCrc16Modbus(byte[] data, int offset, int length);

    /// <summary>
    /// 计算CRC16-IBM校验值
    /// </summary>
    ushort CalculateCrc16IBM(byte[] data, int offset, int length);

    /// <summary>
    /// 计算CRC32校验值
    /// </summary>
    uint CalculateCrc32(byte[] data, int offset, int length);

    /// <summary>
    /// 计算Checksum校验值（累加和）
    /// </summary>
    byte CalculateChecksum(byte[] data, int offset, int length);

    /// <summary>
    /// 验证CRC校验（根据类型自动选择算法）
    /// </summary>
    /// <param name="data">完整数据（包含CRC）</param>
    /// <param name="crcType">CRC类型</param>
    /// <param name="dataOffset">数据起始偏移</param>
    /// <param name="dataLength">数据长度（不含CRC）</param>
    /// <param name="crcOffset">CRC字段偏移</param>
    /// <returns>校验是否通过</returns>
    bool VerifyCrc(byte[] data, CrcType crcType, int dataOffset, int dataLength, int crcOffset);

    /// <summary>
    /// 获取CRC字段的字节长度
    /// </summary>
    int GetCrcLength(CrcType crcType);
}
