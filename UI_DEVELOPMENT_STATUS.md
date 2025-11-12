# UI开发状态报告

📅 **更新时间**: 2025-11-12
🎯 **当前阶段**: UI层开发已启动
✅ **完成度**: 主窗口框架 100%, 实时监控页面 0%

---

## ✅ 已完成的工作

### 1. 主窗口重构 (100%)

#### MainWindow.xaml 更新
- ✅ 添加菜单栏（文件、视图、工具、帮助）
- ✅ 添加Tab控件支持6个页面
- ✅ 窗口标题改为 "RS-485多设备监控系统"
- ✅ 调整窗口大小为 1600x900（可调整）
- ✅ ResizeMode 改为 CanResize

#### 菜单功能实现
**文件菜单**:
- ✅ 加载协议配置（OpenFileDialog）
- ✅ 保存协议配置（SaveFileDialog）
- ✅ 导出当前数据（占位符）
- ✅ 退出

**视图菜单**:
- ✅ 刷新
- ✅ 全屏切换

**工具菜单**:
- ✅ 数据库优化（VACUUM）
- ✅ 数据库备份
- ✅ 清理历史数据（30天前）

**帮助菜单**:
- ✅ 关于对话框（显示数据库统计）

#### MainWindow.xaml.cs 更新
- ✅ 注入必要的服务：
  - ISettingsService
  - IDatabaseService
  - IDeviceManagerService
  - IExportService
  - ILoggingService
- ✅ 实现所有菜单点击事件处理
- ✅ Window_Closing 事件保存窗口位置和大小
- ✅ 错误处理和用户反馈（MessageBox）

---

## 📑 Tab页面结构

### Tab 1: 实时监控 ⏳ (占位符)
**功能需求**:
- 左侧图例树形控件（TreeView + CheckBox）
  - 一级节点：设备
  - 二级节点：通道（可勾选）
- 右上区域：ScottPlot 实时曲线图
- 右下区域：数据表格（行=设备，列=通道）

**状态**: 🔴 未开始

---

### Tab 2: 历史数据 ⏳ (占位符)
**功能需求**:
- 顶部查询条件面板
  - 设备选择下拉框
  - 通道选择下拉框
  - 起始/结束日期选择器
  - 查询按钮、导出按钮
- 中间数据表格（分页）
- 底部分页控件

**状态**: 🔴 未开始

---

### Tab 3: 报警管理 ⏳ (占位符)
**功能需求**:
- 左侧（50%）：报警列表
  - 当前告警（红色高亮）
  - 历史告警（可筛选）
- 右侧（50%）：报警统计
  - ScottPlot 柱状图
  - 按设备统计
  - 按类型统计

**状态**: 🔴 未开始

---

### Tab 4: 设备配置 ⏳ (占位符)
**功能需求**:
- 顶部串口设置面板
- 中间协议配置区域
- 底部设备列表（DataGrid）
  - 启用/禁用复选框
  - 设备ID、名称、地址
  - 命令、轮询间隔
  - 编辑/删除按钮

**状态**: 🔴 未开始

---

### Tab 5: 系统设置 ⏳ (占位符)
**功能需求**:
- 数据存储设置
- 报警设置
- 图表设置
- 界面设置

**状态**: 🔴 未开始

---

### Tab 6: 单设备调试 ✅ (旧版界面)
**功能**:
- 保留原有单设备调试界面
- SerialSettingsView
- ProtocolControlView
- DataDisplayView
- LogView

**状态**: ✅ 完成（向下兼容）

---

## 🎨 UI设计规范

### 尺寸标准
```
- 按钮高度: 32px
- 文本框高度: 32px
- 下拉框高度: 32px
- 行高 (DataGrid): 40px
- Tab高度: 40px
- 菜单高度: 32px
```

### 字体
```
- 标题: 16pt, Bold
- 正文: 13pt
- 小字: 11pt
- Tab标签: 14pt
```

### 间距
```
- 内边距 (按钮): 10,0 或 8,6
- 内边距 (容器): 10px
- 外边距: 8px
```

### 颜色方案
```csharp
// 状态
在线: #4CAF50
离线: #F44336
警告: #FF9800
信息: #2196F3

// 报警
活动: #F44336 (粉红背景)
已恢复: #4CAF50 (绿色背景)
```

---

## 📦 依赖的第三方库

### 已添加
- ✅ WPF-UI 3.0.4 (现代化UI组件)
- ✅ ScottPlot.WPF 5.0.42 (图表)
- ✅ ClosedXML 0.102.3 (Excel导出)
- ✅ NAudio 2.2.1 (音频)
- ✅ Microsoft.Data.Sqlite 8.0.0
- ✅ Dapper 2.1.35

