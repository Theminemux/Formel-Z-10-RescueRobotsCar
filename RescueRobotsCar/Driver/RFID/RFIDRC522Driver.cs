using Iot.Device.Card.Mifare;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using System.Device.Gpio;
using System.Device.Spi;
using System.Reflection.PortableExecutable;
using System.IO.Ports;

namespace RescueRobotsCar.Driver.RFID
{
    public class RFIDRC522Config
    {
        
    }

    public class RFIDRC522Driver
    {
        private readonly SerialPort _serialPort;
        public event Action<string>? NewCardDetected;

        public RFIDRC522Driver()
        {
            _serialPort = new SerialPort("/dev/serial0", 9600);
            _serialPort.DataReceived += OnReceivedData;
            _serialPort.Open();
        }

        private void OnNewCardDetected(string carddata)
        {
            NewCardDetected?.Invoke(carddata);
        }

        private void OnReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            var data = _serialPort.ReadLine();
            OnNewCardDetected(data);
            Console.WriteLine($"Received Card Data from ESP. Data: {data}");
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
