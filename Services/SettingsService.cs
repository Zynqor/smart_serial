using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.IO;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 设置服务实现（使用SQLite存储）
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _connectionString;
    private readonly ILoggingService _loggingService;

    public SettingsService(ILoggingService loggingService, string databasePath = "Data/monitoring.db")
    {
        _loggingService = loggingService;

        // 确保数据目录存在
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={databasePath}";
    }

    public T GetSetting<T>(string key, T defaultValue)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var valueStr = connection.ExecuteScalar<string>(
                "SELECT Value FROM AppSettings WHERE Key = @Key",
                new { Key = key });

            if (string.IsNullOrEmpty(valueStr))
            {
                return defaultValue;
            }

            // 尝试反序列化
            if (typeof(T) == typeof(string))
            {
                return (T)(object)valueStr;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(valueStr) ?? defaultValue;
            }
        }
        catch (Exception ex)
        {
            _loggingService.Warning($"获取设置失败 [{key}]: {ex.Message}");
            return defaultValue;
        }
    }

    public void SetSetting<T>(string key, T value)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // 序列化值
            string valueStr;
            if (value is string strValue)
            {
                valueStr = strValue;
            }
            else
            {
                valueStr = JsonConvert.SerializeObject(value);
            }

            // 使用UPSERT操作（INSERT OR REPLACE）
            connection.Execute(@"
                INSERT INTO AppSettings (Key, Value)
                VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = @Value",
                new { Key = key, Value = valueStr });

            _loggingService.Debug($"已保存设置: {key}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存设置失败 [{key}]: {ex.Message}");
        }
    }

    public void SaveWindowBounds(double left, double top, double width, double height)
    {
        try
        {
            SetSetting("Window.Left", left);
            SetSetting("Window.Top", top);
            SetSetting("Window.Width", width);
            SetSetting("Window.Height", height);

            _loggingService.Debug($"已保存窗口大小: {width}x{height} at ({left},{top})");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存窗口大小失败: {ex.Message}");
        }
    }

    public (double left, double top, double width, double height)? LoadWindowBounds()
    {
        try
        {
            var left = GetSetting("Window.Left", -1.0);
            var top = GetSetting("Window.Top", -1.0);
            var width = GetSetting("Window.Width", -1.0);
            var height = GetSetting("Window.Height", -1.0);

            // 如果任何值无效，返回null
            if (left < 0 || top < 0 || width < 0 || height < 0)
            {
                return null;
            }

            _loggingService.Debug($"已加载窗口大小: {width}x{height} at ({left},{top})");
            return (left, top, width, height);
        }
        catch (Exception ex)
        {
            _loggingService.Warning($"加载窗口大小失败: {ex.Message}");
            return null;
        }
    }

    public void RemoveSetting(string key)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute("DELETE FROM AppSettings WHERE Key = @Key", new { Key = key });

            _loggingService.Debug($"已删除设置: {key}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"删除设置失败 [{key}]: {ex.Message}");
        }
    }

    public void ClearAllSettings()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute("DELETE FROM AppSettings");

            _loggingService.Information("已清除所有设置");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"清除所有设置失败: {ex.Message}");
        }
    }

    public bool HasSetting(string key)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var count = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM AppSettings WHERE Key = @Key",
                new { Key = key });

            return count > 0;
        }
        catch (Exception ex)
        {
            _loggingService.Warning($"检查设置是否存在失败 [{key}]: {ex.Message}");
            return false;
        }
    }
}
