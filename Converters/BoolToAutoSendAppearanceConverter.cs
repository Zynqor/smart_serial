using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace SerialProtocolAssistant.Converters;

public class BoolToAutoSendAppearanceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAutoSending)
        {
            return isAutoSending ? ControlAppearance.Danger : ControlAppearance.Success;
        }
        return ControlAppearance.Success;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
