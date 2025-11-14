using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace SerialProtocolAssistant.Views;

public partial class MonitorView : UserControl
{
    public MonitorView()
    {
        InitializeComponent();
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        // 全选所有通道
        // 使用动态方式访问 DataContext，避免强类型 ViewModel 依赖
        if (DataContext != null)
        {
            var devices = GetPropertyValue(DataContext, "Devices") as IEnumerable;
            if (devices != null)
            {
                foreach (var device in devices)
                {
                    var channelData = GetPropertyValue(device, "ChannelData") as IEnumerable;
                    if (channelData != null)
                    {
                        foreach (var channel in channelData)
                        {
                            SetPropertyValue(channel, "IsVisible", true);
                        }
                    }
                }
            }
        }
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        // 取消选择所有通道
        // 使用动态方式访问 DataContext，避免强类型 ViewModel 依赖
        if (DataContext != null)
        {
            var devices = GetPropertyValue(DataContext, "Devices") as IEnumerable;
            if (devices != null)
            {
                foreach (var device in devices)
                {
                    var channelData = GetPropertyValue(device, "ChannelData") as IEnumerable;
                    if (channelData != null)
                    {
                        foreach (var channel in channelData)
                        {
                            SetPropertyValue(channel, "IsVisible", false);
                        }
                    }
                }
            }
        }
    }

    // 使用反射获取属性值
    private static object? GetPropertyValue(object obj, string propertyName)
    {
        return obj?.GetType().GetProperty(propertyName)?.GetValue(obj);
    }

    // 使用反射设置属性值
    private static void SetPropertyValue(object obj, string propertyName, object value)
    {
        obj?.GetType().GetProperty(propertyName)?.SetValue(obj, value);
    }
}
