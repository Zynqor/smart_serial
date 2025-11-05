using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class ResponseDefinition
{
    [JsonProperty("fields", Required = Required.Always)]
    public List<FieldDefinition> Fields { get; set; } = new();
}
