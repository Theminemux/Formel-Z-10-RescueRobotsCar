using RescueRobotsCar.Driver.MPU6050;

namespace RescueRobotsCar.Driving.Sensors
{
    public class Compass : BackgroundService
    {
        public double CurrentAngle { get; private set; }

        private readonly Mpu6050Driver _mpuDriver;
        private double _yaw = 0;
        private double _gyroZBias = 0; // Der Bias/Offset
        private DateTime _lastUpdate = DateTime.Now;

        public Compass(Mpu6050Driver mpu6050Driver)
        {
            _mpuDriver = mpu6050Driver;
        }

        public void ResetAngle()
        {
            _yaw = 0;
            CurrentAngle = 0;
            _lastUpdate = DateTime.Now;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _lastUpdate = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                double dt = (now - _lastUpdate).TotalSeconds;
                _lastUpdate = now;

                // Bias subtrahieren bevor man integriert!
                double correctedGyroZ = _mpuDriver.Data.GyroZ - _gyroZBias;

                // Gyro Z-Achse integrieren
                _yaw += correctedGyroZ * dt;

                // Normalize zu 0-360
                CurrentAngle = _yaw % 360;
                if (CurrentAngle < 0) CurrentAngle += 360;

                await Task.Delay(100, cancellationToken);
            }
        }

        public async Task CalibrateGyroscope(int samples = 100, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Calibrating Compass... Keep device still!");
            double sum = 0;

            for (int i = 0; i < samples; i++)
            {
                sum += _mpuDriver.Data.GyroZ;
                await Task.Delay(100, cancellationToken);
            }

            _gyroZBias = sum / samples;
            Console.WriteLine($"Gyroscope Bias: {_gyroZBias:F4} °/s");
        }
    }
}