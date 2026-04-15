namespace RescueRobotsCar.Driving.Maps.Models
{
    public record Position
    {
        public required int X { get; init; }
        public required int Y { get; init; }
        public required int Level { get; init; }
    }
}
