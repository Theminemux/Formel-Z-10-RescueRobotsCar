using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.Motor;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.Driving
{
    public class LineFollower
    {
        private readonly MotorDriver _motorDriver;
        private readonly LineSensor _lineSensor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _lineFollowingTask;

        public int Speed = 50;
        public int BackupSpeed = -20;
        public int LineDetectionThreshold = 512;
        public double LineCenteredRange = 0.1; // 10% des Sensorbereichs

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
                return _lineFollowingTask;
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
                    // Erwartet
                }
            }
        }

        private bool NoLineDetected(int[] points)
        {
            // Calculate the average of the sensor values
            return points.All(value => value < LineDetectionThreshold);
        }

        private bool IsLineCentered(double midpoint)
        {
            return Math.Abs(midpoint) < LineCenteredRange;
        }

        private async Task RunLineFollowing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int leftValue;
                    int rightValue;
                    var newDictionary = new Dictionary<string, string>();
                    
                    var sensorValues = _lineSensor.SensorValuesLeftRight;
                    newDictionary.Add("Sensor Values", $"Sensor Values: {string.Join(", ", sensorValues)}");

                    var adjustedValues = sensorValues.Select(value => 1024 - value).ToArray();
                    newDictionary.Add("Adjusted Sensor Values", $"Sensor Values: {string.Join(", ", adjustedValues)}");

                    var midpoint = LineSensorsMidpoint.CalculateMidpoint(adjustedValues);
                    newDictionary["Midpoint"] = midpoint.ToString("F2");

                    if (NoLineDetected(adjustedValues))
                    {
                        // Linie vollständig verloren - Rückwärts
                        leftValue = BackupSpeed;
                        rightValue = BackupSpeed;
                        newDictionary["Status"] = "Line Lost - Reversing";
                    }
                    else if (!IsLineCentered(midpoint))
                    {
                        leftValue = midpoint >= LineCenteredRange ? Speed : 0;
                        rightValue = midpoint <= 0 - LineCenteredRange ? Speed : 0;
                        newDictionary["Status"] = "Drifting - Correcting";
                    }
                    else
                    {
                        leftValue = Speed;
                        rightValue = Speed;
                        newDictionary["Status"] = "Following Line - Centered";
                    }

                    newDictionary["LeftValue"] = leftValue.ToString();
                    newDictionary["RightValue"] = rightValue.ToString();
                    newDictionary["Speed"] = Speed.ToString();
                    newDictionary["LineDetectionThreshold"] = LineDetectionThreshold.ToString();
                    newDictionary["LineCenteredRange"] = LineCenteredRange.ToString("F2");
                    newDictionary["BackupSpeed"] = BackupSpeed.ToString();

                    debugdata = newDictionary;

                    if (_motorDriver.FrontLeftMotor is null ||
                        _motorDriver.RearLeftMotor is null ||
                        _motorDriver.FrontRightMotor is null ||
                        _motorDriver.RearRightMotor is null)
                    {
                        Console.WriteLine("One or more motors are not initialized.");
                        continue;
                    }

                    _motorDriver.FrontLeftMotor.SetSpeed(leftValue);
                    _motorDriver.RearLeftMotor.SetSpeed(leftValue);
                    _motorDriver.FrontRightMotor.SetSpeed(rightValue);
                    _motorDriver.RearRightMotor.SetSpeed(rightValue);
                    _motorDriver.FrontLeftMotor.Start();
                    _motorDriver.RearLeftMotor.Start();
                    _motorDriver.FrontRightMotor.Start();
                    _motorDriver.RearRightMotor.Start();
                }
                finally
                {
                    Console.Write(".");
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

        public void ImportNewSettings(
            int speed, 
            int backupSpeed,
            int lineDetectionThreshold,
            double lineCenteredRange)
        {
            Speed = speed;
            BackupSpeed = backupSpeed;
            LineDetectionThreshold = lineDetectionThreshold;
            LineCenteredRange = lineCenteredRange;
        }
    }
}
