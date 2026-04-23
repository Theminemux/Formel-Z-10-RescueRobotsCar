using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.Motor;

namespace RescueRobotsCar.Services
{
    public class LineFollower
    {
        private readonly MotorDriver _motorDriver;
        private readonly LineSensor _lineSensor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _lineFollowingTask; // Als Feld speichern!

        public int Speed = 5;
        public double SensorSensitivity = 0.2;
        public double SteeringBoostFactor = 1.0;

        private Dictionary<string, string> debugdata = [];

        public LineFollower(MotorDriver motorDriver, LineSensor lineSensor)
        {
            _motorDriver = motorDriver;
            _lineSensor = lineSensor;
        }

        public Task Start()
        {
            if (_lineFollowingTask != null && !_lineFollowingTask.IsCompleted)
            {
                return _lineFollowingTask; // Task läuft bereits
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _lineFollowingTask = RunLineFollowing(_cancellationTokenSource.Token);
            return _lineFollowingTask;
        }

        public async Task Stop()
        {
            _cancellationTokenSource.Cancel();
            if (_lineFollowingTask != null)
            {
                try
                {
                    await _lineFollowingTask;
                }
                catch (OperationCanceledException)
                {
                    // Erwartet, wenn Task abgebrochen wird
                }
            }
        }

        private async Task RunLineFollowing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var newDictionary = new Dictionary<string, string>();

                    var sensorValues = _lineSensor.SensorValuesLeftRight;
                    var midpoint = LineSensorsMidpoint.CalculateMidpoint(
                        sensorValues.Select(value => 1024 - value).ToArray());
                    newDictionary["Midpoint"] = midpoint.ToString("F2");

                    var leftValue = (midpoint <= 0 || Math.Abs(midpoint) < SensorSensitivity
                        ? 1 + (Math.Abs(midpoint) * SteeringBoostFactor)
                        : 1 - Math.Abs(midpoint)) * Speed;
                    var rightValue = (midpoint >= 0 || Math.Abs(midpoint) < SensorSensitivity
                        ? 1 + (Math.Abs(midpoint) * SteeringBoostFactor)
                        : 1 - Math.Abs(midpoint)) * Speed;

                    newDictionary["LeftValue"] = leftValue.ToString("F2");
                    newDictionary["RightValue"] = rightValue.ToString("F2");
                    newDictionary["Speed"] = Speed.ToString();
                    newDictionary["SensorSensitivity"] = SensorSensitivity.ToString("F2");
                    newDictionary["SteeringBoostFactor"] = SteeringBoostFactor.ToString("F2");

                    debugdata = newDictionary;

                    if (_motorDriver.FrontLeftMotor is null ||
                        _motorDriver.RearLeftMotor is null ||
                        _motorDriver.FrontRightMotor is null ||
                        _motorDriver.RearRightMotor is null)
                    {
                        Console.WriteLine("One or more motors are not initialized.");
                        continue;
                    }

                    _motorDriver.FrontLeftMotor.SetSpeed((int)leftValue);
                    _motorDriver.RearLeftMotor.SetSpeed((int)leftValue);
                    _motorDriver.FrontRightMotor.SetSpeed((int)rightValue);
                    _motorDriver.RearRightMotor.SetSpeed((int)rightValue);
                    _motorDriver.FrontLeftMotor.Start();
                    _motorDriver.RearLeftMotor.Start();
                    _motorDriver.FrontRightMotor.Start();
                    _motorDriver.RearRightMotor.Start();
                }
                finally
                {
                    await Task.Delay(50, cancellationToken);
                }
            }
            Console.WriteLine("Stopping all motors.");
            _motorDriver.StopAllMotors();
        }

        public Dictionary<string, string> GetDebugData()
        {
            return debugdata;
        }

        public void ImportNewSettings(int speed, double sensorSensitivity, double steeringBoostFactor)
        {
            Speed = speed;
            SensorSensitivity = sensorSensitivity;
            SteeringBoostFactor = steeringBoostFactor;
        }
    }
}
