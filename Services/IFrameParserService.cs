using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public interface IFrameParserService
{
    List<ParsedFieldResult> ParseFrame(byte[] data, CommandDefinition command);
}
