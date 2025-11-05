using System.IO;
using Newtonsoft.Json;
using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

public class ProtocolService : IProtocolService
{
    private readonly ILoggingService _loggingService;

    public Protocol? CurrentProtocol { get; private set; }

    public ProtocolService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void LoadProtocol(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _loggingService.Error($"协议文件不存在: {filePath}");
                return;
            }

            var json = File.ReadAllText(filePath);
            CurrentProtocol = JsonConvert.DeserializeObject<Protocol>(json);

            if (CurrentProtocol == null)
            {
                _loggingService.Error("协议文件反序列化失败");
                return;
            }

            _loggingService.Information($"成功加载协议: {CurrentProtocol.ProtocolName} v{CurrentProtocol.Version}");
            _loggingService.Information($"共加载 {CurrentProtocol.Commands.Count} 个命令");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载协议文件失败: {ex.Message}");
            CurrentProtocol = null;
        }
    }

    public List<string> GetCommandNames()
    {
        if (CurrentProtocol == null)
            return new List<string>();

        return CurrentProtocol.Commands.Select(c => c.Name).ToList();
    }

    public CommandDefinition? GetCommand(string commandName)
    {
        if (CurrentProtocol == null)
            return null;

        return CurrentProtocol.Commands.FirstOrDefault(c => c.Name == commandName);
    }
}
