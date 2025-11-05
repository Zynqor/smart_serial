using System.Text;
using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public class FrameParserService : IFrameParserService
{
    private readonly ILoggingService _loggingService;
    private readonly ICrcService _crcService;

    public FrameParserService(ILoggingService loggingService, ICrcService crcService)
    {
        _loggingService = loggingService;
        _crcService = crcService;
    }

    public List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command)
    {
        var results = new List<ParsedFieldResult>();

        // CRC校验（如果启用）
        if (command.Response.CrcEnabled)
        {
            bool crcValid = ValidateCrc(data, command.Response);

            if (!crcValid)
            {
                _loggingService.Error("CRC校验失败，数据被丢弃");

                // 返回一个错误结果而不是解析字段
                results.Add(new ParsedFieldResult
                {
                    FieldName = "CRC校验",
                    FieldDescription = "数据完整性校验",
                    RawBytes = "FAILED",
                    ParsedValue = "CRC校验失败，数据已被丢弃"
                });

                return results;
            }

            _loggingService.Information("CRC校验通过");
        }

        foreach (var field in command.Response.Fields)
        {
            var result = new ParsedFieldResult
            {
                FieldName = field.Name,
                FieldDescription = field.Description,
                Unit = field.Unit
            };

            try
            {
                // 检查数据长度是否足够
                if (field.Offset + field.ByteLength > data.Length)
                {
                    result.ParsedValue = "数据长度不足";
                    result.RawBytes = "";
                    results.Add(result);
                    continue;
                }

                // 提取字节
                var bytes = new byte[field.ByteLength];
                Array.Copy(data, field.Offset, bytes, 0, field.ByteLength);

                // 记录原始字节
                result.RawBytes = BitConverter.ToString(bytes).Replace("-", " ");

                // 根据类型解析
                result.ParsedValue = ParseValue(bytes, field);
            }
            catch (Exception ex)
            {
                result.ParsedValue = $"解析错误: {ex.Message}";
                _loggingService.Error($"解析字段 {field.Name} 时出错: {ex.Message}");
            }

            results.Add(result);
        }

        return results;
    }

    private string ParseValue(byte[] bytes, FieldDefinition field)
    {
        // 根据字节序调整字节数组
        var processedBytes = bytes;
        if (field.ByteOrder.HasValue && field.ByteOrder.Value == Models.ByteOrder.BigEndian)
        {
            processedBytes = bytes.Reverse().ToArray();
        }

        double numericValue = 0;
        string result = "";

        switch (field.Type)
        {
            case DataType.uint8:
                numericValue = bytes[0];
                result = ((int)numericValue * field.Multiplier).ToString("F2");
                break;

            case DataType.int8:
                numericValue = (sbyte)bytes[0];
                result = ((int)numericValue * field.Multiplier).ToString("F2");
                break;

            case DataType.uint16:
                if (processedBytes.Length >= 2)
                {
                    numericValue = BitConverter.ToUInt16(processedBytes, 0);
                    result = (numericValue * field.Multiplier).ToString("F2");
                }
                break;

            case DataType.int16:
                if (processedBytes.Length >= 2)
                {
                    numericValue = BitConverter.ToInt16(processedBytes, 0);
                    result = (numericValue * field.Multiplier).ToString("F2");
                }
                break;

            case DataType.uint32:
                if (processedBytes.Length >= 4)
                {
                    numericValue = BitConverter.ToUInt32(processedBytes, 0);
                    result = (numericValue * field.Multiplier).ToString("F2");
                }
                break;

            case DataType.int32:
                if (processedBytes.Length >= 4)
                {
                    numericValue = BitConverter.ToInt32(processedBytes, 0);
                    result = (numericValue * field.Multiplier).ToString("F2");
                }
                break;

            case DataType.@float:
                if (processedBytes.Length >= 4)
                {
                    numericValue = BitConverter.ToSingle(processedBytes, 0);
                    result = (numericValue * field.Multiplier).ToString("F4");
                }
                break;

            case DataType.hex:
                result = "0x" + BitConverter.ToString(bytes).Replace("-", "");
                break;

            case DataType.@string:
                result = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                break;

            default:
                result = "未知类型";
                break;
        }

        return result;
    }

    /// <summary>
    /// 验证CRC校验
    /// </summary>
    private bool ValidateCrc(byte[] data, ResponseDefinition response)
    {
        try
        {
            // 确定CRC校验的数据长度
            int crcDataLength = response.CrcDataLength;
            if (crcDataLength <= 0)
            {
                // 如果未指定，自动计算为 crcOffset - crcDataStart
                crcDataLength = response.CrcOffset - response.CrcDataStart;
            }

            // 检查数据长度是否足够
            if (data.Length < response.CrcOffset + 2)
            {
                _loggingService.Error($"数据长度不足，无法进行CRC校验。期望至少 {response.CrcOffset + 2} 字节，实际 {data.Length} 字节");
                return false;
            }

            // 根据CRC类型进行校验
            switch (response.CrcType?.ToUpper())
            {
                case "CRC16-MODBUS":
                case "CRC16MODBUS":
                case "MODBUS":
                    bool isValid = _crcService.VerifyCrc16Modbus(
                        data,
                        response.CrcDataStart,
                        crcDataLength,
                        response.CrcOffset);

                    if (!isValid)
                    {
                        // 计算期望的CRC以便调试
                        ushort calculatedCrc = _crcService.CalculateCrc16Modbus(
                            data,
                            response.CrcDataStart,
                            crcDataLength);

                        ushort actualCrc = (ushort)(data[response.CrcOffset] | (data[response.CrcOffset + 1] << 8));

                        _loggingService.Error(
                            $"CRC16-Modbus校验失败。期望: 0x{calculatedCrc:X4}, 实际: 0x{actualCrc:X4}");
                    }

                    return isValid;

                default:
                    _loggingService.Warning($"不支持的CRC类型: {response.CrcType}，跳过CRC校验");
                    return true; // 不支持的类型，默认通过
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"CRC校验过程中发生异常: {ex.Message}");
            return false;
        }
    }
}

    /// <summary>
    /// 解析数据帧（支持动态CRC配置，优先于JSON配置）
    /// </summary>
    public List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command, CrcType crcType, int crcOffset)
    {
        var results = new List<ParsedFieldResult>();

        // 使用动态CRC配置进行校验
        if (crcType != CrcType.None)
        {
            bool crcValid = ValidateCrcDynamic(data, crcType, crcOffset);

            if (!crcValid)
            {
                _loggingService.Error($"CRC校验失败（{crcType}），数据被丢弃");

                results.Add(new ParsedFieldResult
                {
                    FieldName = "CRC校验",
                    FieldDescription = "数据完整性校验",
                    RawBytes = "FAILED",
                    ParsedValue = $"CRC校验失败（{crcType}），数据已被丢弃"
                });

                return results;
            }

            _loggingService.Information($"CRC校验通过（{crcType}）");
        }

        // 解析字段（与原方法相同）
        foreach (var field in command.Response.Fields)
        {
            var result = new ParsedFieldResult
            {
                FieldName = field.Name,
                FieldDescription = field.Description,
                Unit = field.Unit
            };

            try
            {
                if (field.Offset + field.ByteLength > data.Length)
                {
                    result.ParsedValue = "数据长度不足";
                    result.RawBytes = "";
                    results.Add(result);
                    continue;
                }

                var bytes = new byte[field.ByteLength];
                Array.Copy(data, field.Offset, bytes, 0, field.ByteLength);
                result.RawBytes = BitConverter.ToString(bytes).Replace("-", " ");
                result.ParsedValue = ParseValue(bytes, field);
            }
            catch (Exception ex)
            {
                result.ParsedValue = $"解析错误: {ex.Message}";
                _loggingService.Error($"解析字段 {field.Name} 时出错: {ex.Message}");
            }

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// 验证CRC（使用动态配置）
    /// </summary>
    private bool ValidateCrcDynamic(byte[] data, CrcType crcType, int crcOffset)
    {
        try
        {
            // 数据起始位置固定为0，数据长度为CRC偏移
            int dataStart = 0;
            int dataLength = crcOffset;

            return _crcService.VerifyCrc(data, crcType, dataStart, dataLength, crcOffset);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"CRC校验过程中发生异常: {ex.Message}");
            return false;
        }
    }
