namespace RescueRobotsCar.Driving.Maps.MapObjects
{
    public record MapObjectsContainer
    {
        public required List<MapObject> Objects { get; init; }
    }
}
