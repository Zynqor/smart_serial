using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public interface IProtocolService
{
    Protocol? CurrentProtocol { get; }
    void LoadProtocol(string filePath);
    List<string> GetCommandNames();
    CommandDefinition? GetCommand(string commandName);
}
