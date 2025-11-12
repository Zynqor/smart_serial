namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设置服务接口
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 获取设置值
    /// </summary>
    T GetSetting<T>(string key, T defaultValue);

    /// <summary>
    /// 设置值
    /// </summary>
    void SetSetting<T>(string key, T value);

    /// <summary>
    /// 保存窗口位置和大小
    /// </summary>
    void SaveWindowBounds(double left, double top, double width, double height);

    /// <summary>
    /// 加载窗口位置和大小
    /// </summary>
    (double left, double top, double width, double height)? LoadWindowBounds();

    /// <summary>
    /// 删除设置
    /// </summary>
    void RemoveSetting(string key);

    /// <summary>
    /// 清除所有设置
    /// </summary>
    void ClearAllSettings();

    /// <summary>
    /// 检查设置是否存在
    /// </summary>
    bool HasSetting(string key);
}
