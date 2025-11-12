using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Text;
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

        // 添加键盘快捷键处理
        LogListBox.PreviewKeyDown += LogListBox_PreviewKeyDown;
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

    /// <summary>
    /// 键盘快捷键处理
    /// </summary>
    private void LogListBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
        {
            CopySelectedLogs();
            e.Handled = true;
        }
        else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
        {
            LogListBox.SelectAll();
            e.Handled = true;
        }
    }

    /// <summary>
    /// 复制选中的日志
    /// </summary>
    private void CopySelectedLogs()
    {
        if (LogListBox.SelectedItems.Count > 0)
        {
            var sb = new StringBuilder();
            foreach (var item in LogListBox.SelectedItems)
            {
                sb.AppendLine(item.ToString());
            }

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 复制菜单项点击事件
    /// </summary>
    private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CopySelectedLogs();
    }

    /// <summary>
    /// 全选菜单项点击事件
    /// </summary>
    private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
    {
        LogListBox.SelectAll();
    }

    /// <summary>
    /// 清空日志菜单项点击事件
    /// </summary>
    private void ClearMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LogViewModel viewModel)
        {
            var result = MessageBox.Show("确定要清空所有日志吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                viewModel.ClearLogs();
            }
        }
    }
}
