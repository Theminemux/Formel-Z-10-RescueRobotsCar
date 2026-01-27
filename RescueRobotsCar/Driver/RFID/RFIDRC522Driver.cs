using System.Device.Gpio;
using System.Device.Spi;

namespace RescueRobotsCar.Driver.RFID
{
    public class RFIDRC522Config
    {
        public readonly int BusID = 0x68;
        public readonly int ChipSelectLine = 1;
        public readonly int ResetPin = 2;
    }

    public class RFIDRC522Driver
    {
        private readonly RFIDRC522Config _config = new RFIDRC522Config();

        public bool CardPresent => IsCardPresent();

        private SpiDevice _spi;
        private GpioController _gpio;
        private int _resetPin;

        public RFIDRC522Driver()
        {
            this._resetPin = _config.ResetPin;

            _gpio = new GpioController();
            _gpio.OpenPin(_resetPin, PinMode.Output);
            _gpio.Write(_resetPin, PinValue.High);

            _spi = SpiDevice.Create(new SpiConnectionSettings(_config.BusID, _config.ChipSelectLine)
            {
                ClockFrequency = 1_000_000,
                Mode = SpiMode.Mode0
            });

            Init();
        }

        private void Init()
        {
            WriteRegister(0x01, 0x0F); // Soft Reset
            Thread.Sleep(50);

            WriteRegister(0x2A, 0x8D); // TMode
            WriteRegister(0x2B, 0x3E); // TPrescaler
            WriteRegister(0x2D, 30);   // TReload low
            WriteRegister(0x2C, 0);    // TReload high

            WriteRegister(0x15, 0x40); // TxASK
            WriteRegister(0x11, 0x3D); // Mode

            // Antenne einschalten
            byte val = ReadRegister(0x14);
            WriteRegister(0x14, (byte)(val | 0x03));
        }

        private void WriteRegister(byte reg, byte value)
        {
            byte[] buffer = {
                (byte)((reg << 1) & 0x7E),
                value
            };
            _spi.Write(buffer);
        }

        private byte ReadRegister(byte reg)
        {
            byte[] writeBuffer = {
                (byte)(((reg << 1) & 0x7E) | 0x80),
                0x00
            };

            byte[] readBuffer = new byte[2];
            _spi.TransferFullDuplex(writeBuffer, readBuffer);

            return readBuffer[1];
        }

        public bool IsCardPresent()
        {
            WriteRegister(0x0D, 0x07); // BitFramingReg
            WriteRegister(0x09, 0x26); // REQA

            WriteRegister(0x01, 0x0C); // Transceive
            WriteRegister(0x0D, 0x87); // StartSend

            Thread.Sleep(5);

            byte irq = ReadRegister(0x04);
            return (irq & 0x30) != 0;
        }

        public byte[] ReadUid()
        {
            WriteRegister(0x09, 0x93); // SELECT
            WriteRegister(0x09, 0x20);

            WriteRegister(0x01, 0x0C);
            WriteRegister(0x0D, 0x87);

            Thread.Sleep(5);

            byte length = ReadRegister(0x0A);
            byte[] uid = new byte[length];

            for (int i = 0; i < length; i++)
                uid[i] = ReadRegister(0x09);

            return uid;
        }
    }
}
