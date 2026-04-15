namespace RescueRobotsCar.Driving.Maps.Models
{
    public record MapPiece
    {
        public required Position Position { get; init; }
        public required int Type { get; init; }
        public required bool Drivable { get; init; }
        public required List<Exit> Exits { get; init; }
    }
}
