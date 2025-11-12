using System.Text;
using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public class FrameParserService : IFrameParserService
{
    private readonly ILoggingService _loggingService;

    public FrameParserService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command)
    {
        var results = new List<ParsedFieldResult>();

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

    public Dictionary<string, (double value, string rawBytes)> ParseDeviceFrame(byte[] data, DeviceDefinition device)
    {
        var results = new Dictionary<string, (double, string)>();

        foreach (var channel in device.Channels)
        {
            try
            {
                // 检查数据长度
                if (channel.Offset + channel.ByteLength > data.Length)
                {
                    _loggingService.Warning($"通道 {channel.Name} 数据长度不足");
                    continue;
                }

                // 提取字节
                var bytes = new byte[channel.ByteLength];
                Array.Copy(data, channel.Offset, bytes, 0, channel.ByteLength);

                // 解析为数值
                var value = ParseValueNumeric(bytes, channel);
                var rawBytes = BitConverter.ToString(bytes).Replace("-", " ");

                results[channel.Id] = (value, rawBytes);
            }
            catch (Exception ex)
            {
                _loggingService.Error($"解析通道 {channel.Name} 时出错: {ex.Message}");
            }
        }

        return results;
    }

    public List<ParsedFieldResult> ParseDeviceChannels(byte[] data, DeviceDefinition device)
    {
        var results = new List<ParsedFieldResult>();

        foreach (var channel in device.Channels)
        {
            var result = new ParsedFieldResult
            {
                FieldName = channel.Name,
                FieldDescription = channel.Description,
                Unit = channel.Unit
            };

            try
            {
                // 检查数据长度
                if (channel.Offset + channel.ByteLength > data.Length)
                {
                    result.ParsedValue = "数据长度不足";
                    result.RawBytes = "";
                    results.Add(result);
                    continue;
                }

                // 提取字节
                var bytes = new byte[channel.ByteLength];
                Array.Copy(data, channel.Offset, bytes, 0, channel.ByteLength);

                // 记录原始字节
                result.RawBytes = BitConverter.ToString(bytes).Replace("-", " ");

                // 解析值
                var value = ParseValueNumeric(bytes, channel);
                result.ParsedValue = value.ToString($"F{channel.DecimalPlaces}");
            }
            catch (Exception ex)
            {
                result.ParsedValue = $"解析错误: {ex.Message}";
                _loggingService.Error($"解析通道 {channel.Name} 时出错: {ex.Message}");
            }

            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// 解析字节为数值（用于报警检测和图表绘制）
    /// </summary>
    private double ParseValueNumeric(byte[] bytes, ChannelDefinition channel)
    {
        // 根据字节序调整字节数组
        var processedBytes = bytes;
        if (channel.ByteOrder.HasValue && channel.ByteOrder.Value == Models.ByteOrder.BigEndian)
        {
            processedBytes = bytes.Reverse().ToArray();
        }

        double value = 0;

        switch (channel.Type)
        {
            case DataType.uint8:
                value = bytes[0];
                break;

            case DataType.int8:
                value = (sbyte)bytes[0];
                break;

            case DataType.uint16:
                if (processedBytes.Length >= 2)
                    value = BitConverter.ToUInt16(processedBytes, 0);
                break;

            case DataType.int16:
                if (processedBytes.Length >= 2)
                    value = BitConverter.ToInt16(processedBytes, 0);
                break;

            case DataType.uint32:
                if (processedBytes.Length >= 4)
                    value = BitConverter.ToUInt32(processedBytes, 0);
                break;

            case DataType.int32:
                if (processedBytes.Length >= 4)
                    value = BitConverter.ToInt32(processedBytes, 0);
                break;

            case DataType.@float:
                if (processedBytes.Length >= 4)
                    value = BitConverter.ToSingle(processedBytes, 0);
                break;

            case DataType.hex:
            case DataType.@string:
                // 非数值类型返回0
                value = 0;
                break;
        }

        // 应用乘数因子
        return value * channel.Multiplier;
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
}
