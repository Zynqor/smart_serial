namespace SerialProtocolAssistant.Services;

/// <summary>
/// CRC校验计算服务接口
/// </summary>
public interface ICrcService
{
    /// <summary>
    /// 计算CRC值
    /// </summary>
    /// <param name="data">待计算的数据</param>
    /// <param name="crcType">CRC类型</param>
    /// <returns>CRC值（字节数组）</returns>
    byte[] Calculate(byte[] data, Models.CrcType crcType);

    /// <summary>
    /// 计算CRC值（指定范围）
    /// </summary>
    /// <param name="data">待计算的数据</param>
    /// <param name="offset">起始偏移量</param>
    /// <param name="length">计算长度</param>
    /// <param name="crcType">CRC类型</param>
    /// <returns>CRC值（字节数组）</returns>
    byte[] Calculate(byte[] data, int offset, int length, Models.CrcType crcType);

    /// <summary>
    /// 验证CRC
    /// </summary>
    /// <param name="data">包含CRC的完整数据</param>
    /// <param name="crcConfig">CRC配置</param>
    /// <returns>验证是否通过</returns>
    bool Verify(byte[] data, Models.CrcConfig crcConfig);

    /// <summary>
    /// 替换数据中的CRC值
    /// </summary>
    /// <param name="data">待替换的数据（会被修改）</param>
    /// <param name="crcConfig">CRC配置</param>
    void ReplaceCrc(byte[] data, Models.CrcConfig crcConfig);
}
