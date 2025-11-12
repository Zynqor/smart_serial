using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using SerialProtocolAssistant.ViewModels;

namespace SerialProtocolAssistant.Views;

public partial class LogView : UserControl
{
    // 字体大小依赖属性
    public static readonly DependencyProperty LogFontSizeProperty =
        DependencyProperty.Register(nameof(LogFontSize), typeof(double), typeof(LogView), new PropertyMetadata(11.0));

    public double LogFontSize
    {
        get => (double)GetValue(LogFontSizeProperty);
        set => SetValue(LogFontSizeProperty, value);
    }

    public LogView()
    {
        InitializeComponent();
        DataContextChanged += LogView_DataContextChanged;
    }

    /// <summary>
    /// DataContext变化时订阅日志消息集合变化事件
    /// </summary>
    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is LogViewModel viewModel)
        {
            // 订阅集合变化事件，实现自动滚动
            viewModel.LogMessages.CollectionChanged += LogMessages_CollectionChanged;
        }
    }

    /// <summary>
    /// 日志消息添加时自动滚动到最新行
    /// </summary>
    private void LogMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (LogListBox.Items.Count > 0)
                {
                    LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// 鼠标滚轮缩放字体（Ctrl + 滚轮）
    /// </summary>
    private void LogListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Ctrl + 滚轮缩放字体
            if (e.Delta > 0)
            {
                LogFontSize = Math.Min(LogFontSize + 1, 24); // 最大24
            }
            else
            {
                LogFontSize = Math.Max(LogFontSize - 1, 8); // 最小8
            }
            e.Handled = true;
        }
    }
}
