using Iot.Device.ServoMotor;
using System.Device.Gpio;
using System.Device.Pwm.Drivers;

namespace RescueRobotsCar.Driver.Servo
{
    public class ServoDriver : BackgroundService
    {
        private volatile int _rotationDegree;

        private readonly int _servoPin = 13; // GPIO Pin für Servo

        private readonly ServoMotor _servo;

        public ServoDriver()
        {
            _rotationDegree = 0;

             _servo = new ServoMotor(new SoftwarePwmChannel(_servoPin));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Servo Driver started");
            _servo.Start();

            _servo.WriteAngle(90); // Test: Servo in Mittelstellung bringen
            Console.WriteLine("Servo testen");
            await Task.Delay(2000, stoppingToken); // Kurze Pause, um die Mittelstellung zu testen

            int lastDegree = -1;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (lastDegree != _rotationDegree)
                {
                    _servo.WriteAngle(_rotationDegree);
                    lastDegree = _rotationDegree;
                }

                await Task.Delay(20, stoppingToken);
            }

            _servo.Stop();
        }

        public void SetRotationDegree(int degree)
        {
            if (degree < 0 || degree > 180)
                throw new ArgumentOutOfRangeException(nameof(degree));

            _rotationDegree = degree;

            Console.WriteLine($"Servo rotation set to {degree}°");
        }
    }
}