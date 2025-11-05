using System.Globalization;
using System.Windows.Data;

namespace SerialProtocolAssistant.Converters;

public class BoolToConnectionTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? "断开连接" : "连接";
        }
        return "连接";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
