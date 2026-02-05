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
                ClockFrequency = 400_000,  // Erstmal langsamer (0.4MHz statt 10MHz)!
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
            PcdInit(_spi);

            await Task.Delay(50);

            Console.WriteLine("RFID RC522 initialisiert");
        }

        public void PcdInit(SpiDevice spi)
        {
            // 1. PCD Soft Reset
            _rfid.SoftReset();
            Thread.Sleep(50);

            // 2. Timer stoppen + Prescaler setzen (Arduino PCD_Init)
            WriteSpiRegister(spi, Register.TModeReg, 0x8D);
            WriteSpiRegister(spi, Register.TPrescalerReg, 0x3E);
            WriteSpiRegister(spi, Register.TReloadRegL, 30);
            WriteSpiRegister(spi, Register.TReloadRegH, 0);

            // 3. TX Auto + CRC aktivieren
            WriteSpiRegister(spi, Register.TxAutoReg, 0x40);
            WriteSpiRegister(spi, Register.ModeReg, 0x3D);

            // 4. MAX RECEIVER GAIN (48dB)

            // 5. ANTENNENFELD AKTIVIEREN (Tx1+Tx2 ON)
            byte txControl = ReadSpiRegister(spi, Register.TxControlReg);
            txControl |= 0x03;  // Bit 0+1 = Tx1, Tx2 ON
            WriteSpiRegister(spi, Register.TxControlReg, txControl);

            // 6. Timer neu starten
            WriteSpiRegister(spi, Register.TModeReg, 0x8D);

            Console.WriteLine("✅ PCD_Init() komplett - Antenne ON!");
        }

        private void WriteSpiRegister(SpiDevice spi, Register reg, byte value)
        {
            // MFRC522 SPI Protokoll: [0xA0 | reg] + [value]
            byte[] cmd = { (byte)(0xA0 | (byte)reg), value };
            spi.Write(cmd);
            Thread.Sleep(5);
        }

        private byte ReadSpiRegister(SpiDevice spi, Register reg)
        {
            // MFRC522 SPI Protokoll: [0x60 | reg] → 1 Byte lesen
            byte[] cmd = { (byte)(0x60 | (byte)reg) };
            byte[] response = new byte[1];

            spi.Write(cmd);
            Thread.Sleep(5);
            spi.Read(response);

            return response[0];
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
