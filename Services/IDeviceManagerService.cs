using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设备管理服务接口
/// </summary>
public interface IDeviceManagerService
{
    /// <summary>
    /// 当前加载的协议
    /// </summary>
    Protocol? CurrentProtocol { get; }

    /// <summary>
    /// 加载协议配置文件
    /// </summary>
    /// <param name="filePath">协议文件路径</param>
    void LoadProtocol(string filePath);

    /// <summary>
    /// 获取所有设备列表
    /// </summary>
    List<DeviceDefinition> GetAllDevices();

    /// <summary>
    /// 获取启用的设备列表
    /// </summary>
    List<DeviceDefinition> GetEnabledDevices();

    /// <summary>
    /// 根据设备ID获取设备
    /// </summary>
    DeviceDefinition? GetDeviceById(string deviceId);

    /// <summary>
    /// 根据设备地址获取设备
    /// </summary>
    DeviceDefinition? GetDeviceByAddress(byte address);

    /// <summary>
    /// 从接收到的数据中提取设备地址
    /// </summary>
    /// <param name="data">接收到的数据</param>
    /// <returns>设备地址</returns>
    byte ExtractAddress(byte[] data);

    /// <summary>
    /// 构建设备读取命令
    /// </summary>
    /// <param name="device">设备定义</param>
    /// <returns>构建好的命令字节数组</returns>
    byte[] BuildReadCommand(DeviceDefinition device);

    /// <summary>
    /// 验证接收到的数据（CRC校验）
    /// </summary>
    /// <param name="device">设备定义</param>
    /// <param name="data">接收到的数据</param>
    /// <returns>验证是否通过</returns>
    bool ValidateReceivedData(DeviceDefinition device, byte[] data);

    /// <summary>
    /// 添加或更新设备
    /// </summary>
    void AddOrUpdateDevice(DeviceDefinition device);

    /// <summary>
    /// 删除设备
    /// </summary>
    void RemoveDevice(string deviceId);

    /// <summary>
    /// 保存协议到文件
    /// </summary>
    void SaveProtocol(string filePath);
}
