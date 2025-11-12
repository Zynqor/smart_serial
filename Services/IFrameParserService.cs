using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public interface IFrameParserService
{
    /// <summary>
    /// 解析数据帧（旧接口，向下兼容）
    /// </summary>
    List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command);

    /// <summary>
    /// 解析设备数据帧为通道数据字典
    /// </summary>
    /// <param name="data">接收到的数据</param>
    /// <param name="device">设备定义</param>
    /// <returns>通道ID -> (值, 原始字节) 的字典</returns>
    Dictionary<string, (double value, string rawBytes)> ParseDeviceFrame(byte[] data, DeviceDefinition device);

    /// <summary>
    /// 解析设备通道列表（包含所有字段信息）
    /// </summary>
    List<ParsedFieldResult> ParseDeviceChannels(byte[] data, DeviceDefinition device);
}
