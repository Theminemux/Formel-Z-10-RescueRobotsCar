using System.Device.I2c;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.Driver.MPU6050
{
    public class MPU6050Config
    {
        public readonly int Mpu6050I2cAddress = 0x68;
        public readonly int I2cBusId = 1;

        // Pins
        public readonly int SDAPin = 2;
        public readonly int SCLPin = 3;
    }

    public partial class Mpu6050Data
    {
        public short AccelX { get; set; }
        public short AccelY { get; set; }
        public short AccelZ { get; set; }
        public short GyroX { get; set; }
        public short GyroY { get; set; }
        public short GyroZ { get; set; }

        // Acceleration in mm/(10ms)² (assuming default ±2g range)
        // 1g = 9806.65 mm/s² = 0.980665 mm/(10ms)²
        // Sensitivity: 16384 LSB/g for ±2g range
        // Rounded: values between -10 mm/s² and 10 mm/s² (±1 cm/s²) are set to 0
        public float AccelXMms2
        {
            get
            {
                float value = (AccelX / 16384.0f) * 0.980665f;
                return Math.Abs(value) < 10.0f ? 0.0f : value;
            }
        }

        public float AccelYMms2
        {
            get
            {
                float value = (AccelY / 16384.0f) * 0.980665f;
                return Math.Abs(value) < 10.0f ? 0.0f : value;
            }
        }

        public float AccelZMms2
        {
            get
            {
                float value = (AccelZ / 16384.0f) * 0.980665f;
                return Math.Abs(value) < 10.0f ? 0.0f : value;
            }
        }

        // Angular velocity in degrees per (10ms) (assuming default ±250°/s range)
        // Sensitivity: 131 LSB/(°/s) for ±250°/s range
        // Converting to degrees per 10ms: divide by 100
        // Rounded: values between -1 and 1 are set to 0
        public float GyroXDps
        {
            get
            {
                float value = GyroX / 13100.0f;
                return Math.Abs(value) < 1.0f ? 0.0f : value;
            }
        }

        public float GyroYDps
        {
            get
            {
                float value = GyroY / 13100.0f;
                return Math.Abs(value) < 1.0f ? 0.0f : value;
            }
        }

        public float GyroZDps
        {
            get
            {
                float value = GyroZ / 13100.0f;
                return Math.Abs(value) < 1.0f ? 0.0f : value;
            }
        }

        // Pitch and Roll angles in degrees calculated from accelerometer data
        // Pitch: rotation around Y-axis (forward/backward tilt)
        // Roll: rotation around X-axis (left/right tilt)
        public float PitchDegrees => (float)(Math.Atan2(AccelY, Math.Sqrt(AccelX * AccelX + AccelZ * AccelZ)) * 180.0 / Math.PI);
        public float RollDegrees => (float)(Math.Atan2(-AccelX, AccelZ) * 180.0 / Math.PI);

        public override string ToString()
        {
            return $"Accel: ({AccelX}, {AccelY}, {AccelZ}) Raw, ({AccelXMms2:F2}, {AccelYMms2:F2}, {AccelZMms2:F2}) mm/s², " +
                   $"Gyro: ({GyroX}, {GyroY}, {GyroZ}) Raw, ({GyroXDps:F2}, {GyroYDps:F2}, {GyroZDps:F2}) °/s";
        }
    }

    public class Mpu6050Driver : BackgroundService
    {
        private I2cConnectionSettings _i2cSettings;
        private I2cDevice _i2cDevice;
        private MPU6050Config _config = new MPU6050Config();

        private readonly Logger _logger;

        public Mpu6050Data Data { get; private set; } = new Mpu6050Data();

        public Mpu6050Driver(Logger logger)
        {
            _logger = logger;

            _i2cSettings = new I2cConnectionSettings(_config.I2cBusId, _config.Mpu6050I2cAddress);
            _i2cDevice = I2cDevice.Create(_i2cSettings);
        }

        private void Initialize()
        {
            // Wake up the MPU6050 as it starts in sleep mode
            _i2cDevice.Write(new byte[] { 0x6B, 0x00 }); // PWR_MGMT_1 register
        }

        private Mpu6050Data ReadAll()
        {
            byte[] readBuffer = new byte[14];
            _i2cDevice.WriteRead(new byte[] { 0x3B }, readBuffer); // Starting from ACCEL_XOUT_H register
            Mpu6050Data data = new Mpu6050Data
            {
                AccelX = (short)((readBuffer[0] << 8) | readBuffer[1]),
                AccelY = (short)((readBuffer[2] << 8) | readBuffer[3]),
                AccelZ = (short)((readBuffer[4] << 8) | readBuffer[5]),
                GyroX = (short)((readBuffer[8] << 8) | readBuffer[9]),
                GyroY = (short)((readBuffer[10] << 8) | readBuffer[11]),
                GyroZ = (short)((readBuffer[12] << 8) | readBuffer[13])
            };
            return data;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Initialize();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10, cancellationToken);
                Data = ReadAll();
                //_logger.Log($"MPU6050 Data: {Data}", Logger.Severity.Info);
            }
        }
    }
}
