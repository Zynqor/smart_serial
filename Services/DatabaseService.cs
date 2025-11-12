using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// SQLite数据库服务实现
/// </summary>
public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly string _connectionString;
    private readonly ILoggingService _loggingService;
    private readonly object _lock = new();
    private readonly List<DeviceDataRecord> _batchBuffer = new();
    private readonly Timer _batchTimer;
    private const int BatchSize = 100;
    private const int BatchIntervalMs = 1000;

    public DatabaseService(ILoggingService loggingService, string databasePath = "Data/monitoring.db")
    {
        _loggingService = loggingService;

        // 确保数据目录存在
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={databasePath}";
        _loggingService.Information($"数据库路径: {Path.GetFullPath(databasePath)}");

        // 创建批量写入定时器
        _batchTimer = new Timer(FlushBatchData, null, BatchIntervalMs, BatchIntervalMs);
    }

    public void Initialize()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // 创建设备数据表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS DeviceData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME NOT NULL,
                    DeviceId TEXT NOT NULL,
                    DeviceName TEXT NOT NULL,
                    ChannelId TEXT NOT NULL,
                    ChannelName TEXT NOT NULL,
                    Value REAL NOT NULL,
                    Unit TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_device_time ON DeviceData(DeviceId, Timestamp);
                CREATE INDEX IF NOT EXISTS idx_timestamp ON DeviceData(Timestamp);
                CREATE INDEX IF NOT EXISTS idx_device_channel ON DeviceData(DeviceId, ChannelId);
            ");

            // 创建报警记录表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS AlarmRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StartTime DATETIME NOT NULL,
                    EndTime DATETIME,
                    DeviceId TEXT NOT NULL,
                    DeviceName TEXT NOT NULL,
                    ChannelId TEXT NOT NULL,
                    ChannelName TEXT NOT NULL,
                    AlarmType INTEGER NOT NULL,
                    TriggerValue REAL NOT NULL,
                    LimitValue REAL NOT NULL,
                    Status INTEGER NOT NULL,
                    Duration INTEGER,
                    Unit TEXT,
                    Notes TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_alarm_device ON AlarmRecords(DeviceId);
                CREATE INDEX IF NOT EXISTS idx_alarm_time ON AlarmRecords(StartTime);
                CREATE INDEX IF NOT EXISTS idx_alarm_status ON AlarmRecords(Status);
            ");

            // 创建应用设置表
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS AppSettings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );
            ");

            _loggingService.Information("数据库初始化成功");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"数据库初始化失败: {ex.Message}");
            throw;
        }
    }

    public void SaveDeviceData(string deviceId, string deviceName, string channelId, string channelName,
                               double value, string? unit, DateTime timestamp)
    {
        var record = new DeviceDataRecord
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            ChannelId = channelId,
            ChannelName = channelName,
            Value = value,
            Unit = unit,
            Timestamp = timestamp
        };

        lock (_lock)
        {
            _batchBuffer.Add(record);

            // 如果达到批量大小，立即写入
            if (_batchBuffer.Count >= BatchSize)
            {
                FlushBatchData(null);
            }
        }
    }

    public void SaveDeviceDataBatch(List<DeviceDataRecord> records)
    {
        if (records == null || records.Count == 0) return;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            connection.Execute(@"
                INSERT INTO DeviceData (Timestamp, DeviceId, DeviceName, ChannelId, ChannelName, Value, Unit)
                VALUES (@Timestamp, @DeviceId, @DeviceName, @ChannelId, @ChannelName, @Value, @Unit)",
                records, transaction);

            transaction.Commit();

            _loggingService.Debug($"批量保存{records.Count}条数据记录");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"批量保存数据失败: {ex.Message}");
        }
    }

    private void FlushBatchData(object? state)
    {
        List<DeviceDataRecord> recordsToSave;

        lock (_lock)
        {
            if (_batchBuffer.Count == 0) return;

            recordsToSave = new List<DeviceDataRecord>(_batchBuffer);
            _batchBuffer.Clear();
        }

        SaveDeviceDataBatch(recordsToSave);
    }

    public List<DeviceDataRecord> QueryData(string? deviceId, string? channelId,
                                           DateTime startTime, DateTime endTime, int limit = 1000)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT Id, Timestamp, DeviceId, DeviceName, ChannelId, ChannelName, Value, Unit
                FROM DeviceData
                WHERE Timestamp >= @StartTime AND Timestamp <= @EndTime";

            if (!string.IsNullOrEmpty(deviceId))
                sql += " AND DeviceId = @DeviceId";

            if (!string.IsNullOrEmpty(channelId))
                sql += " AND ChannelId = @ChannelId";

            sql += " ORDER BY Timestamp DESC";

            if (limit > 0)
                sql += $" LIMIT {limit}";

            var results = connection.Query<DeviceDataRecord>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                DeviceId = deviceId,
                ChannelId = channelId
            }).ToList();

            _loggingService.Debug($"查询到{results.Count}条数据记录");
            return results;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"查询数据失败: {ex.Message}");
            return new List<DeviceDataRecord>();
        }
    }

    public void SaveAlarmRecord(AlarmRecord alarm)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"
                INSERT INTO AlarmRecords (StartTime, EndTime, DeviceId, DeviceName, ChannelId, ChannelName,
                                         AlarmType, TriggerValue, LimitValue, Status, Duration, Unit, Notes)
                VALUES (@StartTime, @EndTime, @DeviceId, @DeviceName, @ChannelId, @ChannelName,
                       @AlarmType, @TriggerValue, @LimitValue, @Status, @Duration, @Unit, @Notes);
                SELECT last_insert_rowid();";

            alarm.Id = connection.ExecuteScalar<long>(sql, new
            {
                alarm.StartTime,
                alarm.EndTime,
                alarm.DeviceId,
                alarm.DeviceName,
                alarm.ChannelId,
                alarm.ChannelName,
                AlarmType = (int)alarm.AlarmType,
                alarm.TriggerValue,
                alarm.LimitValue,
                Status = (int)alarm.Status,
                alarm.Duration,
                alarm.Unit,
                alarm.Notes
            });

            _loggingService.Debug($"保存报警记录: {alarm.DeviceName} - {alarm.ChannelName}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"保存报警记录失败: {ex.Message}");
        }
    }

    public void UpdateAlarmRecord(AlarmRecord alarm)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute(@"
                UPDATE AlarmRecords
                SET EndTime = @EndTime,
                    Status = @Status,
                    Duration = @Duration,
                    Notes = @Notes
                WHERE Id = @Id",
                new
                {
                    alarm.Id,
                    alarm.EndTime,
                    Status = (int)alarm.Status,
                    alarm.Duration,
                    alarm.Notes
                });

            _loggingService.Debug($"更新报警记录: ID={alarm.Id}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"更新报警记录失败: {ex.Message}");
        }
    }

    public List<AlarmRecord> QueryAlarms(string? deviceId, AlarmStatus? status,
                                        DateTime startTime, DateTime endTime)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT Id, StartTime, EndTime, DeviceId, DeviceName, ChannelId, ChannelName,
                       AlarmType, TriggerValue, LimitValue, Status, Duration, Unit, Notes
                FROM AlarmRecords
                WHERE StartTime >= @StartTime AND StartTime <= @EndTime";

            if (!string.IsNullOrEmpty(deviceId))
                sql += " AND DeviceId = @DeviceId";

            if (status.HasValue)
                sql += " AND Status = @Status";

            sql += " ORDER BY StartTime DESC";

            var results = connection.Query<AlarmRecordDto>(sql, new
            {
                StartTime = startTime,
                EndTime = endTime,
                DeviceId = deviceId,
                Status = status.HasValue ? (int)status.Value : (int?)null
            }).Select(dto => dto.ToAlarmRecord()).ToList();

            _loggingService.Debug($"查询到{results.Count}条报警记录");
            return results;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"查询报警记录失败: {ex.Message}");
            return new List<AlarmRecord>();
        }
    }

    public List<AlarmRecord> GetActiveAlarms()
    {
        return QueryAlarms(null, AlarmStatus.Active, DateTime.Now.AddYears(-1), DateTime.Now);
    }

    public Dictionary<string, int> GetAlarmStatistics(DateTime startTime, DateTime endTime)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var results = connection.Query<(string DeviceId, int Count)>(@"
                SELECT DeviceId, COUNT(*) as Count
                FROM AlarmRecords
                WHERE StartTime >= @StartTime AND StartTime <= @EndTime
                GROUP BY DeviceId",
                new { StartTime = startTime, EndTime = endTime });

            return results.ToDictionary(r => r.DeviceId, r => r.Count);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"获取报警统计失败: {ex.Message}");
            return new Dictionary<string, int>();
        }
    }

    public Dictionary<AlarmType, int> GetAlarmStatisticsByType(DateTime startTime, DateTime endTime)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var results = connection.Query<(int AlarmType, int Count)>(@"
                SELECT AlarmType, COUNT(*) as Count
                FROM AlarmRecords
                WHERE StartTime >= @StartTime AND StartTime <= @EndTime
                GROUP BY AlarmType",
                new { StartTime = startTime, EndTime = endTime });

            return results.ToDictionary(r => (AlarmType)r.AlarmType, r => r.Count);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"获取报警类型统计失败: {ex.Message}");
            return new Dictionary<AlarmType, int>();
        }
    }

    public int CleanupOldData(DateTime olderThan)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var deletedRecords = connection.Execute(
                "DELETE FROM DeviceData WHERE Timestamp < @OlderThan",
                new { OlderThan = olderThan });

            _loggingService.Information($"清理了{deletedRecords}条历史数据");
            return deletedRecords;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"清理历史数据失败: {ex.Message}");
            return 0;
        }
    }

    public DatabaseStatistics GetStatistics()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var stats = new DatabaseStatistics();

            // 统计数据记录数
            stats.TotalDataRecords = connection.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM DeviceData");

            // 统计报警记录数
            stats.TotalAlarmRecords = connection.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM AlarmRecords");

            // 获取最早和最新的数据时间戳
            stats.OldestDataTimestamp = connection.ExecuteScalar<DateTime?>(
                "SELECT MIN(Timestamp) FROM DeviceData");

            stats.NewestDataTimestamp = connection.ExecuteScalar<DateTime?>(
                "SELECT MAX(Timestamp) FROM DeviceData");

            // 获取数据库文件大小
            var dbPath = connection.DataSource;
            if (File.Exists(dbPath))
            {
                stats.DatabaseSizeBytes = new FileInfo(dbPath).Length;
            }

            return stats;
        }
        catch (Exception ex)
        {
            _loggingService.Error($"获取数据库统计信息失败: {ex.Message}");
            return new DatabaseStatistics();
        }
    }

    public void OptimizeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute("VACUUM");
            connection.Execute("ANALYZE");

            _loggingService.Information("数据库优化完成");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"数据库优化失败: {ex.Message}");
        }
    }

    public void BackupDatabase(string backupPath)
    {
        try
        {
            // 先刷新批量缓存
            FlushBatchData(null);

            using var source = new SqliteConnection(_connectionString);
            source.Open();

            var directory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var destination = new SqliteConnection($"Data Source={backupPath}");
            destination.Open();
            source.BackupDatabase(destination);

            _loggingService.Information($"数据库备份成功: {backupPath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"数据库备份失败: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        // 刷新剩余的批量数据
        FlushBatchData(null);

        // 停止定时器
        _batchTimer?.Dispose();
    }

    /// <summary>
    /// DTO类用于从数据库读取报警记录
    /// </summary>
    private class AlarmRecordDto
    {
        public long Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string ChannelId { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public int AlarmType { get; set; }
        public double TriggerValue { get; set; }
        public double LimitValue { get; set; }
        public int Status { get; set; }
        public int? Duration { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }

        public AlarmRecord ToAlarmRecord()
        {
            return new AlarmRecord
            {
                Id = Id,
                StartTime = StartTime,
                EndTime = EndTime,
                DeviceId = DeviceId,
                DeviceName = DeviceName,
                ChannelId = ChannelId,
                ChannelName = ChannelName,
                AlarmType = (Models.AlarmType)AlarmType,
                TriggerValue = TriggerValue,
                LimitValue = LimitValue,
                Status = (AlarmStatus)Status,
                Duration = Duration,
                Unit = Unit,
                Notes = Notes
            };
        }
    }
}
