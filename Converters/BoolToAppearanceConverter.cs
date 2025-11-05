using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace SerialProtocolAssistant.Converters;

public class BoolToAppearanceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? ControlAppearance.Danger : ControlAppearance.Primary;
        }
        return ControlAppearance.Primary;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