---

## 🔜 下一步计划

### 优先级 P0: 实时监控页面（核心功能）

#### 1. 创建MonitorViewModel
- [ ] 设备运行时状态管理（ObservableCollection<DeviceRuntimeInfo>）
- [ ] 图例树形数据绑定
- [ ] ScottPlot 图表数据更新
- [ ] 数据表格数据绑定
- [ ] 串口数据接收事件订阅
- [ ] 轮询服务控制（开始/停止）

#### 2. 创建MonitorView.xaml
- [ ] 左侧图例面板 (250px宽)
  - TreeView 绑定设备和通道
  - CheckBox 控制通道可见性
  - 全选/全不选按钮
- [ ] 右上ScottPlot图表区域
  - 实时曲线绘制
  - 自动缩放
  - 图例显示
- [ ] 右下数据表格
  - 动态列生成
  - 值格式化（小数位）
  - 状态指示器

#### 3. 数据流集成
- [ ] PollingService.SendRequest → SerialPortService
- [ ] SerialPortService.DataReceived → 解析
- [ ] 解析结果 → AlarmService 检测
- [ ] 解析结果 → DatabaseService 保存
- [ ] 解析结果 → MonitorViewModel 更新
- [ ] MonitorViewModel → ScottPlot 刷新
- [ ] MonitorViewModel → DataGrid 刷新

---

### 优先级 P1: 设备配置页面

这个页面对于多设备监控至关重要，需要尽快实现：

- [ ] DeviceConfigViewModel
- [ ] DeviceConfigView.xaml
- [ ] 串口设置集成
- [ ] 协议加载界面
- [ ] 设备列表CRUD

---

### 优先级 P2: 其他页面

- [ ] 历史数据查询页面
- [ ] 报警管理页面
- [ ] 系统设置页面

---

## 📊 工作量估算

| 任务 | 预计时间 | 优先级 |
|------|---------|--------|
| MonitorViewModel | 2-3小时 | P0 |
| MonitorView (XAML) | 2-3小时 | P0 |
| 数据流集成 | 1-2小时 | P0 |
| DeviceConfigViewModel | 1-2小时 | P1 |
| DeviceConfigView | 1-2小时 | P1 |
| 历史数据页面 | 2-3小时 | P2 |
| 报警管理页面 | 2-3小时 | P2 |
| 系统设置页面 | 1-2小时 | P2 |
| **总计** | **12-20小时** | - |

---

## 🎯 技术挑战

### 1. ScottPlot 实时图表
**挑战**:
- 多条曲线同时绘制
- 数据点超过1000时抽样
- 颜色与图例匹配

**解决方案**:
- 使用 `ScottPlot.WpfPlot` 控件
- 每个通道一个 `SignalPlot`
- 定时器100ms刷新（`Dispatcher.Invoke`）
- 数据点限制 + 滚动窗口

---

### 2. 动态表格列生成
**挑战**:
- 设备和通道数量动态变化
- 需要横向展示（行=设备，列=通道）

**解决方案**:
- 代码动态生成 DataGrid 列
- 使用 `DataGridTextColumn` 绑定
- ViewModel 提供 `Dictionary<string, object>` 数据源

---

### 3. 图例树形控件
**挑战**:
- 两级结构（设备→通道）
- CheckBox 状态同步

**解决方案**:
- `TreeView` + `HierarchicalDataTemplate`
- ViewModel 中维护勾选状态
- PropertyChanged 事件更新图表

---

## 📝 代码示例占位

### MonitorViewModel 骨架
```csharp
public partial class MonitorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DeviceRuntimeInfo> _devices = new();

    [ObservableProperty]
    private bool _isMonitoring;

    [RelayCommand]
    private void StartMonitoring() { }

    [RelayCommand]
    private void StopMonitoring() { }

    private void OnDataReceived(object? sender, byte[] data) { }
}
```

---

## ✅ 总结

**当前状态**:
- ✅ 主窗口框架完成
- ✅ 菜单栏功能实现
- ✅ Tab控件结构搭建
- ✅ 向下兼容旧界面
- 🔴 核心监控页面待实现

**下一步**:
1. 实现 MonitorViewModel
2. 实现 MonitorView
3. 集成串口和轮询服务
4. 测试多设备数据流

**预计完成时间**: 2-3个工作日

---

📌 **备注**: 本文档将持续更新，记录UI开发的最新进展。
