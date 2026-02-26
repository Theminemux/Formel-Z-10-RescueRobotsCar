using RescueRobotsCar.Driver.RFID;
using System.IO.Ports;
using System.Text;

namespace RescueRobotsCar.Driver.RFID
{
    public class RFIDRC522Config
    {

    }
}

namespace RescueRobotsCar.Driver.RFID
{
    public class RFIDRC522Driver
    {
        private readonly SerialPort _serialPort;
        private readonly StringBuilder _buffer = new StringBuilder();
        public event Action<string>? NewCardDetected;

        public RFIDRC522Driver()
        {
            _serialPort = new SerialPort("/dev/serial0", 9600)
            {
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500,
                NewLine = "\n",
                Encoding = System.Text.Encoding.ASCII  // explizit für ESP32
            };

            _serialPort.DataReceived += OnReceivedData;
            _serialPort.Open();
        }

        private void OnNewCardDetected(string carddata)
        {
            NewCardDetected?.Invoke(carddata);
        }

        private void OnReceivedData(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[RFID] Data received event triggered. Bytes available: {_serialPort.BytesToRead}");
                
                while (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();
                    Console.WriteLine($"[RFID] Read byte: {b} ('{(char)b}')");

                    if (b == '\n')  // ESP32 println() sendet \r\n
                    {
                        string line = _buffer.ToString().Trim();
                        Console.WriteLine($"[RFID] Newline detected. Buffer content: '{_buffer}'");
                        _buffer.Clear();

                        if (!string.IsNullOrEmpty(line))
                        {
                            Console.WriteLine($"[RFID] Valid line received: '{line}'");
                            OnNewCardDetected(line);
                            Console.WriteLine($"ESP → {line}");
                        }
                        else
                        {
                            Console.WriteLine("[RFID] Empty line, skipping...");
                        }
                    }
                    else if (b != '\r')  // Carriage Return ignorieren
                    {
                        _buffer.Append((char)b);
                        Console.WriteLine($"[RFID] Appended to buffer. Current buffer: '{_buffer}'");
                    }
                    else
                    {
                        Console.WriteLine("[RFID] Carriage return detected, ignoring...");
                    }
                }
                
                Console.WriteLine("[RFID] Finished processing received data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RFID] Serial Error: {ex.Message}");
                Console.WriteLine($"[RFID] Stack trace: {ex.StackTrace}");
            }
        }

        public void Dispose()
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
        }
    }

    public class RFIDReader : BackgroundService
    {
        private readonly RFIDRC522Driver _driver;
        
        // Driver über DI injizieren
        public RFIDReader(RFIDRC522Driver driver)
        {
            _driver = driver;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {

        }
    }
}
