using System;
using System.Globalization;
using System.Windows.Data;

namespace SerialProtocolAssistant.Converters;

/// <summary>
/// 布尔值反转转换器
/// 用于按钮的IsEnabled绑定（例如：IsMonitoring=true时，开始按钮应禁用）
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}
