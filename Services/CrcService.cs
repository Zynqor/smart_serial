using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// CRC校验计算服务实现
/// </summary>
public class CrcService : ICrcService
{
    private readonly ILoggingService _loggingService;

    public CrcService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public byte[] Calculate(byte[] data, CrcType crcType)
    {
        return Calculate(data, 0, data.Length, crcType);
    }

    public byte[] Calculate(byte[] data, int offset, int length, CrcType crcType)
    {
        return crcType switch
        {
            CrcType.None => Array.Empty<byte>(),
            CrcType.CRC8 => new[] { CalculateCrc8(data, offset, length) },
            CrcType.CRC8_ITU => new[] { CalculateCrc8Itu(data, offset, length) },
            CrcType.CRC8_MAXIM => new[] { CalculateCrc8Maxim(data, offset, length) },
            CrcType.CRC16_MODBUS => BitConverter.GetBytes(CalculateCrc16Modbus(data, offset, length)),
            CrcType.CRC16_CCITT => BitConverter.GetBytes(CalculateCrc16Ccitt(data, offset, length)),
            CrcType.CRC16_CCITT_FALSE => BitConverter.GetBytes(CalculateCrc16CcittFalse(data, offset, length)),
            CrcType.CRC16_XMODEM => BitConverter.GetBytes(CalculateCrc16Xmodem(data, offset, length)),
            CrcType.CRC16_X25 => BitConverter.GetBytes(CalculateCrc16X25(data, offset, length)),
            CrcType.CRC16_USB => BitConverter.GetBytes(CalculateCrc16Usb(data, offset, length)),
            CrcType.CRC16_IBM => BitConverter.GetBytes(CalculateCrc16Ibm(data, offset, length)),
            CrcType.CRC16_DNP => BitConverter.GetBytes(CalculateCrc16Dnp(data, offset, length)),
            CrcType.CRC32 => BitConverter.GetBytes(CalculateCrc32(data, offset, length)),
            CrcType.CRC32_MPEG2 => BitConverter.GetBytes(CalculateCrc32Mpeg2(data, offset, length)),
            _ => throw new NotSupportedException($"不支持的CRC类型: {crcType}")
        };
    }

    public bool Verify(byte[] data, CrcConfig crcConfig)
    {
        if (!crcConfig.Enabled || crcConfig.Type == CrcType.None)
        {
            return true;
        }

        try
        {
            // 提取CRC位置
            int crcPosition = crcConfig.Position >= 0
                ? crcConfig.Position
                : data.Length + crcConfig.Position;

            if (crcPosition < 0 || crcPosition + crcConfig.ByteLength > data.Length)
            {
                _loggingService.Warning($"CRC位置无效: position={crcConfig.Position}, dataLength={data.Length}");
                return false;
            }

            // 计算CRC（不包含CRC字段本身）
            var calculatedCrc = Calculate(data, 0, crcPosition, crcConfig.Type);

            // 提取实际CRC
            var actualCrc = new byte[crcConfig.ByteLength];
            Array.Copy(data, crcPosition, actualCrc, 0, crcConfig.ByteLength);

            // 处理字节序
            if (crcConfig.ByteOrder == Models.ByteOrder.BigEndian && calculatedCrc.Length > 1)
            {
                Array.Reverse(calculatedCrc);
            }

            // 比较CRC
            bool isValid = calculatedCrc.SequenceEqual(actualCrc);

            if (!isValid)
            {
                _loggingService.Warning(
                    $"CRC验证失败: 期望={BitConverter.ToString(calculatedCrc)}, " +
                    $"实际={BitConverter.ToString(actualCrc)}");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"CRC验证异常: {ex.Message}");
            return false;
        }
    }

    public void ReplaceCrc(byte[] data, CrcConfig crcConfig)
    {
        if (!crcConfig.Enabled || !crcConfig.AutoCalculate || crcConfig.Type == CrcType.None)
        {
            return;
        }

        try
        {
            // 计算CRC位置
            int crcPosition = crcConfig.Position >= 0
                ? crcConfig.Position
                : data.Length + crcConfig.Position;

            if (crcPosition < 0 || crcPosition + crcConfig.ByteLength > data.Length)
            {
                _loggingService.Warning($"CRC位置无效: position={crcConfig.Position}, dataLength={data.Length}");
                return;
            }

            // 计算CRC（不包含CRC字段本身）
            var crc = Calculate(data, 0, crcPosition, crcConfig.Type);

            // 处理字节序
            if (crcConfig.ByteOrder == Models.ByteOrder.BigEndian && crc.Length > 1)
            {
                Array.Reverse(crc);
            }

            // 替换CRC
            Array.Copy(crc, 0, data, crcPosition, Math.Min(crc.Length, crcConfig.ByteLength));

            _loggingService.Debug($"已替换CRC: {BitConverter.ToString(crc)} at position {crcPosition}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"替换CRC异常: {ex.Message}");
        }
    }

    #region CRC8 算法

    private byte CalculateCrc8(byte[] data, int offset, int length)
    {
        byte crc = 0x00;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x80) != 0)
                    crc = (byte)((crc << 1) ^ 0x07);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }

    private byte CalculateCrc8Itu(byte[] data, int offset, int length)
    {
        byte crc = 0x00;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x80) != 0)
                    crc = (byte)((crc << 1) ^ 0x07);
                else
                    crc <<= 1;
            }
        }
        return (byte)(crc ^ 0x55);
    }

    private byte CalculateCrc8Maxim(byte[] data, int offset, int length)
    {
        byte crc = 0x00;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x01) != 0)
                    crc = (byte)((crc >> 1) ^ 0x8C);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }

    #endregion

    #region CRC16 算法

    private ushort CalculateCrc16Modbus(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }

    private ushort CalculateCrc16Ccitt(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }

    private ushort CalculateCrc16CcittFalse(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }

    private ushort CalculateCrc16Xmodem(byte[] data, int offset, int length)
    {
        ushort crc = 0x0000;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }

    private ushort CalculateCrc16X25(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0x8408);
                else
                    crc >>= 1;
            }
        }
        return (ushort)(crc ^ 0xFFFF);
    }

    private ushort CalculateCrc16Usb(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return (ushort)(crc ^ 0xFFFF);
    }

    private ushort CalculateCrc16Ibm(byte[] data, int offset, int length)
    {
        ushort crc = 0x0000;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }

    private ushort CalculateCrc16Dnp(byte[] data, int offset, int length)
    {
        ushort crc = 0x0000;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA6BC);
                else
                    crc >>= 1;
            }
        }
        return (ushort)(crc ^ 0xFFFF);
    }

    #endregion

    #region CRC32 算法

    private static readonly uint[] Crc32Table = GenerateCrc32Table();

    private static uint[] GenerateCrc32Table()
    {
        uint[] table = new uint[256];
        uint poly = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ poly;
                else
                    crc >>= 1;
            }
            table[i] = crc;
        }
        return table;
    }

    private uint CalculateCrc32(byte[] data, int offset, int length)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            byte index = (byte)((crc & 0xFF) ^ data[i]);
            crc = (crc >> 8) ^ Crc32Table[index];
        }
        return crc ^ 0xFFFFFFFF;
    }

    private uint CalculateCrc32Mpeg2(byte[] data, int offset, int length)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            byte index = (byte)((crc & 0xFF) ^ data[i]);
            crc = (crc >> 8) ^ Crc32Table[index];
        }
        return crc;
    }

    #endregion
}
