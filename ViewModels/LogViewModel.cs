using CommunityToolkit.Mvvm.ComponentModel;
using SerialProtocolAssistant.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace SerialProtocolAssistant.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    private const int MaxLogMessages = 1000;

    public LogViewModel(ILoggingService loggingService)
    {
        loggingService.LogStream.Subscribe(new LogObserver(this));
    }

    private class LogObserver : IObserver<string>
    {
        private readonly LogViewModel _viewModel;

        public LogObserver(LogViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void OnNext(string value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.LogMessages.Add(value);

                // 限制日志数量
                while (_viewModel.LogMessages.Count > MaxLogMessages)
                {
                    _viewModel.LogMessages.RemoveAt(0);
                }
            });
        }

        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}
