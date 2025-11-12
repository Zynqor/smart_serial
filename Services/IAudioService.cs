namespace SerialProtocolAssistant.Services;

/// <summary>
/// 音频服务接口
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 是否启用音效
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// 播放报警音效
    /// </summary>
    void PlayAlarmSound();

    /// <summary>
    /// 设置报警音效文件路径
    /// </summary>
    void SetAlarmSoundPath(string path);

    /// <summary>
    /// 测试播放音效
    /// </summary>
    void TestPlay();

    /// <summary>
    /// 停止播放
    /// </summary>
    void Stop();
}
