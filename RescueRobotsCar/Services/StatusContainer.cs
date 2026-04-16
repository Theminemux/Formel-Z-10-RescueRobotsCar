using RescueRobotsCar.Driving.Maps.MapObjects;

namespace RescueRobotsCar.Services
{
    public record StatusContainer
    {
        public enum EStatus
        {
            Stopped = 0,
            Driving = 1,
            Pause = 2,
            Finished = 3
        }

        public required MapObjectsContainer? MapObjects { get; init; }
        public required int X { get; init; }
        public required int Y { get; init; }
        public required int Lv { get; init; }
        public required int Status { get; init; }
        public required IReadOnlyList<string> CollectedObjects { get; init; }
        public required bool TargetObjectCollected { get; init; }
        public string? ErrorMessage { get; init; }

        public static StatusContainer Default => new StatusContainer
        {
            MapObjects = null,
            X = 0,
            Y = 9,
            Lv = 0,
            Status = (int)EStatus.Stopped,
            CollectedObjects = [],
            TargetObjectCollected = false,
            ErrorMessage = null
        };
    }
}
