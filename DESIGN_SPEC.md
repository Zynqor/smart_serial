# RS-485多设备监控系统 - 设计规范

## 一、系统概述

这是一个基于 .NET 8.0 + WPF 的工业级RS-485总线多设备监控系统，支持：
- 多设备并发监控
- 实时数据图表显示
- 历史数据查询和导出
- 上下限报警和音效提示
- SQLite数据持久化

---

## 二、UI布局设计

### 主窗口结构
```
┌─────────────────────────────────────────────────────────────┐
│ 菜单栏: [文件] [编辑] [视图] [工具] [帮助]                    │
├─────────────────────────────────────────────────────────────┤
│ Tab栏: [实时监控] [历史数据] [报警管理] [设备配置] [系统设置]│
├─────────────────────────────────────────────────────────────┤
│                                                               │
│                      Tab内容区域                              │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Tab 1: 实时监控页面
```
┌──────────────────────────────────────────────────────────────┐
│ 工具栏: [加载协议] [连接串口] [开始监控] [停止监控] [导出当前] │
├─────────────────┬────────────────────────────────────────────┤
│ 图例面板 (250px)│          主显示区域                        │
│                 │                                            │
│ □ 1号楼电流表   │  ┌─────────────────────────────────────┐  │
│   ├─□ A相电流   │  │                                     │  │
│   ├─□ B相电流   │  │        ScottPlot 实时曲线图          │  │
│   └─□ C相电流   │  │         (支持缩放、平移)             │  │
│                 │  │                                     │  │
│ □ 2号楼电流表   │  └─────────────────────────────────────┘  │
│   ├─□ A相电流   │                                            │
│   ├─□ B相电流   │  ┌─────────────────────────────────────┐  │
│   └─□ C相电流   │  │ 设备名  │ A相  │ B相  │ C相  │ 状态 │  │
│                 │  ├─────────┼──────┼──────┼──────┼──────┤  │
│ □ 温湿度传感器  │  │1号楼    │10.5A │12.3A │11.8A │ ✅  │  │
│   ├─□ 温度      │  │2号楼    │ 8.2A │ 9.1A │ 8.8A │ ✅  │  │
│   └─□ 湿度      │  │温湿度   │25.3℃│60.2% │  -   │ ✅  │  │
│                 │  └─────────────────────────────────────┘  │
│ [刷新] [全选]   │           数据表格 (可排序、筛选)          │
│ [全不选]        │                                            │
└─────────────────┴────────────────────────────────────────────┘
```

### Tab 2: 历史数据页面
```
┌──────────────────────────────────────────────────────────────┐
│ 查询条件面板:                                                 │
│ 设备: [下拉选择▼] 通道: [下拉选择▼]                          │
│ 起始: [DatePicker] 结束: [DatePicker] [查询] [导出Excel]     │
├──────────────────────────────────────────────────────────────┤
│ 查询结果表格:                                                 │
│ ┌────────────┬──────────┬──────────┬──────────┬──────────┐  │
│ │ 时间戳     │ 设备     │ 通道     │ 数值     │ 单位     │  │
│ ├────────────┼──────────┼──────────┼──────────┼──────────┤  │
│ │ 2025-11-12 │ 1号楼    │ A相电流  │ 10.50    │ A        │  │
│ │ 10:23:45   │          │          │          │          │  │
│ └────────────┴──────────┴──────────┴──────────┴──────────┘  │
│                 [分页控件] 1/100页                            │
└──────────────────────────────────────────────────────────────┘
```

### Tab 3: 报警管理页面
```
┌─────────────────────┬────────────────────────────────────────┐
│ 报警列表 (左侧50%)  │  报警统计 (右侧50%)                    │
│                     │                                        │
│ 当前告警 (3条) 🔴  │  ┌──────────────────────────────────┐ │
│ ┌─────────────────┐ │  │                                  │ │
│ │⚠ 1号楼 A相电流  │ │  │     报警统计柱状图 (ScottPlot)    │ │
│ │  超上限 105.2A  │ │  │                                  │ │
│ │  2025-11-12     │ │  │  按设备统计 / 按类型统计          │ │
│ │  10:23:45       │ │  │                                  │ │
│ └─────────────────┘ │  └──────────────────────────────────┘ │
│                     │                                        │
│ 历史告警           │  查询条件:                              │
│ 设备:[全部▼]       │  设备: [下拉▼] 类型: [全部▼]          │
│ 时间:[最近24h▼]    │  时间范围: [DatePicker]                │
│ [查询] [导出]      │  [查询] [导出统计报表]                 │
│                     │                                        │
│ ┌─────────────────┐ │  统计数据:                             │
│ │✅ 2号楼 B相电流 │ │  今日报警: 12 条                       │
│ │  已恢复         │ │  本周报警: 45 条                       │
│ │  持续: 2分钟    │ │  本月报警: 156 条                      │
│ └─────────────────┘ │  最高频率设备: 1号楼电流表              │
└─────────────────────┴────────────────────────────────────────┘
```

### Tab 4: 设备配置页面
```
┌──────────────────────────────────────────────────────────────┐
│ 串口设置:                                                     │
│ 端口:[COM3▼] 波特率:[115200▼] 数据位:[8▼] 校验:[None▼]      │
│ 停止位:[1▼] [连接] [断开]  状态: ✅ 已连接                   │
├──────────────────────────────────────────────────────────────┤
│ 协议配置:                                                     │
│ 当前协议: example_protocol.json  [重新加载] [新建] [编辑]    │
├──────────────────────────────────────────────────────────────┤
│ 设备列表:                                                     │
│ ┌────┬────────┬──────┬──────┬────────┬──────┬──────────┐   │
│ │启用│ 设备ID │ 名称 │ 地址 │ 命令   │间隔ms│ 操作     │   │
│ ├────┼────────┼──────┼──────┼────────┼──────┼──────────┤   │
│ │ ☑ │ DEV001 │1号楼 │ 0x01 │ Read.. │ 1000 │[编辑][删]│   │
│ │ ☑ │ DEV002 │2号楼 │ 0x02 │ Read.. │ 1000 │[编辑][删]│   │
│ │ □ │ DEV003 │温湿度│ 0x03 │ Read.. │ 5000 │[编辑][删]│   │
│ └────┴────────┴──────┴──────┴────────┴──────┴──────────┘   │
│ [添加设备] [批量导入] [保存配置]                              │
└──────────────────────────────────────────────────────────────┘
```

### Tab 5: 系统设置页面
```
┌──────────────────────────────────────────────────────────────┐
│ 数据存储设置:                                                 │
│ □ 启用数据记录到数据库                                        │
│ □ 自动清理历史数据  保留天数: [30] 天                        │
│ 数据库路径: C:\ProgramData\SerialMonitor\data.db             │
│ 数据库大小: 125.6 MB  记录数: 1,234,567 条                   │
│ [清空数据库] [优化数据库] [备份数据库]                        │
├──────────────────────────────────────────────────────────────┤
│ 报警设置:                                                     │
│ □ 启用报警音效提示                                            │
│ 音效文件: [选择文件] alarm.wav  [测试播放]                   │
│ □ 启用邮件报警 (暂未实现)                                     │
├──────────────────────────────────────────────────────────────┤
│ 图表设置:                                                     │
│ 显示点数上限: [1000] 点                                       │
│ 刷新间隔: [100] ms                                            │
│ 默认时间窗口: [60] 秒                                         │
├──────────────────────────────────────────────────────────────┤
│ 界面设置:                                                     │
│ 主题: [○ 浅色主题 ● 深色主题]                                │
│ 字体大小: [中等▼]                                             │
│ □ 记住窗口位置和大小                                          │
│                                                               │
│ [恢复默认设置] [应用] [保存]                                  │
└──────────────────────────────────────────────────────────────┘
```

---

## 三、数据模型设计

### 1. 协议配置文件格式 (JSON)
```json
{
  "protocolName": "8路电流采集系统",
  "version": "2.0",
  "addressConfig": {
    "offset": 0,
    "byteLength": 1,
    "byteOrder": "big"
  },
  "devices": [
    {
      "id": "DEV001",
      "name": "1号楼电流表",
      "description": "主配电箱",
      "address": 1,
      "enabled": true,
      "pollingInterval": 1000,
      "readCommand": {
        "data": "XX0320010008XXXX",
        "hasAddressPlaceholder": true,
        "addressOffset": 0,
        "hasCrcPlaceholder": true,
        "crcType": "modbus"
      },
      "channels": [
        {
          "id": "Ia",
          "name": "A相电流",
          "description": "A相线路电流",
          "offset": 3,
          "byteLength": 2,
          "type": "uint16",
          "byteOrder": "big",
          "multiplier": 0.01,
          "unit": "A",
          "color": "#FF0000",
          "alarmEnabled": true,
          "lowerLimit": 0.5,
          "upperLimit": 100.0
        },
        {
          "id": "Ib",
          "name": "B相电流",
          "description": "B相线路电流",
          "offset": 5,
          "byteLength": 2,
          "type": "uint16",
          "byteOrder": "big",
          "multiplier": 0.01,
          "unit": "A",
          "color": "#00FF00",
          "alarmEnabled": true,
          "lowerLimit": 0.5,
          "upperLimit": 100.0
        }
      ]
    }
  ]
}
```

### 2. C# 数据模型类

#### Protocol.cs
```csharp
public class Protocol
{
    public string ProtocolName { get; set; }
    public string Version { get; set; }
    public AddressConfig AddressConfig { get; set; }
    public List<DeviceDefinition> Devices { get; set; }
}

