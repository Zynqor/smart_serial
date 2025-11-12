using SerialProtocolAssistant.Models;
using System.Collections.Concurrent;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设备轮询服务实现
/// </summary>
public class PollingService : IPollingService, IDisposable
{
    private readonly ILoggingService _loggingService;
    private readonly IDeviceManagerService _deviceManager;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _pollingTask;
    private bool _isRunning;

    // 每个设备的下次轮询时间
    private readonly ConcurrentDictionary<string, DateTime> _nextPollTime = new();

    public bool IsRunning => _isRunning;

    public event EventHandler<SendRequestEventArgs>? SendRequest;
    public event EventHandler<PollingStateChangedEventArgs>? StateChanged;

    public PollingService(ILoggingService loggingService, IDeviceManagerService deviceManager)
    {
        _loggingService = loggingService;
        _deviceManager = deviceManager;
    }

    public void Start()
    {
        if (_isRunning)
        {
            _loggingService.Warning("轮询服务已在运行中");
            return;
        }

        try
        {
            var devices = _deviceManager.GetEnabledDevices();
            if (devices.Count == 0)
            {
                _loggingService.Warning("没有启用的设备，无法启动轮询");
                throw new InvalidOperationException("没有启用的设备");
            }

            _loggingService.Information($"启动轮询服务，设备数量: {devices.Count}");

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            // 初始化每个设备的下次轮询时间为当前时间（立即轮询）
            foreach (var device in devices)
            {
                _nextPollTime[device.Id] = DateTime.Now;
            }

            // 启动轮询任务
            _pollingTask = Task.Run(() => PollingLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            // 触发状态变化事件
            StateChanged?.Invoke(this, new PollingStateChangedEventArgs { IsRunning = true });
        }
        catch (Exception ex)
        {
            _loggingService.Error($"启动轮询服务失败: {ex.Message}");
            _isRunning = false;
            throw;
        }
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        try
        {
            _loggingService.Information("停止轮询服务");

            _cancellationTokenSource?.Cancel();
            _pollingTask?.Wait(TimeSpan.FromSeconds(5));

            _isRunning = false;
            _nextPollTime.Clear();

            // 触发状态变化事件
            StateChanged?.Invoke(this, new PollingStateChangedEventArgs { IsRunning = false });
        }
        catch (Exception ex)
        {
            _loggingService.Error($"停止轮询服务失败: {ex.Message}");
        }
    }

    private async Task PollingLoop(CancellationToken cancellationToken)
    {
        _loggingService.Debug("轮询循环开始");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var devices = _deviceManager.GetEnabledDevices();
                var now = DateTime.Now;

                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // 检查是否到了该设备的轮询时间
                    if (_nextPollTime.TryGetValue(device.Id, out var nextTime) && now >= nextTime)
                    {
                        try
                        {
                            // 构建并发送请求
                            var requestData = _deviceManager.BuildReadCommand(device);

                            // 触发发送请求事件
                            SendRequest?.Invoke(this, new SendRequestEventArgs
                            {
                                Device = device,
                                RequestData = requestData,
                                RequestTime = now
                            });

                            // 触发状态变化事件
                            StateChanged?.Invoke(this, new PollingStateChangedEventArgs
                            {
                                IsRunning = true,
                                CurrentDevice = device
                            });

                            _loggingService.Debug($"已发送轮询请求: {device.Name} (0x{device.Address:X2})");

                            // 更新下次轮询时间
                            _nextPollTime[device.Id] = now.AddMilliseconds(device.PollingInterval);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Error($"轮询设备失败 [{device.Name}]: {ex.Message}");

                            // 即使失败也要更新下次轮询时间，避免卡在这个设备上
                            _nextPollTime[device.Id] = now.AddMilliseconds(device.PollingInterval);
                        }
                    }
                }

                // 计算下一次需要轮询的最早时间
                var nextPollTime = _nextPollTime.Values.DefaultIfEmpty(now.AddMilliseconds(100)).Min();
                var delayMs = Math.Max(10, (int)(nextPollTime - DateTime.Now).TotalMilliseconds);

                // 等待直到下一次轮询或被取消
                await Task.Delay(Math.Min(delayMs, 100), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，退出循环
                break;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"轮询循环异常: {ex.Message}");
                await Task.Delay(1000, cancellationToken); // 发生错误时等待1秒
            }
        }

        _loggingService.Debug("轮询循环结束");
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
        _pollingTask?.Dispose();
    }
}
