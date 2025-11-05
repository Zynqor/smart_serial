using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class Protocol
{
    [JsonProperty("protocolName", Required = Required.Always)]
    public string ProtocolName { get; set; } = string.Empty;

    [JsonProperty("version", Required = Required.Always)]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("commands", Required = Required.Always)]
    public List<CommandDefinition> Commands { get; set; } = new();
}
