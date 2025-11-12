using SerialProtocolAssistant.Models;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 导出服务接口
/// </summary>
public interface IExportService
{
    /// <summary>
    /// 导出设备数据到Excel
    /// </summary>
    /// <param name="data">数据列表</param>
    /// <param name="filePath">输出文件路径</param>
    void ExportDeviceDataToExcel(List<DeviceDataRecord> data, string filePath);

    /// <summary>
    /// 导出报警记录到Excel
    /// </summary>
    /// <param name="alarms">报警列表</param>
    /// <param name="filePath">输出文件路径</param>
    void ExportAlarmsToExcel(List<AlarmRecord> alarms, string filePath);

    /// <summary>
    /// 导出设备数据到CSV
    /// </summary>
    void ExportDeviceDataToCsv(List<DeviceDataRecord> data, string filePath);

    /// <summary>
    /// 导出报警记录到CSV
    /// </summary>
    void ExportAlarmsToCsv(List<AlarmRecord> alarms, string filePath);
}
