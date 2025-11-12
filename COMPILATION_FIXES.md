# 编译错误修复方案

由于新创建的ViewModel代码与现有服务接口不兼容，有两个选择：

## 方案1：简化版本（快速）
删除有问题的3个ViewModel，只保留实时监控功能
- 删除 HistoryViewModel, AlarmViewModel, DeviceConfigViewModel, SettingsViewModel
- 只保留 MonitorViewModel（需要少量修复）
- 用户可以使用"单设备调试"标签页的现有功能

## 方案2：完整修复（需要时间）
修改所有接口以支持新功能：
1. 扩展 ISerialPortService 添加属性访问
2. 扩展 ISettingsService 添加泛型方法
3. 扩展 IExportService 添加导出方法
4. 修复 AlarmRecord 模型添加缺失属性
5. 修复 ScottPlot API 调用

建议：先用方案1快速编译运行，然后逐步完善功能。
