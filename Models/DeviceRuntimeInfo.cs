using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SerialProtocolAssistant.Models;

/// <summary>
/// 设备运行时信息
/// </summary>
public partial class DeviceRuntimeInfo : ObservableObject
{
    /// <summary>
    /// 设备定义
    /// </summary>
    public DeviceDefinition Definition { get; set; } = new();

    /// <summary>
    /// 通道数据
    /// </summary>
    public ObservableCollection<ChannelDataPoint> ChannelData { get; set; } = new();

    /// <summary>
    /// 设备状态
    /// </summary>
    [ObservableProperty]
    private string _status = "Offline";

    /// <summary>
    /// 最后更新时间
    /// </summary>
    [ObservableProperty]
    private DateTime _lastUpdateTime = DateTime.Now;
}
