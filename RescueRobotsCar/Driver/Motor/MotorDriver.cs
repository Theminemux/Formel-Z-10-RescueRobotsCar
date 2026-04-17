using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.Driver.Motor
{
    public class MotorConfig
    {
        // Motor GPIO Pins
        public readonly int FLFMotorPin = 14; // B-IN4 (Vorne Links Vorwärts)
        public readonly int FLRMotorPin = 15; // B-IN3 (Vorne Links Rückwärts)
        public readonly int FRFMotorPin = 17; // B-IN2 (Vorne Rechts Vorwärts)
        public readonly int FRRMotorPin = 27; // B-IN1 (Vorne Rechts Rückwärts)
        public readonly int RLFMotorPin = 23; // A-IN4 (Hinten Links Vorwärts)
        public readonly int RLRMotorPin = 24; // A-IN3 (Hinten Links Rückwärts)
        public readonly int RRFMotorPin = 12; // A-IN2 (Hinten Rechts Vorwärts)
        public readonly int RRRMotorPin = 6;  // A-IN1 (Hinten Rechts Rückwärts)

        // PWM Pins
        public readonly int FLPWMPin = 4;  // B-ENB (Vorne Links)
        public readonly int FRPWMPin = 18; // B-ENA (Vorne Rechts)
        public readonly int RLPWMPin = 22; // A-ENB (Hinten Links)
        public readonly int RRPWMPin = 5;  // A-ENA (Hinten Rechts)

        //BK1 = Vorne Rechts
        //BK3 = Vorne Links
        //AK1 = Hinten Rechts
        //AK3 = Hinten Links
    }

    public class MotorControls : IDisposable
    {
        private SoftwarePwmChannel _speed;
        private int _fDirectionPin;
        private int _rDirectionPin;
        private GpioController _gpio;

        private bool _forward = true;
        private bool _backward = false;
        public int Speed { get; private set; }

        public MotorControls(SoftwarePwmChannel speedChannel, int fDirectionPin, int rDirectionPin, ref GpioController gpio)
        {
            _speed = speedChannel;
            _fDirectionPin = fDirectionPin;
            _rDirectionPin = rDirectionPin;
            _gpio = gpio;
            Speed = 0;
        }

        public void SetSpeed(int speed)
        {
            if (speed < -100 || speed > 100)
                throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between -100 and 100.");
            Speed = speed;
            if (speed > 0)
            {
                // Forward
                _forward = true;
                _backward = false;
                _speed.DutyCycle = speed / 100.0;
            }
            else if (speed < 0)
            {
                // Backward
                _forward = false;
                _backward = true;
                _speed.DutyCycle = -speed / 100.0;
            }
            else
            {
                // Stop
                _forward = false;
                _backward = false;
                _speed.DutyCycle = 0.0;
            }
        }

        public void Start()
        {
            if (_forward)
                _gpio.Write(_fDirectionPin, PinValue.High);
            if (_backward)
                _gpio.Write(_rDirectionPin, PinValue.High);
            _speed.Start();
            
        }

        public void Stop()
        {
            _gpio.Write(_fDirectionPin, PinValue.Low);
            _gpio.Write(_rDirectionPin, PinValue.Low);
            _speed.Stop();
        }

        public void Dispose()
        {
            _speed.Dispose();
        }
    }

    public class MotorDriver : IDisposable
    {
        private readonly MotorConfig _config;
        private GpioController _gpio;

        public MotorControls? FrontLeftMotor { get; private set; }
        public MotorControls? FrontRightMotor { get; private set; }
        public MotorControls? RearLeftMotor { get; private set; }
        public MotorControls? RearRightMotor { get; private set; }

        public MotorDriver(MotorConfig config)
        {
            _config = config;
            _gpio = new();
        }

        public void InitializeMotors()
        {
            Console.WriteLine("Initializing motordriver");
            // GPIO Pins
            _gpio.OpenPin(_config.FLFMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.FLRMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.FRFMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.FRRMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.RLFMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.RLRMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.RRFMotorPin, PinMode.Output);
            _gpio.OpenPin(_config.RRRMotorPin, PinMode.Output);

            // PWM Channels
            var SpeedPwmFL = new SoftwarePwmChannel(_config.FLPWMPin, 1000, 0.0);
            var SpeedPwmFR = new SoftwarePwmChannel(_config.FRPWMPin, 1000, 0.0);
            var SpeedPwmRL = new SoftwarePwmChannel(_config.RLPWMPin, 1000, 0.0);
            var SpeedPwmRR = new SoftwarePwmChannel(_config.RRPWMPin, 1000, 0.0);

            FrontLeftMotor = new MotorControls(SpeedPwmFL, _config.FLFMotorPin, _config.FLRMotorPin, ref _gpio);
            FrontRightMotor = new MotorControls(SpeedPwmFR, _config.FRFMotorPin, _config.FRRMotorPin, ref _gpio);
            RearLeftMotor = new MotorControls(SpeedPwmRL, _config.RLFMotorPin, _config.RLRMotorPin, ref _gpio);
            RearRightMotor = new MotorControls(SpeedPwmRR, _config.RRFMotorPin, _config.RRRMotorPin, ref _gpio);
        }

        public void TestAllMotors()
        {
            if (FrontLeftMotor == null || FrontRightMotor == null || RearLeftMotor == null || RearRightMotor == null)
            {
                Console.WriteLine("Motors not initialized. Cannot perform test.");
                return;
            }

            FrontLeftMotor?.SetSpeed(20);
            FrontRightMotor?.SetSpeed(20);
            RearLeftMotor?.SetSpeed(20);
            RearRightMotor?.SetSpeed(20);
            FrontLeftMotor?.Start();
            FrontRightMotor?.Start();
            RearLeftMotor?.Start();
            RearRightMotor?.Start();

            System.Threading.Thread.Sleep(2000); // Run for 2 seconds

            FrontLeftMotor?.SetSpeed(100);
            FrontRightMotor?.SetSpeed(100);
            RearLeftMotor?.SetSpeed(100);
            RearRightMotor?.SetSpeed(100);
            FrontLeftMotor?.Start();
            FrontRightMotor?.Start();
            RearLeftMotor?.Start();
            RearRightMotor?.Start();

            System.Threading.Thread.Sleep(2000); // Run for 2 seconds

            StopAllMotors();

            Console.WriteLine("Motor test completed.");
        }

        public void StopAllMotors()
        {
            FrontLeftMotor?.SetSpeed(0);
            FrontRightMotor?.SetSpeed(0);
            RearLeftMotor?.SetSpeed(0);
            RearRightMotor?.SetSpeed(0);
            FrontLeftMotor?.Stop();
            FrontRightMotor?.Stop();
            RearLeftMotor?.Stop();
            RearRightMotor?.Stop();
        }

        public void Dispose()
        {
            _gpio.Dispose();
            FrontLeftMotor?.Dispose();
            FrontRightMotor?.Dispose();
            RearLeftMotor?.Dispose();
            RearRightMotor?.Dispose();
        }
    }
}
