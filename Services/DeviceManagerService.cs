using System.IO;
using Newtonsoft.Json;
using SerialProtocolAssistant.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设备管理服务实现
/// </summary>
public class DeviceManagerService : IDeviceManagerService
{
    private readonly ILoggingService _loggingService;
    private readonly ICrcService _crcService;
    private Protocol? _currentProtocol;

    public Protocol? CurrentProtocol => _currentProtocol;

    public DeviceManagerService(ILoggingService loggingService, ICrcService crcService)
    {
        _loggingService = loggingService;
        _crcService = crcService;
    }

    public void LoadProtocol(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _loggingService.Error($"协议文件不存在: {filePath}");
                throw new FileNotFoundException($"协议文件不存在: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            _currentProtocol = JsonConvert.DeserializeObject<Protocol>(json);

            if (_currentProtocol == null)
            {
                throw new InvalidOperationException("协议文件解析失败");
            }

            _loggingService.Information(
                $"已加载协议: {_currentProtocol.ProtocolName} v{_currentProtocol.Version}, " +
                $"设备数量: {_currentProtocol.Devices.Count}");

            // 验证设备配置
            ValidateProtocol();
        }
        catch (Exception ex)
        {
            _loggingService.Error($"加载协议失败: {ex.Message}");
            throw;
        }
    }

    private void ValidateProtocol()
    {
        if (_currentProtocol == null) return;

        // 检查地址冲突
        var addressGroups = _currentProtocol.Devices.GroupBy(d => d.Address);
        foreach (var group in addressGroups)
        {
            if (group.Count() > 1)
            {
                var deviceNames = string.Join(", ", group.Select(d => d.Name));
                _loggingService.Warning($"检测到地址冲突: 地址0x{group.Key:X2} 被以下设备使用: {deviceNames}");
            }
        }

        // 检查设备ID唯一性
        var idGroups = _currentProtocol.Devices.GroupBy(d => d.Id);
        foreach (var group in idGroups)
        {
            if (group.Count() > 1)
            {
                _loggingService.Warning($"检测到设备ID重复: {group.Key}");
            }
        }
    }

    public List<DeviceDefinition> GetAllDevices()
    {
        return _currentProtocol?.Devices ?? new List<DeviceDefinition>();
    }

    public List<DeviceDefinition> GetEnabledDevices()
    {
        return _currentProtocol?.Devices.Where(d => d.Enabled).ToList() ?? new List<DeviceDefinition>();
    }

    public DeviceDefinition? GetDeviceById(string deviceId)
    {
        return _currentProtocol?.Devices.FirstOrDefault(d => d.Id == deviceId);
    }

    public DeviceDefinition? GetDeviceByAddress(byte address)
    {
        return _currentProtocol?.Devices.FirstOrDefault(d => d.Address == address);
    }

    public byte ExtractAddress(byte[] data)
    {
        if (_currentProtocol == null || data == null || data.Length == 0)
        {
            return 0;
        }

        var config = _currentProtocol.AddressConfig;

        if (config.Offset < 0 || config.Offset + config.ByteLength > data.Length)
        {
            _loggingService.Warning($"地址提取失败: offset={config.Offset}, length={config.ByteLength}, dataLength={data.Length}");
            return 0;
        }

        if (config.ByteLength == 1)
        {
            return data[config.Offset];
        }
        else if (config.ByteLength == 2)
        {
            var bytes = new byte[2];
            Array.Copy(data, config.Offset, bytes, 0, 2);

            if (config.ByteOrder == ByteOrder.LittleEndian)
            {
                return (byte)BitConverter.ToUInt16(bytes, 0);
            }
            else
            {
                Array.Reverse(bytes);
                return (byte)BitConverter.ToUInt16(bytes, 0);
            }
        }
        else
        {
            _loggingService.Warning($"不支持的地址字节长度: {config.ByteLength}");
            return 0;
        }
    }

