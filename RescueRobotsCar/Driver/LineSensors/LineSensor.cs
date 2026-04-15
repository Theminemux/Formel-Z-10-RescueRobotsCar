using System.Device.Spi;

namespace RescueRobotsCar.Driver.LineSensors
{
    public class LineSensor : BackgroundService
    {
        private readonly SpiDevice _spi;

        public int[] SensorValuesLeftRight { get; private set; }

        public LineSensor()
        {
            var settings = new SpiConnectionSettings(0, 0) // Bus 0, CE0
            {
                ClockFrequency = 500000,
                Mode = SpiMode.Mode0
            };

            _spi = SpiDevice.Create(settings);

            SensorValuesLeftRight = new int[5];
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Line Sensor started.");
            while (!cancellationToken.IsCancellationRequested)
            {
                int[] values = new int[5];

                for (int i = 0; i < 5; i++)
                {
                    values[i] = ReadChannel(i);
                }

                SensorValuesLeftRight = values;

                await Task.Delay(50); // kannst du später auf 10 runterdrehen
            }
        }

        private int ReadChannel(int channel)
        {
            byte[] writeBuffer = new byte[3];
            byte[] readBuffer = new byte[3];

            writeBuffer[0] = 0x01;
            writeBuffer[1] = (byte)((0x08 | channel) << 4);
            writeBuffer[2] = 0x00;

            _spi.TransferFullDuplex(writeBuffer, readBuffer);

            int value = ((readBuffer[1] & 0x03) << 8) | readBuffer[2];
            return value;
        }
    }
}