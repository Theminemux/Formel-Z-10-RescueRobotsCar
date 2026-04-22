using RescueRobotsCar.Driver.RFID;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RescueRobotsCar.Services
{
    public class RFIDTagConverter
    {
        public event EventHandler<RFIDTag>? OnPositionChanged;

        private readonly RFIDRC522Driver _rfidDriver;

        public RFIDTagConverter(RFIDRC522Driver rfidDriver)
        {
            _rfidDriver = rfidDriver;
            _rfidDriver.OnTagRead += RfidDriver_OnTagRead;
        }

        private void ChangePosition(RFIDTag tag)
        {
            OnPositionChanged?.Invoke(this, tag);
        }

        private void RfidDriver_OnTagRead(object? sender, RFIDCardData e)
        {
            Console.WriteLine($"RFID Tag read with data: {e.Data}");
            var rfidData = JsonSerializer.Deserialize<RFIDData>(e.Data);
            if (rfidData is null)
            {
                Console.WriteLine($"Failed to deserialize RFID data: {e.Data}");
                return;
            }
            if (rfidData.TagType == 0)
            {
                ChangePosition(new RFIDObject { Name = rfidData.Data });
            }
            else if (rfidData.TagType == 1)
            {
                ChangePosition(new RFIDCoordinate { Coordinate = rfidData.Data });
            }
        }
    }

    public class RFIDData
    {
        [JsonPropertyName("tag_type")]
        public required int TagType { get; init; }
        [JsonPropertyName("data")]
        public required string Data { get; init; }
    }
    public class RFIDTag : EventArgs { }
    public class RFIDObject : RFIDTag
    {
        public required string Name { get; init; }
    }
    public class RFIDCoordinate : RFIDTag
    {
        public required string Coordinate { get; init; }
    }
}
