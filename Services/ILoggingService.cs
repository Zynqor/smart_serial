namespace SerialProtocolAssistant.Services;

public interface ILoggingService
{
    IObservable<string> LogStream { get; }
    void Information(string message);
    void Warning(string message);
    void Error(string message);
    void Debug(string message);
}
