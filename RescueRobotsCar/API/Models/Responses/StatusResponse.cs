namespace RescueRobotsCar.API.Models.Responses
{
    public record StatusResponse
    {
        public required int X { get; init; }
        public required int Y { get; init; }
        public required int Level { get; init; }

        public required int DrivingState { get; init; }
    }
}
