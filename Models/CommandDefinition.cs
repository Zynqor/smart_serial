using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class CommandDefinition
{
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description", Required = Required.Always)]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("request")]
    public RequestDefinition? Request { get; set; }

    [JsonProperty("response", Required = Required.Always)]
    public ResponseDefinition Response { get; set; } = new();
}
