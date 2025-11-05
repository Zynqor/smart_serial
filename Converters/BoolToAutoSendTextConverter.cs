using System.Globalization;
using System.Windows.Data;

namespace SerialProtocolAssistant.Converters;

public class BoolToAutoSendTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAutoSending)
        {
            return isAutoSending ? "停止自动发送" : "开始自动发送";
        }
        return "开始自动发送";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
