using System.Windows;
using System.Windows.Controls;

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
        if (DataContext is ViewModels.MonitorViewModel viewModel)
        {
            foreach (var device in viewModel.Devices)
            {
                foreach (var channel in device.ChannelData)
                {
                    channel.IsVisible = true;
                }
            }
        }
    }

    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        // 取消选择所有通道
        if (DataContext is ViewModels.MonitorViewModel viewModel)
        {
            foreach (var device in viewModel.Devices)
            {
                foreach (var channel in device.ChannelData)
                {
                    channel.IsVisible = false;
                }
            }
        }
    }
}
