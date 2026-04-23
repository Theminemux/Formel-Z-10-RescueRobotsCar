namespace RescueRobotsCar.Services
{
    public class PositionService
    {
        private Position _currentPosition = Position.Default;

        private readonly RFIDTagConverter _rfidTag;
        private readonly SemaphoreSlim _positionLock = new SemaphoreSlim(1, 1);
        private readonly IReadOnlyDictionary<char, int> keyValuePairs;

        public PositionService(RFIDTagConverter rfidTag)
        {
            _rfidTag = rfidTag;
            _rfidTag.OnPositionChanged += async (sender, e) => await RfidTag_OnPositionChanged(sender, e);

            keyValuePairs = new Dictionary<char, int>
            {
                { 'A', 1 },
                { 'B', 2 },
                { 'C', 3 },
                { 'D', 4 },
                { 'E', 5 },
                { 'F', 6 },
                { 'G', 7 },
                { 'H', 8 },
                { 'I', 9 },
                { 'J', 10 }
            };
        }

        private async Task RfidTag_OnPositionChanged(object? sender, RFIDTag e)
        {
            try
            {
                if (e is RFIDCoordinate coordinate)
                {
                    string rawCoordinate = coordinate.Coordinate;
                    int x, y, lv;

                    // Get lv
                    if (rawCoordinate.Length == 2)
                        lv = 0;
                    else if (rawCoordinate.Length == 3)
                    {
                        lv = 1;
                        rawCoordinate = rawCoordinate.Substring(1, 2);
                    }
                    else
                        throw new Exception("Irgend wer hat mit den RFID Tags gepfuscht. Das sollte ne Koordinate sein eigentlich. ISSES ABER NICHT. LV falsch");

                    // get x
                    if (!keyValuePairs.TryGetValue(rawCoordinate[0], out x))
                        throw new Exception("Irgend wer hat mit den RFID Tags gepfuscht. Das sollte ne Koordinate sein eigentlich. ISSES ABER NICHT. X falsch");

                    // get y
                    if (!int.TryParse(rawCoordinate[1].ToString(), out y))
                        throw new Exception("Irgend wer hat mit den RFID Tags gepfuscht. Das sollte ne Koordinate sein eigentlich. ISSES ABER NICHT. Y falsch");
                    var newPosition = new Position { X = x, Y = y, Lv = lv };
                    await SetPositionAsync(newPosition);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing RFID coordinate: {ex.Message}");
            }
        }

        public async Task<Position> GetPositionAsync()
        {
            await _positionLock.WaitAsync();
            try
            {
                return _currentPosition;
            }
            finally
            {
                _positionLock.Release();
            }
        }

        private async Task SetPositionAsync(Position newPosition)
        {
            await _positionLock.WaitAsync();
            try
            {
                Console.WriteLine($"Position updated: X={newPosition.X}, Y={newPosition.Y}, Lv={newPosition.Lv}");
                _currentPosition = newPosition;
            }
            finally
            {
                _positionLock.Release();
            }
        }
    }

    public record Position
    {
        public required int X { get; init; }
        public required int Y { get; init; }
        public required int Lv { get; init; }

        public static Position Default => new Position { X = 0, Y = 0, Lv = 0 };
    }
}
