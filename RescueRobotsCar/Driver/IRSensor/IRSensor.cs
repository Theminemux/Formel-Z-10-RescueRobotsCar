using System.Device.Spi;

namespace RescueRobotsCar.Driver.IRSensor
{
    public class IRSensor : BackgroundService
    {
        private readonly SpiDevice _spi;
        private bool _running;

        public IRSensor()
        {
            var settings = new SpiConnectionSettings(0, 0) // Bus 0, CE0
            {
                ClockFrequency = 500000,
                Mode = SpiMode.Mode0
            };

            _spi = SpiDevice.Create(settings);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int[] values = new int[5];

                for (int i = 0; i < 5; i++)
                {
                    values[i] = ReadChannel(i);
                }

                Console.WriteLine(
                    $"L2:{values[0],4}  L1:{values[1],4}  M:{values[2],4}  R1:{values[3],4}  R2:{values[4],4}"
                );

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