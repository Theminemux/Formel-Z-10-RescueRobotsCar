using RescueRobotsCar.Driving.Maps.MapObjects;

namespace RescueRobotsCar.Driving.Maps.Status
{
    public record StatusContainer
    {
        public required MapObjectsContainer? MapObjects { get; init; }
    }
}
