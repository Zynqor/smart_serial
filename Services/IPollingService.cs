using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设备轮询服务接口
/// </summary>
public interface IPollingService
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 启动轮询
    /// </summary>
    void Start();

    /// <summary>
    /// 停止轮询
    /// </summary>
    void Stop();

    /// <summary>
    /// 请求发送事件（当需要发送命令时触发）
    /// </summary>
    event EventHandler<SendRequestEventArgs>? SendRequest;

    /// <summary>
    /// 轮询状态变化事件
    /// </summary>
    event EventHandler<PollingStateChangedEventArgs>? StateChanged;
}

/// <summary>
/// 发送请求事件参数
/// </summary>
public class SendRequestEventArgs : EventArgs
{
    /// <summary>
    /// 设备定义
    /// </summary>
    public DeviceDefinition Device { get; set; } = new();

    /// <summary>
    /// 请求数据
    /// </summary>
    public byte[] RequestData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 轮询状态变化事件参数
/// </summary>
public class PollingStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// 当前轮询的设备
    /// </summary>
    public DeviceDefinition? CurrentDevice { get; set; }

    /// <summary>
    /// 变化时间
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
