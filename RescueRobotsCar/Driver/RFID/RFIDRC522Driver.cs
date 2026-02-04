using Iot.Device.Card.Mifare;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using System.Device.Gpio;
using System.Device.Spi;

namespace RescueRobotsCar.Driver.RFID
{
    public class RFIDRC522Config
    {
        public readonly int BusID = 0;
        public readonly int ChipSelectLine = 0;
        public readonly int ResetPin = 25;
    }

    public class RFIDRC522Driver
    {
        private readonly RFIDRC522Config _config = new RFIDRC522Config();

        public bool CardPresent => _rfid.IsCardPresent(new byte[2]);
        public MfRc522 Rfid => _rfid;

        private SpiDevice _spi;
        private GpioController _gpio;
        private int _resetPin;
        private MfRc522 _rfid;

        public RFIDRC522Driver()
        {
            _resetPin = _config.ResetPin;

            _gpio = new GpioController();

            // SPI erst DANACH initialisieren
            _spi = SpiDevice.Create(new SpiConnectionSettings(_config.BusID, _config.ChipSelectLine)
            {
                ClockFrequency = 1_000_000,  // Erstmal langsamer (1MHz statt 10MHz)!
                Mode = SpiMode.Mode0
            });

            _rfid = new MfRc522(_spi, _resetPin, _gpio);
        }

        public async Task Init()
        {
            // RESET: Low → 50ms warten → High → 50ms warten
            _gpio.OpenPin(_resetPin, PinMode.Output);
            _gpio.Write(_resetPin, PinValue.Low);
            await Task.Delay(50);
            _gpio.Write(_resetPin, PinValue.High);
            await Task.Delay(50);

            // PCD_Init() aufrufen!
            _rfid.SoftReset();

            await Task.Delay(50);

            Console.WriteLine("RFID RC522 initialisiert");
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
            await _driver.Init();

            Console.WriteLine("🔍 RFID Scan STARTED - KARTE AUFLEGEN!");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    bool cardFound = _driver.Rfid.ListenToCardIso14443TypeA(
                        out Data106kbpsTypeA card,
                        TimeSpan.FromMilliseconds(300)
                    );

                    if (cardFound)
                    {
                        Console.WriteLine($"✅ KARTE! UID: {BitConverter.ToString(card.NfcId)}");

                        var mifare = new MifareCard(_driver.Rfid, card.TargetNumber);
                        mifare.SerialNumber = card.NfcId;
                        mifare.Capacity = MifareCardCapacity.Mifare1K;
                        mifare.BlockNumber = 4;
                        mifare.KeyA = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                        mifare.Command = MifareCardCommand.AuthenticationA;

                        if (mifare.RunMifareCardCommand() >= 0)
                        {
                            mifare.Command = MifareCardCommand.Read16Bytes;
                            if (mifare.RunMifareCardCommand() >= 0 && mifare.Data != null)
                            {
                                string text = System.Text.Encoding.ASCII.GetString(mifare.Data).TrimEnd('\0');
                                Console.WriteLine($"✅ BLOCK 4: \"{text}\"");
                            }
                        }
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ ERROR: {ex.Message}");
                }

                await Task.Delay(250, ct);
            }
        }
    }
}
