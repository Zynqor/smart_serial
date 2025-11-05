using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SerialProtocolAssistant.Models;

public class FieldDefinition
{
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description", Required = Required.Always)]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("offset", Required = Required.Always)]
    public int Offset { get; set; }

    [JsonProperty("byteLength", Required = Required.Always)]
    public int ByteLength { get; set; }

    [JsonProperty("type", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public DataType Type { get; set; }

    [JsonProperty("byteOrder")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ByteOrder? ByteOrder { get; set; }

    [JsonProperty("multiplier")]
    public double Multiplier { get; set; } = 1.0;

    [JsonProperty("unit")]
    public string? Unit { get; set; }
}
