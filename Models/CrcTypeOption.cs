namespace SerialProtocolAssistant.Models;

/// <summary>
/// CRC 类型选项（用于UI绑定）
/// </summary>
public class CrcTypeOption
{
    public CrcType Type { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}
