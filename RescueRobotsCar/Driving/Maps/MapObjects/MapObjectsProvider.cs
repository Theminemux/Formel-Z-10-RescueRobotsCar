namespace RescueRobotsCar.Driving.Maps.MapObjects
{
    public class MapObjectsProvider
    {
        private MapObjectsContainer? _mapObjectsContainer;

        private readonly SemaphoreSlim _mapLock = new SemaphoreSlim(1, 1);

        public async Task UpdateMapObjects(MapObjectsContainer? newMapObjects)
        {
            await _mapLock.WaitAsync();
            try
            {
                _mapObjectsContainer = newMapObjects;
            }
            finally
            {
                _mapLock.Release();
            }
        }
        public async Task<MapObjectsContainer?> GetMapObjectsAsync()
        {
            await _mapLock.WaitAsync();
            try
            {
                return _mapObjectsContainer;
            }
            finally
            {
                _mapLock.Release();
            }
        }
    }
}
