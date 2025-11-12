using ClosedXML.Excel;
using SerialProtocolAssistant.Models;
using System.Text;

namespace SerialProtocolAssistant.Services;

/// <summary>
/// 导出服务实现
/// </summary>
public class ExportService : IExportService
{
    private readonly ILoggingService _loggingService;

    public ExportService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void ExportDeviceDataToExcel(List<DeviceDataRecord> data, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("设备数据");

            // 设置表头
            worksheet.Cell(1, 1).Value = "时间戳";
            worksheet.Cell(1, 2).Value = "设备ID";
            worksheet.Cell(1, 3).Value = "设备名称";
            worksheet.Cell(1, 4).Value = "通道ID";
            worksheet.Cell(1, 5).Value = "通道名称";
            worksheet.Cell(1, 6).Value = "数值";
            worksheet.Cell(1, 7).Value = "单位";

            // 设置表头样式
            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 填充数据
            int row = 2;
            foreach (var record in data)
            {
                worksheet.Cell(row, 1).Value = record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 2).Value = record.DeviceId;
                worksheet.Cell(row, 3).Value = record.DeviceName;
                worksheet.Cell(row, 4).Value = record.ChannelId;
                worksheet.Cell(row, 5).Value = record.ChannelName;
                worksheet.Cell(row, 6).Value = record.Value;
                worksheet.Cell(row, 7).Value = record.Unit ?? "";
                row++;
            }

            // 自动调整列宽
            worksheet.Columns().AdjustToContents();

            // 保存文件
            workbook.SaveAs(filePath);

            _loggingService.Information($"已导出{data.Count}条设备数据到: {filePath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出设备数据到Excel失败: {ex.Message}");
            throw;
        }
    }

    public void ExportAlarmsToExcel(List<AlarmRecord> alarms, string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("报警记录");

            // 设置表头
            worksheet.Cell(1, 1).Value = "开始时间";
            worksheet.Cell(1, 2).Value = "结束时间";
            worksheet.Cell(1, 3).Value = "持续时间(秒)";
            worksheet.Cell(1, 4).Value = "设备ID";
            worksheet.Cell(1, 5).Value = "设备名称";
            worksheet.Cell(1, 6).Value = "通道ID";
            worksheet.Cell(1, 7).Value = "通道名称";
            worksheet.Cell(1, 8).Value = "报警类型";
            worksheet.Cell(1, 9).Value = "触发值";
            worksheet.Cell(1, 10).Value = "限值";
            worksheet.Cell(1, 11).Value = "单位";
            worksheet.Cell(1, 12).Value = "状态";
            worksheet.Cell(1, 13).Value = "备注";

            // 设置表头样式
            var headerRange = worksheet.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 填充数据
            int row = 2;
            foreach (var alarm in alarms)
            {
                worksheet.Cell(row, 1).Value = alarm.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 2).Value = alarm.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                worksheet.Cell(row, 3).Value = alarm.Duration?.ToString() ?? "-";
                worksheet.Cell(row, 4).Value = alarm.DeviceId;
                worksheet.Cell(row, 5).Value = alarm.DeviceName;
                worksheet.Cell(row, 6).Value = alarm.ChannelId;
                worksheet.Cell(row, 7).Value = alarm.ChannelName;
                worksheet.Cell(row, 8).Value = alarm.AlarmType == AlarmType.UpperLimit ? "超上限" : "低于下限";
                worksheet.Cell(row, 9).Value = alarm.TriggerValue;
                worksheet.Cell(row, 10).Value = alarm.LimitValue;
                worksheet.Cell(row, 11).Value = alarm.Unit ?? "";
                worksheet.Cell(row, 12).Value = alarm.Status switch
                {
                    AlarmStatus.Active => "活动中",
                    AlarmStatus.Acknowledged => "已确认",
                    AlarmStatus.Resolved => "已恢复",
                    _ => "未知"
                };
                worksheet.Cell(row, 13).Value = alarm.Notes ?? "";

                // 根据状态设置行颜色
                var rowRange = worksheet.Range(row, 1, row, 13);
                if (alarm.Status == AlarmStatus.Active)
                {
                    rowRange.Style.Fill.BackgroundColor = XLColor.LightPink;
                }
                else if (alarm.Status == AlarmStatus.Resolved)
                {
                    rowRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                }

                row++;
            }

            // 自动调整列宽
            worksheet.Columns().AdjustToContents();

            // 保存文件
            workbook.SaveAs(filePath);

            _loggingService.Information($"已导出{alarms.Count}条报警记录到: {filePath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出报警记录到Excel失败: {ex.Message}");
            throw;
        }
    }

    public void ExportDeviceDataToCsv(List<DeviceDataRecord> data, string filePath)
    {
        try
        {
            var csv = new StringBuilder();

            // 添加表头
            csv.AppendLine("时间戳,设备ID,设备名称,通道ID,通道名称,数值,单位");

            // 添加数据行
            foreach (var record in data)
            {
                csv.AppendLine($"{record.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                              $"{record.DeviceId}," +
                              $"{record.DeviceName}," +
                              $"{record.ChannelId}," +
                              $"{record.ChannelName}," +
                              $"{record.Value}," +
                              $"{record.Unit ?? ""}");
            }

            // 保存文件（使用UTF-8 with BOM以确保Excel正确显示中文）
            File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));

            _loggingService.Information($"已导出{data.Count}条设备数据到CSV: {filePath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出设备数据到CSV失败: {ex.Message}");
            throw;
        }
    }

    public void ExportAlarmsToCsv(List<AlarmRecord> alarms, string filePath)
    {
        try
        {
            var csv = new StringBuilder();

            // 添加表头
            csv.AppendLine("开始时间,结束时间,持续时间(秒),设备ID,设备名称,通道ID,通道名称,报警类型,触发值,限值,单位,状态,备注");

            // 添加数据行
            foreach (var alarm in alarms)
            {
                csv.AppendLine($"{alarm.StartTime:yyyy-MM-dd HH:mm:ss}," +
                              $"{alarm.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"}," +
                              $"{alarm.Duration?.ToString() ?? "-"}," +
                              $"{alarm.DeviceId}," +
                              $"{alarm.DeviceName}," +
                              $"{alarm.ChannelId}," +
                              $"{alarm.ChannelName}," +
                              $"{(alarm.AlarmType == AlarmType.UpperLimit ? "超上限" : "低于下限")}," +
                              $"{alarm.TriggerValue}," +
                              $"{alarm.LimitValue}," +
                              $"{alarm.Unit ?? ""}," +
                              $"{alarm.Status switch { AlarmStatus.Active => "活动中", AlarmStatus.Acknowledged => "已确认", AlarmStatus.Resolved => "已恢复", _ => "未知" }}," +
                              $"{alarm.Notes ?? ""}");
            }

            // 保存文件（使用UTF-8 with BOM以确保Excel正确显示中文）
            File.WriteAllText(filePath, csv.ToString(), new UTF8Encoding(true));

            _loggingService.Information($"已导出{alarms.Count}条报警记录到CSV: {filePath}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"导出报警记录到CSV失败: {ex.Message}");
            throw;
        }
    }
}