public class AddressConfig
{
    public int Offset { get; set; } = 0;
    public int ByteLength { get; set; } = 1;
    public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;
}
```

#### DeviceDefinition.cs
```csharp
public class DeviceDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public byte Address { get; set; }
    public bool Enabled { get; set; }
    public int PollingInterval { get; set; }
    public ReadCommand ReadCommand { get; set; }
    public List<ChannelDefinition> Channels { get; set; }
}

public class ReadCommand
{
    public string Data { get; set; }
    public bool HasAddressPlaceholder { get; set; }
    public int AddressOffset { get; set; }
    public bool HasCrcPlaceholder { get; set; }
    public string CrcType { get; set; }
}
```

#### ChannelDefinition.cs
```csharp
public class ChannelDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Offset { get; set; }
    public int ByteLength { get; set; }
    public DataType Type { get; set; }
    public ByteOrder? ByteOrder { get; set; }
    public double Multiplier { get; set; } = 1.0;
    public string Unit { get; set; }
    public string Color { get; set; }
    public bool AlarmEnabled { get; set; }
    public double LowerLimit { get; set; }
    public double UpperLimit { get; set; }
}
```

### 3. 数据库表结构 (SQLite)

#### DeviceData (设备数据表)
```sql
CREATE TABLE DeviceData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Timestamp DATETIME NOT NULL,
    DeviceId TEXT NOT NULL,
    DeviceName TEXT NOT NULL,
    ChannelId TEXT NOT NULL,
    ChannelName TEXT NOT NULL,
    Value REAL NOT NULL,
    Unit TEXT,
    INDEX idx_device_time (DeviceId, Timestamp),
    INDEX idx_timestamp (Timestamp)
);
```

#### AlarmRecords (报警记录表)
```sql
CREATE TABLE AlarmRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    DeviceId TEXT NOT NULL,
    DeviceName TEXT NOT NULL,
    ChannelId TEXT NOT NULL,
    ChannelName TEXT NOT NULL,
    AlarmType TEXT NOT NULL, -- 'UpperLimit' | 'LowerLimit'
    TriggerValue REAL NOT NULL,
    LimitValue REAL NOT NULL,
    Status TEXT NOT NULL, -- 'Active' | 'Acknowledged' | 'Resolved'
    Duration INTEGER, -- 持续秒数
    INDEX idx_device (DeviceId),
    INDEX idx_time (StartTime),
    INDEX idx_status (Status)
);
```

#### AppSettings (应用设置表)
```sql
CREATE TABLE AppSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);
```

---

## 四、服务层设计

### IDeviceManagerService
```csharp
public interface IDeviceManagerService
{
    Protocol CurrentProtocol { get; }
    void LoadProtocol(string filePath);
    List<DeviceDefinition> GetEnabledDevices();
    DeviceDefinition GetDeviceByAddress(byte address);
    byte ExtractAddress(byte[] data);
    byte[] BuildRequest(DeviceDefinition device);
}
```

### IPollingService
```csharp
public interface IPollingService
{
    bool IsRunning { get; }
    void Start();
    void Stop();
    event EventHandler<(DeviceDefinition device, byte[] request)> SendRequest;
}
```

### IDatabaseService
```csharp
public interface IDatabaseService
{
    void Initialize();
    void SaveDeviceData(string deviceId, string deviceName,
                       string channelId, string channelName,
                       double value, string unit, DateTime timestamp);
    List<DeviceDataRecord> QueryData(string deviceId, string channelId,
                                     DateTime startTime, DateTime endTime);
    void SaveAlarmRecord(AlarmRecord alarm);
    List<AlarmRecord> QueryAlarms(string deviceId, DateTime startTime,
                                  DateTime endTime, string status);
    Dictionary<string, int> GetAlarmStatistics(DateTime startTime, DateTime endTime);
}
```

### IAlarmService
```csharp
public interface IAlarmService
{
    event EventHandler<AlarmRecord> AlarmTriggered;
    event EventHandler<AlarmRecord> AlarmResolved;
    void CheckAlarm(DeviceDefinition device, ChannelDefinition channel, double value);
    List<AlarmRecord> GetActiveAlarms();
}
```

### IAudioService
```csharp
public interface IAudioService
{
    void PlayAlarmSound();
    void SetAlarmSoundPath(string path);
    bool IsEnabled { get; set; }
}
```

### IExportService
```csharp
public interface IExportService
{
    void ExportToExcel(List<DeviceDataRecord> data, string filePath);
    void ExportAlarmsToExcel(List<AlarmRecord> alarms, string filePath);
}
```

### ISettingsService
```csharp
public interface ISettingsService
{
    T GetSetting<T>(string key, T defaultValue);
    void SetSetting<T>(string key, T value);
    void SaveWindowBounds(double left, double top, double width, double height);
    (double left, double top, double width, double height) LoadWindowBounds();
}
```

---

## 五、UI样式规范

### 尺寸标准
- **按钮高度**: 32px
- **文本框高度**: 32px
- **下拉框高度**: 32px
- **行高**: 40px (DataGrid)
- **字体大小**:
  - 标题: 16pt
  - 正文: 13pt
  - 小字: 11pt
- **内边距**:
  - 按钮: 8,6
  - 文本框: 8,6
  - 容器: 10
- **外边距**: 8px (元素间距)

### 颜色方案
```csharp
// 状态颜色
OnlineColor = "#4CAF50"      // 绿色
OfflineColor = "#F44336"     // 红色
WarningColor = "#FF9800"     // 橙色
InfoColor = "#2196F3"        // 蓝色

