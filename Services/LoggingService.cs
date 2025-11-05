using Serilog;
using Serilog.Events;

namespace SerialProtocolAssistant.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;
    private readonly LogEventObserver _observer;

    public IObservable<string> LogStream => _observer;

    public LoggingService()
    {
        _observer = new LogEventObserver();

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Observers(events => events.Subscribe(_observer))
            .CreateLogger();
    }

    public void Information(string message)
    {
        _logger.Information(message);
    }

    public void Warning(string message)
    {
        _logger.Warning(message);
    }

    public void Error(string message)
    {
        _logger.Error(message);
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    private class LogEventObserver : IObserver<LogEvent>, IObservable<string>
    {
        private readonly List<IObserver<string>> _observers = new();

        public void OnNext(LogEvent value)
        {
            var message = $"{value.Timestamp:HH:mm:ss} [{value.Level}] {value.RenderMessage()}";
            foreach (var observer in _observers)
            {
                observer.OnNext(message);
            }
        }

        public void OnError(Exception error) { }
        public void OnCompleted() { }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<string>> _observers;
            private readonly IObserver<string> _observer;

            public Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                _observers.Remove(_observer);
            }
        }
    }
}
