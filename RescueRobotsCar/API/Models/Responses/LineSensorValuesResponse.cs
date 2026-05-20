namespace RescueRobotsCar.API.Models.Responses
{
    public record LineSensorValuesResponse
    {
        public required int Left { get; init; }
        public required int LeftCenter { get; init; }
        public required int Center { get; init; }
        public required int RightCenter { get; init; }
        public required int Right { get; init; }
        public required double CalculatedMidpoint { get; init; }
        public required LineFollowerSettings Settings { get; init; }
    }

    public record LineFollowerSettings
    {
        public required int Speed { get; init; }
        public required double TurnFactor { get; init; }
        public required double SteeringBoostFactor { get; init; }
        public required int BackupSpeed { get; init; }
        public required int LineDetectionThreshold { get; init; }
        public required double LineCenteredRange { get; init; }
    }
}
