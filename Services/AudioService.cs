using NAudio.Wave;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 音频服务实现（使用NAudio）
/// </summary>
public class AudioService : IAudioService, IDisposable
{
    private readonly ILoggingService _loggingService;
    private IWavePlayer? _wavePlayer;
    private AudioFileReader? _audioFileReader;
    private string _alarmSoundPath = "Assets/alarm.wav";
    private bool _isEnabled = true;
    private readonly object _lock = new();

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public AudioService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void PlayAlarmSound()
    {
        if (!_isEnabled)
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                // 如果音频文件不存在，使用系统蜂鸣声
                if (!File.Exists(_alarmSoundPath))
                {
                    _loggingService.Debug($"报警音效文件不存在: {_alarmSoundPath}，使用系统蜂鸣声");
                    PlaySystemBeep();
                    return;
                }

                // 停止当前播放
                Stop();

                // 创建新的播放器
                _audioFileReader = new AudioFileReader(_alarmSoundPath);
                _wavePlayer = new WaveOutEvent();
                _wavePlayer.Init(_audioFileReader);
                _wavePlayer.Play();

                _loggingService.Debug("播放报警音效");
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"播放报警音效失败: {ex.Message}");
            // 降级到系统蜂鸣声
            PlaySystemBeep();
        }
    }

    public void SetAlarmSoundPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _loggingService.Warning("报警音效路径为空");
            return;
        }

        if (!File.Exists(path))
        {
            _loggingService.Warning($"报警音效文件不存在: {path}");
        }

        _alarmSoundPath = path;
        _loggingService.Information($"已设置报警音效路径: {path}");
    }

    public void TestPlay()
    {
        _loggingService.Information("测试播放报警音效");
        PlayAlarmSound();
    }

    public void Stop()
    {
        lock (_lock)
        {
            try
            {
                _wavePlayer?.Stop();
                _wavePlayer?.Dispose();
                _audioFileReader?.Dispose();
                _wavePlayer = null;
                _audioFileReader = null;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"停止音频播放失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 播放系统蜂鸣声（降级方案）
    /// </summary>
    private void PlaySystemBeep()
    {
        try
        {
            // 播放系统蜂鸣声
            Console.Beep(1000, 200); // 1000Hz, 200ms
        }
        catch
        {
            // 忽略错误（某些系统不支持蜂鸣声）
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
