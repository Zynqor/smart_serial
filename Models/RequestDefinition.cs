using Newtonsoft.Json;

namespace SerialProtocolAssistant.Models;

public class RequestDefinition
{
    [JsonProperty("data")]
    public string? Data { get; set; }
}
