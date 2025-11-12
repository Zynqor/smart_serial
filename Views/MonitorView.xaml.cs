using System.Windows;
using System.Windows.Controls;
using SerialProtocolAssistant.ViewModels;
using SerialProtocolAssistant.Models;
using ScottPlot;

namespace SerialProtocolAssistant.Views;

public partial class MonitorView : UserControl
{
    public MonitorView()
    {
        InitializeComponent();
        InitializeChart();

        // 当DataContext变化时，通知ViewModel图表已准备好
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// 初始化ScottPlot图表
    /// </summary>
    private void InitializeChart()
    {
        // 设置图表基本样式
        ChartPlot.Plot.Title("实时数据曲线");
        ChartPlot.Plot.XLabel("时间 (秒)");
        ChartPlot.Plot.YLabel("数值");

        // 设置图表样式
        ChartPlot.Plot.Style.Background(System.Drawing.Color.White);
        ChartPlot.Plot.Style.FigureBackground(System.Drawing.Color.White);

        // 启用图例
        ChartPlot.Plot.Legend(location: Alignment.UpperRight);

        // 刷新图表
        ChartPlot.Refresh();
    }

    /// <summary>
    /// DataContext变化时，通知ViewModel
    /// </summary>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is MonitorViewModel viewModel)
        {
            viewModel.SetChartPlot(ChartPlot);
        }
    }

    /// <summary>
    /// 全选所有通道
    /// </summary>
    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MonitorViewModel viewModel)
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

    /// <summary>
    /// 全不选所有通道
    /// </summary>
    private void DeselectAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MonitorViewModel viewModel)
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
