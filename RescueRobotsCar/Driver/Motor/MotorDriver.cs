using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.Driver.Motor
{
    public class MotorConfig
    {
        // Motor GPIO Pins
        public readonly int FLFMotorPin = 24; // B-IN3
        public readonly int FLRMotorPin = 23; // B-IN4
        public readonly int FRFMotorPin = 6;  // B-IN1
        public readonly int FRRMotorPin = 12; // B-IN2
        public readonly int RLFMotorPin = 27; // A-IN1
        public readonly int RLRMotorPin = 17; // A-IN2
        public readonly int RRFMotorPin = 15; // A-IN3
        public readonly int RRRMotorPin = 14; // A-IN4

        // PWM Pins
        public readonly int FLPWMPin = 22; // B-ENB
        public readonly int FRPWMPin = 5;  // B-ENA
        public readonly int RLPWMPin = 18; // A-ENA
        public readonly int RRPWMPin = 4;  // A-ENB

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

        public void SetSpeed(int speed, bool inverted)
        {
            if (speed < -100 || speed > 100)
                throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be between -100 and 100.");
            if (inverted)
                speed = -speed;
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

            FrontLeftMotor?.SetSpeed(20, true);
            FrontRightMotor?.SetSpeed(20, true);
            RearLeftMotor?.SetSpeed(20, false);
            RearRightMotor?.SetSpeed(20, false);
            FrontLeftMotor?.Start();
            FrontRightMotor?.Start();
            RearLeftMotor?.Start();
            RearRightMotor?.Start();

            System.Threading.Thread.Sleep(2000); // Run for 2 seconds

            FrontLeftMotor?.SetSpeed(100, true);
            FrontRightMotor?.SetSpeed(100, true);
            RearLeftMotor?.SetSpeed(100, false);
            RearRightMotor?.SetSpeed(100, false);
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
            FrontLeftMotor?.SetSpeed(0, true);
            FrontRightMotor?.SetSpeed(0, true);
            RearLeftMotor?.SetSpeed(0, false);
            RearRightMotor?.SetSpeed(0, false);
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
