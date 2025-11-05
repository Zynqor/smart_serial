namespace SerialProtocolAssistant.Services;

public interface ISerialPortService
{
    bool IsOpen { get; }
    event EventHandler<byte[]>? DataReceived;

    string[] GetAvailablePorts();
    void Open(string portName, int baudRate, int dataBits, string parity, string stopBits);
    void Close();
    void SendData(byte[] data);
}