// 报警颜色
AlarmActiveColor = "#F44336"     // 活动报警
AlarmResolvedColor = "#4CAF50"   // 已恢复

// 背景色
BackgroundColor = "#FFFFFF"
AlternateRowColor = "#F5F5F5"
HeaderBackground = "#E3F2FD"
```

### WPF-UI 组件使用
- 使用 `<ui:Button>` 替代标准按钮
- 使用 `<ui:TextBox>` 带验证
- 使用 `<ui:Card>` 作为容器
- 使用 `<ui:InfoBar>` 显示提示信息

---

## 六、数据流程

### 监控流程
```
1. 用户启动监控
   ↓
2. PollingService 定时器启动
   ↓
3. 遍历启用的设备列表
   ↓
4. DeviceManagerService.BuildRequest() 构建命令
   - 替换地址占位符
   - 计算CRC校验
   ↓
5. SerialPortService.SendData() 发送
   ↓
6. 等待响应 (100-200ms)
   ↓
7. SerialPortService.DataReceived 事件触发
   ↓
8. DeviceManagerService.ExtractAddress() 提取地址
   ↓
9. 匹配设备和命令
   ↓
10. FrameParserService.ParseFrame() 解析字段
   ↓
11. AlarmService.CheckAlarm() 检查报警
   ↓
