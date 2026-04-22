namespace RescueRobotsCar.Services
{
    public class CollectedObjectsManager
    {
        private List<string> collectedObjects;
        public IReadOnlyList<string> CollectedObjects => collectedObjects.AsReadOnly();

        private readonly SemaphoreSlim _collectionLock = new SemaphoreSlim(1, 1);

        private readonly RFIDTagConverter _rfidTag;

        public CollectedObjectsManager(RFIDTagConverter rfidTag)
        {
            _rfidTag = rfidTag;
            _rfidTag.OnPositionChanged += async (sender, e) => await RfidTag_OnPositionChanged(sender, e);
            collectedObjects = new List<string>();
        }

        private async Task RfidTag_OnPositionChanged(object? sender, RFIDTag e)
        {
            if (e is RFIDObject rfidObject)
            {
                await AddCollectedObject(rfidObject.Name);
            }
        }

        public async Task AddCollectedObject(string objectName)
        {
            await _collectionLock.WaitAsync();
            try
            {
                if (!collectedObjects.Contains(objectName))
                {
                    collectedObjects.Add(objectName);
                    Console.WriteLine($"Object collected: {objectName}");
                }
            }
            finally
            {
                _collectionLock.Release();
            }
        }

        public async Task<IReadOnlyList<string>> GetCollectedObjectsAsync()
        {
            await _collectionLock.WaitAsync();
            try
            {
                return collectedObjects.AsReadOnly();
            }
            finally
            {
                _collectionLock.Release();
            }
        }
    }
}
