using Microsoft.AspNetCore.Routing.Constraints;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.Motor;
using RescueRobotsCar.Driving.Sensors;

namespace RescueRobotsCar.Services
{
    public class LineFollower
    {
        private readonly MotorDriver _motorDriver;
        private readonly LineSensor _lineSensor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _lineFollowingTask;

        private const int Speed = 30;
        private const double SensorSensitivity = 0.1;
        private const double MotorDeccelerationFactor = 2.0;

        private Dictionary<string, string> debugdata = [];
        private bool _motorsStarted = false;

        public LineFollower(MotorDriver motorDriver, LineSensor lineSensor)
        {
            _motorDriver = motorDriver;
            _lineSensor = lineSensor;
        }

        public Task Start()
        {
            if (_lineFollowingTask != null && !_lineFollowingTask.IsCompleted)
            {
                return _lineFollowingTask;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _motorsStarted = false;
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
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var newDictionary = new Dictionary<string, string>();

                    var sensorValues = _lineSensor.SensorValuesLeftRight;
                    var midpoint = LineSensorsMidpoint.CalculateMidpoint(
                        sensorValues.Select(value => 1024 - value).ToArray());
                    newDictionary["Midpoint"] = midpoint.ToString("F2");

                    var leftValue = (midpoint <= 0 || Math.Abs(midpoint) < SensorSensitivity
                        ? 1
                        : 1 - (Math.Abs(midpoint)) * MotorDeccelerationFactor) * Speed;
                    var rightValue = (midpoint >= 0 || Math.Abs(midpoint) < SensorSensitivity
                        ? 1
                        : 1 - (Math.Abs(midpoint)) * MotorDeccelerationFactor) * Speed;
                    newDictionary["LeftValue"] = leftValue.ToString("F2");
                    newDictionary["RightValue"] = rightValue.ToString("F2");

                    debugdata = newDictionary;

                    if (_motorDriver.FrontLeftMotor is null ||
                        _motorDriver.RearLeftMotor is null ||
                        _motorDriver.FrontRightMotor is null ||
                        _motorDriver.RearRightMotor is null)
                    {
                        Console.WriteLine("One or more motors are not initialized.");
                        await Task.Delay(50, cancellationToken);
                        continue;
                    }

                    _motorDriver.FrontLeftMotor.SetSpeed((int)leftValue);
                    _motorDriver.RearLeftMotor.SetSpeed((int)leftValue);
                    _motorDriver.FrontRightMotor.SetSpeed((int)rightValue);
                    _motorDriver.RearRightMotor.SetSpeed((int)rightValue);

                    // Start() nur einmal aufrufen!
                    _motorDriver.FrontLeftMotor.Start();
                    _motorDriver.RearLeftMotor.Start();
                    _motorDriver.FrontRightMotor.Start();
                    _motorDriver.RearRightMotor.Start();
                    _motorsStarted = true;

                    await Task.Delay(50, cancellationToken);
                }
            }
            finally
            {
                Console.WriteLine("LineFollower stops Motors.");
                _motorDriver.StopAllMotors();
            }
        }

        public Dictionary<string, string> GetDebugData()
        {
            return debugdata;
        }
    }
}
