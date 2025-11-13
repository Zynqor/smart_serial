using System.IO.Ports;

namespace SerialProtocolAssistant.Services;

public class SerialPortService : ISerialPortService, IDisposable
{
    private readonly ILoggingService _loggingService;
    private SerialPort? _serialPort;
    private readonly List<byte> _receiveBuffer = new();
    private readonly object _bufferLock = new();
    private System.Threading.Timer? _frameTimer;
    private const int FrameTimeoutMs = 50; // Modbus RTU frame timeout

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

        // Clean up frame timer and buffer
        _frameTimer?.Dispose();
        _frameTimer = null;
        lock (_bufferLock)
        {
            _receiveBuffer.Clear();
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

            _loggingService.Debug($"接收到数据片段: {BitConverter.ToString(buffer).Replace("-", " ")} ({buffer.Length} 字节)");

            lock (_bufferLock)
            {
                // Add received bytes to buffer
                _receiveBuffer.AddRange(buffer);

                // Reset frame timeout timer
                _frameTimer?.Dispose();
                _frameTimer = new System.Threading.Timer(OnFrameTimeout, null, FrameTimeoutMs, Timeout.Infinite);
            }
        }
        catch (Exception ex)
        {
            _loggingService.Error($"接收数据时出错: {ex.Message}");
        }
    }

    private void OnFrameTimeout(object? state)
    {
        lock (_bufferLock)
        {
            if (_receiveBuffer.Count > 0)
            {
                // 超时认为是完整一帧，交由上层处理
                var frame = _receiveBuffer.ToArray();
                _loggingService.Information($"接收到完整帧: {BitConverter.ToString(frame).Replace("-", " ")} ({frame.Length} 字节)");
                _receiveBuffer.Clear();

                // 触发数据接收事件
                DataReceived?.Invoke(this, frame);
            }
        }
    }

    private void ProcessFrameBuffer()
    {
        // 协议解析由上层根据JSON配置处理，这里不做协议相关的判断
        // 数据帧的识别完全依赖超时机制
        // 当超时发生时，OnFrameTimeout会将缓冲区的数据作为一帧发送
    }

    private bool ValidateModbusCrc(byte[] frame)
    {
        if (frame.Length < 4)
            return false;

        // Calculate CRC on all bytes except the last 2 (which contain the CRC)
        ushort calculatedCrc = CalculateModbusCrc16(frame, 0, frame.Length - 2);

        // Modbus CRC is sent low byte first
        ushort receivedCrc = (ushort)(frame[frame.Length - 2] | (frame[frame.Length - 1] << 8));

        return calculatedCrc == receivedCrc;
    }

    private ushort CalculateModbusCrc16(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            crc ^= data[offset + i];

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        return crc;
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
        _frameTimer?.Dispose();
        Close();
    }
}
