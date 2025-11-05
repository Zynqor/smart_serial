using System.IO.Ports;

namespace SerialProtocolAssistant.Services;

public class SerialPortService : ISerialPortService, IDisposable
{
    private readonly ILoggingService _loggingService;
    private SerialPort? _serialPort;

    public bool IsOpen => _serialPort?.IsOpen ?? false;
    public event EventHandler<byte[]>? DataReceived;

    public SerialPortService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public void Open(string portName, int baudRate, int dataBits, string parity, string stopBits)
    {
        try
        {
            Close(); // 关闭已有连接

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = ParseParity(parity),
                StopBits = ParseStopBits(stopBits)
            };

            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();

            _loggingService.Information($"串口已打开: {portName}, {baudRate}, {dataBits}, {parity}, {stopBits}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"打开串口失败: {ex.Message}");
            throw;
        }
    }

    public void Close()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.DataReceived -= OnDataReceived;
            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
            _loggingService.Information("串口已关闭");
        }
    }

    public void SendData(byte[] data)
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                _loggingService.Warning("串口未打开，无法发送数据");
                return;
            }

            _serialPort.Write(data, 0, data.Length);
            _loggingService.Information($"已发送数据: {BitConverter.ToString(data).Replace("-", " ")}");
        }
        catch (Exception ex)
        {
            _loggingService.Error($"发送数据失败: {ex.Message}");
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            var bytesToRead = _serialPort.BytesToRead;
            var buffer = new byte[bytesToRead];
            _serialPort.Read(buffer, 0, bytesToRead);

            _loggingService.Information($"接收到数据: {BitConverter.ToString(buffer).Replace("-", " ")}");
            DataReceived?.Invoke(this, buffer);
        }
        catch (Exception ex)
        {
            _loggingService.Error($"接收数据时出错: {ex.Message}");
        }
    }

    private Parity ParseParity(string parity)
    {
        return parity switch
        {
            "None" => Parity.None,
            "Odd" => Parity.Odd,
            "Even" => Parity.Even,
            "Mark" => Parity.Mark,
            "Space" => Parity.Space,
            _ => Parity.None
        };
    }

    private StopBits ParseStopBits(string stopBits)
    {
        return stopBits switch
        {
            "1" => StopBits.One,
            "1.5" => StopBits.OnePointFive,
            "2" => StopBits.Two,
            _ => StopBits.One
        };
    }

    public void Dispose()
    {
        Close();
    }
}
