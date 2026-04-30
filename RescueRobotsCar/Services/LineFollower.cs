using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.Motor;

namespace RescueRobotsCar.Services
{
    public class LineFollower
    {
        private readonly MotorDriver _motorDriver;
        private readonly LineSensor _lineSensor;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _lineFollowingTask;

        public int Speed = 5;
        public double SensorSensitivity = 0.2;
        public double SteeringBoostFactor = 1.0;
        public int BackupSpeed = -20; // Rückwärtsgeschwindigkeit wenn keine Linie erkannt
        public double LineDetectionThreshold = 300; // Abweichung vom Mittelpunkt um Linie als "verloren" zu erkennen

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

        private bool IsLineDetected(double midpoint)
        {
            // Prüfe ob die Abweichung vom Mittelpunkt zu groß ist (Linie verloren)
            return Math.Abs(midpoint) < LineDetectionThreshold;
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

                    int leftValue;
                    int rightValue;

                    // Prüfe ob Linie erkannt wurde
                    if (IsLineDetected(midpoint))
                    {
                        // Normale Panzersteuerung
                        leftValue = (int)(0 - (midpoint * Speed));
                        rightValue = (int)(0 + (midpoint * Speed));
                        
                        newDictionary["Status"] = "Following Line";
                    }
                    else
                    {
                        // Linie verloren - fahre rückwärts
                        leftValue = BackupSpeed;
                        rightValue = BackupSpeed;
                        
                        newDictionary["Status"] = "Line Lost - Reversing";
                    }

                    // Begrenzung auf ±100
                    leftValue = (int)Math.Clamp(leftValue, -100, 100);
                    rightValue = (int)Math.Clamp(rightValue, -100, 100);

                    newDictionary["LeftValue"] = leftValue.ToString();
                    newDictionary["RightValue"] = rightValue.ToString();
                    newDictionary["Speed"] = Speed.ToString();
                    newDictionary["SteeringBoostFactor"] = SteeringBoostFactor.ToString("F2");
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

        public void ImportNewSettings(int speed, double sensorSensitivity, double steeringBoostFactor, int backupSpeed = -10)
        {
            Speed = speed;
            SensorSensitivity = sensorSensitivity;
            SteeringBoostFactor = steeringBoostFactor;
            BackupSpeed = backupSpeed;
        }
    }
}
