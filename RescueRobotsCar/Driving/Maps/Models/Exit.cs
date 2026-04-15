namespace RescueRobotsCar.Driving.Maps.Models
{
    public record Exit
    {
        public required int Direction { get; init; }
        public required Position PiecePosition { get; init; }
    }
}
