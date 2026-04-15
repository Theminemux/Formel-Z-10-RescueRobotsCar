using RescueRobotsCar.Driving.Maps.Models;

namespace RescueRobotsCar.Driving.Maps.MapObjects
{
    public record MapObject
    {
        public required string ID { get; init; }
        public required string Color { get; init; }
        public required int Number { get; init; }
        public required Position Position { get; init; }
        public required bool Collect { get; init; }
    }
}
