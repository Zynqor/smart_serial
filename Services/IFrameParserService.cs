using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public interface IFrameParserService
{
    List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command);
    
    /// <summary>
    /// 解析数据帧（支持动态CRC配置）
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="command">命令定义</param>
    /// <param name="crcType">CRC类型（优先于JSON配置）</param>
    /// <param name="crcOffset">CRC偏移位置</param>
    /// <returns>解析结果列表</returns>
    List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command, CrcType crcType, int crcOffset);
}
