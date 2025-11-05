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