    public byte[] BuildReadCommand(DeviceDefinition device)
    {
        try
        {
            var command = device.ReadCommand;
            if (string.IsNullOrWhiteSpace(command.Data))
            {
                throw new InvalidOperationException($"设备 {device.Name} 的读取命令为空");
            }

            // 移除空格并转换为字节数组
            var hexString = command.Data.Replace(" ", "").Replace("-", "");

            // 检查是否为有效的十六进制字符串
            if (!IsValidHexString(hexString))
            {
                throw new InvalidOperationException($"无效的十六进制命令: {command.Data}");
            }

            // 转换为字节数组
            var bytes = HexStringToBytes(hexString);

            // 替换设备地址（如果需要）
            if (command.HasAddressPlaceholder && command.AddressOffset >= 0 && command.AddressOffset < bytes.Length)
            {
                bytes[command.AddressOffset] = device.Address;
                _loggingService.Debug($"已替换地址字节: offset={command.AddressOffset}, address=0x{device.Address:X2}");
            }

            // 计算并替换CRC（如果启用）
            if (command.CrcConfig.Enabled && command.CrcConfig.AutoCalculate)
            {
                _crcService.ReplaceCrc(bytes, command.CrcConfig);
            }

            _loggingService.Debug($"构建命令 [{device.Name}]: {BitConverter.ToString(bytes)}");
            return bytes;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"构建读取命令失败 [{device.Name}]: {ex.Message}");
            throw;
        }
    }

    public bool ValidateReceivedData(DeviceDefinition device, byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            _loggingService.Warning("接收到空数据");
            return false;
        }

        try
        {
            // 验证CRC（如果启用）
            if (device.ReadCommand.CrcConfig.Enabled && device.ReadCommand.CrcConfig.AutoVerify)
            {
                bool crcValid = _crcService.Verify(data, device.ReadCommand.CrcConfig);

                if (!crcValid)
                {
                    _loggingService.Warning($"CRC校验失败 [{device.Name}]: {BitConverter.ToString(data)}");
                    return false;
                }

                _loggingService.Debug($"CRC校验通过 [{device.Name}]");
            }

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"验证接收数据失败 [{device.Name}]: {ex.Message}");
            return false;
        }
    }

    public void AddOrUpdateDevice(DeviceDefinition device)
    {
        if (_currentProtocol == null)
        {
            throw new InvalidOperationException("未加载协议");
        }

        var existing = _currentProtocol.Devices.FirstOrDefault(d => d.Id == device.Id);
        if (existing != null)
        {
            // 更新现有设备
            var index = _currentProtocol.Devices.IndexOf(existing);
            _currentProtocol.Devices[index] = device;
            _loggingService.Information($"已更新设备: {device.Name}");
        }
        else
        {
            // 添加新设备
            _currentProtocol.Devices.Add(device);
            _loggingService.Information($"已添加设备: {device.Name}");
        }

        ValidateProtocol();
    }

    public void RemoveDevice(string deviceId)
    {
        if (_currentProtocol == null)
        {
            throw new InvalidOperationException("未加载协议");
        }

        var device = _currentProtocol.Devices.FirstOrDefault(d => d.Id == deviceId);
        if (device != null)
        {
            _currentProtocol.Devices.Remove(device);
            _loggingService.Information($"已删除设备: {device.Name}");
        }
    }

    public void SaveProtocol(string filePath)
    {
        try
        {
            if (_currentProtocol == null)
            {
                throw new InvalidOperationException("未加载协议");
            }

            var json = JsonConvert.SerializeObject(_currentProtocol, Formatting.Indented);
            File.WriteAllText(filePath, json);

            _loggingService.Information($"协议已保存到: {filePath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存协议失败: {ex.Message}");
            throw;
        }
    }

    #region 辅助方法

    /// <summary>
    /// 检查是否为有效的十六进制字符串
    /// </summary>
    private bool IsValidHexString(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return false;
        if (hex.Length % 2 != 0) return false;
        return Regex.IsMatch(hex, @"^[0-9A-Fa-f]+$");
    }

    /// <summary>
    /// 十六进制字符串转字节数组
    /// </summary>
    private byte[] HexStringToBytes(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];

        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    #endregion
}
