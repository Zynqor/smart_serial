using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// CRC校验服务实现
/// 支持工业485常用的多种校验算法
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
    /// 计算CRC16-IBM校验值
    /// CRC16-IBM: 多项式0x8005，初始值0x0000
    /// </summary>
    public ushort CalculateCrc16IBM(byte[] data, int offset, int length)
    {
        if (data == null || data.Length < offset + length)
        {
            throw new ArgumentException("数据长度不足");
        }

        ushort crc = 0x0000;

        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ 0x8005);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return crc;
    }

    /// <summary>
    /// 计算CRC32校验值
    /// 标准CRC32算法
    /// </summary>
    public uint CalculateCrc32(byte[] data, int offset, int length)
    {
        if (data == null || data.Length < offset + length)
        {
            throw new ArgumentException("数据长度不足");
        }

        uint crc = 0xFFFFFFFF;
        uint[] table = GenerateCrc32Table();

        for (int i = offset; i < offset + length; i++)
        {
            byte index = (byte)((crc ^ data[i]) & 0xFF);
            crc = (crc >> 8) ^ table[index];
        }

        return ~crc;
    }

    /// <summary>
    /// 生成CRC32查找表
    /// </summary>
    private uint[] GenerateCrc32Table()
    {
        uint[] table = new uint[256];
        uint poly = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                {
                    crc = (crc >> 1) ^ poly;
                }
                else
                {
                    crc >>= 1;
                }
            }
            table[i] = crc;
        }

        return table;
    }

    /// <summary>
    /// 计算Checksum校验值（累加和，取低8位）
    /// </summary>
    public byte CalculateChecksum(byte[] data, int offset, int length)
    {
        if (data == null || data.Length < offset + length)
        {
            throw new ArgumentException("数据长度不足");
        }

        int sum = 0;
        for (int i = offset; i < offset + length; i++)
        {
            sum += data[i];
        }

        return (byte)(sum & 0xFF);
    }

    /// <summary>
    /// 验证CRC校验（根据类型自动选择算法）
    /// </summary>
    public bool VerifyCrc(byte[] data, CrcType crcType, int dataOffset, int dataLength, int crcOffset)
    {
        try
        {
            if (crcType == CrcType.None)
            {
                return true; // 无校验，直接通过
            }

            int crcLength = GetCrcLength(crcType);
            if (data == null || data.Length < crcOffset + crcLength)
            {
                return false;
            }

            switch (crcType)
            {
                case CrcType.CRC16_Modbus:
                    {
                        ushort calculatedCrc = CalculateCrc16Modbus(data, dataOffset, dataLength);
                        ushort actualCrc = (ushort)(data[crcOffset] | (data[crcOffset + 1] << 8));
                        return calculatedCrc == actualCrc;
                    }

                case CrcType.CRC16_IBM:
                    {
                        ushort calculatedCrc = CalculateCrc16IBM(data, dataOffset, dataLength);
                        // IBM使用大端序
                        ushort actualCrc = (ushort)((data[crcOffset] << 8) | data[crcOffset + 1]);
                        return calculatedCrc == actualCrc;
                    }

                case CrcType.CRC32:
                    {
                        uint calculatedCrc = CalculateCrc32(data, dataOffset, dataLength);
                        // CRC32使用小端序
                        uint actualCrc = (uint)(data[crcOffset] |
                                               (data[crcOffset + 1] << 8) |
                                               (data[crcOffset + 2] << 16) |
                                               (data[crcOffset + 3] << 24));
                        return calculatedCrc == actualCrc;
                    }

                case CrcType.Checksum:
                    {
                        byte calculatedChecksum = CalculateChecksum(data, dataOffset, dataLength);
                        byte actualChecksum = data[crcOffset];
                        return calculatedChecksum == actualChecksum;
                    }

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取CRC字段的字节长度
    /// </summary>
    public int GetCrcLength(CrcType crcType)
    {
        return crcType switch
        {
            CrcType.None => 0,
            CrcType.CRC16_Modbus => 2,
            CrcType.CRC16_IBM => 2,
            CrcType.CRC32 => 4,
            CrcType.Checksum => 1,
            _ => 0
        };
    }
}
