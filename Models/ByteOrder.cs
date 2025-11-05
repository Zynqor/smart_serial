using System.Runtime.Serialization;

namespace SerialProtocolAssistant.Models;

public enum ByteOrder
{
    [EnumMember(Value = "big")]
    BigEndian,

    [EnumMember(Value = "little")]
    LittleEndian
}
