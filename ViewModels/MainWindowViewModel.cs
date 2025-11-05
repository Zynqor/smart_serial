using CommunityToolkit.Mvvm.ComponentModel;

namespace SerialProtocolAssistant.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public SerialSettingsViewModel SerialSettingsViewModel { get; }
    public ProtocolControlViewModel ProtocolControlViewModel { get; }
    public DataDisplayViewModel DataDisplayViewModel { get; }
    public LogViewModel LogViewModel { get; }

    public MainWindowViewModel(
        SerialSettingsViewModel serialSettingsViewModel,
        ProtocolControlViewModel protocolControlViewModel,
        DataDisplayViewModel dataDisplayViewModel,
        LogViewModel logViewModel)
    {
        SerialSettingsViewModel = serialSettingsViewModel;
        ProtocolControlViewModel = protocolControlViewModel;
        DataDisplayViewModel = dataDisplayViewModel;
        LogViewModel = logViewModel;

        // 自动加载默认协议文件
        LoadDefaultProtocol();
    }

    private void LoadDefaultProtocol()
    {
        try
        {
            // 获取exe所在目录（单文件发布兼容）
            var exePath = System.Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDirectory = System.IO.Path.GetDirectoryName(exePath) ?? System.AppDomain.CurrentDomain.BaseDirectory;

            var defaultProtocolPath = System.IO.Path.Combine(
                exeDirectory,
                "example_protocol.json");

            if (System.IO.File.Exists(defaultProtocolPath))
            {
                ProtocolControlViewModel.LoadProtocolFromFile(defaultProtocolPath);
            }
        }
        catch
        {
            // 忽略加载失败，用户可以手动加载
        }
    }
}
