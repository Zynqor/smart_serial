namespace SerialProtocolAssistant.Services;

/// <summary>
/// CRC校验服务实现
/// 支持CRC16-Modbus算法（常用于串口通信）
/// </summary>
public class CrcService : ICrcService
{
    /// <summary>
    /// 计算CRC16-Modbus校验值
    /// CRC16-Modbus: 多项式0xA001（反向0x8005），初始值0xFFFF
    /// </summary>
    public ushort CalculateCrc16Modbus(byte[] data, int offset, int length)
    {
        if (data == null || data.Length < offset + length)
        {
            throw new ArgumentException("数据长度不足");
        }

        ushort crc = 0xFFFF;

        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        return crc;
    }

    /// <summary>
    /// 验证CRC16-Modbus校验
    /// </summary>
    public bool VerifyCrc16Modbus(byte[] data, int dataOffset, int dataLength, int crcOffset)
    {
        try
        {
            // 检查数据长度
            if (data == null || data.Length < crcOffset + 2)
            {
                return false;
            }

            // 计算期望的CRC值
            ushort calculatedCrc = CalculateCrc16Modbus(data, dataOffset, dataLength);

            // 从数据中提取实际的CRC值（Modbus使用小端序，低字节在前）
            ushort actualCrc = (ushort)(data[crcOffset] | (data[crcOffset + 1] << 8));

            return calculatedCrc == actualCrc;
        }
        catch
        {
            return false;
        }
    }
}
