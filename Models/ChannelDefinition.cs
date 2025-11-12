namespace SerialProtocolAssistant.Models;

/// <summary>
/// 通道定义（设备的一个数据字段）
/// </summary>
public class ChannelDefinition
{
    /// <summary>
    /// 通道唯一标识符
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 通道名称（显示在UI上）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 通道描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 数据在响应帧中的字节偏移量（0-based）
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 数据占用的字节长度
    /// </summary>
    public int ByteLength { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public DataType Type { get; set; }

    /// <summary>
    /// 字节序（可选，如果未指定则使用默认值）
    /// </summary>
    public ByteOrder? ByteOrder { get; set; }

    /// <summary>
    /// 乘数因子（用于单位转换）
    /// </summary>
    public double Multiplier { get; set; } = 1.0;

    /// <summary>
    /// 单位（例如: "A", "V", "℃", "%"）
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 图表显示颜色（十六进制格式，例如: "#FF0000"）
    /// </summary>
    public string Color { get; set; } = "#2196F3";

    /// <summary>
    /// 是否启用报警检测
    /// </summary>
    public bool AlarmEnabled { get; set; } = false;

    /// <summary>
    /// 下限值（低于此值触发报警）
    /// </summary>
    public double LowerLimit { get; set; } = 0.0;

    /// <summary>
    /// 上限值（高于此值触发报警）
    /// </summary>
    public double UpperLimit { get; set; } = 100.0;

    /// <summary>
    /// 是否在图例中默认勾选显示
    /// </summary>
    public bool DefaultVisible { get; set; } = true;

    /// <summary>
    /// 小数点位数（用于显示格式化）
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;
}
