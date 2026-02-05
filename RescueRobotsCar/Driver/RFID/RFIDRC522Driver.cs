using Iot.Device.Card.Mifare;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using System.Device.Gpio;
using System.Device.Spi;
using System.Reflection.PortableExecutable;

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
        public enum Register : byte
        {
            TModeReg = 0x2A,
            TPrescalerReg = 0x2B,
            TReloadRegL = 0x2C,
            TReloadRegH = 0x2D,
            TxAutoReg = 0x2E,
            ModeReg = 0x11,
            TxControlReg = 0x14,
            RxGainReg = 0x14  // Gain Property nutzt das
        }


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
                ClockFrequency = 10_000_000,  // Erstmal langsamer (0.4MHz statt 10MHz)!
                Mode = SpiMode.Mode0
            });

            _rfid = new MfRc522(_spi, _resetPin, _gpio);
        }

        public async Task Init()
        {
            _rfid.SoftReset();

            await Task.Delay(50);

            Console.WriteLine("RFID RC522 initialisiert");
        }

        public async Task ScanAndRead(CancellationToken ct)
        {
            Console.WriteLine("Suche nach Karte...");

            byte[] atqa = new byte[2];

            byte block = 4;
            byte[] keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            try
            {
                if (!_rfid.IsCardPresent(atqa, false))
                {
                    Console.WriteLine("Keine Karte gefunden.");
                    return;
                }

                if (!_rfid.ListenToCardIso14443TypeA(out var card, TimeSpan.FromSeconds(1)))
                {
                    Console.WriteLine("Karte konnte nicht gelesen werden.");
                    return;
                }

                var mifare = new MifareCard(_rfid, card.TargetNumber);
                var uid = card.NfcId;
                Console.WriteLine($"Karte gefunden! UID: {BitConverter.ToString(uid)}");

                var authStatus = _rfid.MifareAuthenticate(keyA, MifareCardCommand.AuthenticationA, block, uid);
                if (authStatus != Status.Ok)
                {
                    Console.WriteLine("Authentifizierung fehlgeschlagen.");
                    return;
                }

                byte[] buffer = new byte[16]; // zum Empfangen der Daten
                var sendBuffer = new byte[2] { 0x30, block }; // 0x30 = Read, Blocknummer

                var receiveBuffer = buffer;

                Status readStatus = (Status) _rfid.Transceive(0, sendBuffer, receiveBuffer.AsSpan(), Iot.Device.Card.NfcProtocol.Mifare);

                if (readStatus == Status.Ok)
                {
                    Console.WriteLine($"Block {block}: {BitConverter.ToString(receiveBuffer.ToArray())}");
                }
                else
                {
                    Console.WriteLine("Fehler beim Lesen des Blocks.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Lesen der Karte: {ex.Message}");
            }
            finally
            {
                await Task.Delay(500, ct);
            }
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
                await _driver.ScanAndRead(ct);
            }
        }
    }
}
