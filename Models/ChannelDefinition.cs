namespace SerialProtocolAssistant.Models;

/// <summary>
/// 通道定义
/// </summary>
public class ChannelDefinition
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 显示颜色（用于图表）
    /// </summary>
    public string Color { get; set; } = "#2196F3";

    /// <summary>
    /// 小数位数
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;
}