12. DatabaseService.SaveDeviceData() 存储
   ↓
13. MonitorViewModel 更新UI
   - 更新表格
   - 更新图表
   ↓
14. 延迟后发送下一个设备命令
```

### 报警流程
```
1. AlarmService.CheckAlarm()
   ↓
2. 判断是否超限
   ↓
3. 如果超限:
   - 创建 AlarmRecord (Status=Active)
   - 触发 AlarmTriggered 事件
   - DatabaseService.SaveAlarmRecord()
   ↓
4. AudioService.PlayAlarmSound() 播放音效
   ↓
5. AlarmViewModel 更新当前告警列表
   ↓
6. 如果恢复:
   - 更新 AlarmRecord (Status=Resolved, EndTime)
   - 触发 AlarmResolved 事件
   - 计算持续时间
```

---

## 七、第三方库依赖

### 新增依赖包
```xml
<!-- 图表库 -->
<PackageReference Include="ScottPlot.WPF" Version="5.0.x" />

<!-- 数据库 -->
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.x" />
<PackageReference Include="Dapper" Version="2.1.x" />

<!-- Excel导出 -->
<PackageReference Include="ClosedXML" Version="0.102.x" />

<!-- 音频播放 -->
<PackageReference Include="NAudio" Version="2.2.x" />
```

---

## 八、配置文件示例

### appsettings.json
```json
{
  "Database": {
    "Path": "Data/monitoring.db",
    "AutoCleanup": true,
    "RetentionDays": 30
  },
  "Alarm": {
    "EnableSound": true,
    "SoundPath": "Assets/alarm.wav",
    "EnableEmail": false
  },
  "Chart": {
    "MaxDataPoints": 1000,
    "RefreshInterval": 100,
    "DefaultTimeWindow": 60
  },
  "UI": {
    "Theme": "Light",
    "RememberWindowPosition": true
  }
}
```

---

## 九、实施计划

### 阶段1: 基础架构 (2-3天)
- [x] 扩展数据模型
- [ ] 实现 DeviceManagerService
- [ ] 实现 DatabaseService
- [ ] 实现 SettingsService
- [ ] 升级协议文件格式

### 阶段2: 核心功能 (2-3天)
- [ ] 实现 PollingService
- [ ] 实现 AlarmService
- [ ] 实现 AudioService
- [ ] CRC16/Modbus 校验计算
- [ ] 地址占位符替换逻辑

### 阶段3: UI开发 (3-4天)
- [ ] 主窗口和菜单栏
- [ ] 实时监控页面 (表格+图表+图例)
- [ ] 历史数据查询页面
- [ ] 报警管理页面
- [ ] 设备配置页面
- [ ] 系统设置页面

### 阶段4: 导出和优化 (1-2天)
- [ ] 实现 ExportService
- [ ] Excel 导出功能
- [ ] 数据库优化
- [ ] UI性能优化
- [ ] 内存管理优化

### 阶段5: 测试和完善 (1-2天)
- [ ] 单元测试
- [ ] 集成测试
- [ ] 多设备并发测试
- [ ] 长时间稳定性测试
- [ ] 文档编写

---

## 十、注意事项

### 性能优化
1. **图表数据点限制**: 超过1000点自动抽样
2. **数据库批量写入**: 累积100条或1秒后批量提交
3. **UI线程优化**: 使用 `Dispatcher.InvokeAsync` 避免阻塞
4. **内存管理**: 及时清理图表历史数据

### 异常处理
1. **串口异常**: 自动重连机制
2. **数据解析失败**: 记录日志但不中断
3. **数据库异常**: 降级到内存模式
4. **CRC校验失败**: 丢弃数据并记录

### 用户体验
1. **加载提示**: 长时间操作显示进度条
2. **错误提示**: 使用 InfoBar 友好提示
3. **快捷键**: Ctrl+S保存, Ctrl+E导出, F5刷新
4. **状态栏**: 显示连接状态、数据速率、错误计数

---

## 十一、扩展功能 (未来版本)

- [ ] 支持TCP/IP转串口
- [ ] 支持Modbus TCP协议
- [ ] 邮件报警通知
- [ ] 微信/钉钉报警推送
- [ ] 数据分析和预测
- [ ] 多语言支持
- [ ] 用户权限管理
- [ ] 远程监控Web界面
