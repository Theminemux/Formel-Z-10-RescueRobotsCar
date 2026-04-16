using RescueRobotsCar.Driving.Maps.MapObjects;

namespace RescueRobotsCar.Services
{
    public class StatusProvider
    {
        private readonly MapObjectsProvider _mapObjectsProvider;
        private readonly CollectedObjectsManager _objectManager;
        private readonly StatusSetter _statusSetter;

        public StatusProvider(MapObjectsProvider mapObjectsProvider, CollectedObjectsManager objectManager, StatusSetter statusSetter)
        {
            _mapObjectsProvider = mapObjectsProvider;
            _objectManager = objectManager;
            _statusSetter = statusSetter;
        }

        public async Task<StatusContainer> GetStatusAsync()
        {
            MapObjectsContainer? mapObjects = await _mapObjectsProvider.GetMapObjectsAsync();
            int status = await _statusSetter.GetStatusAsync();

            return StatusContainer.Default with
            {
                MapObjects = mapObjects,
                CollectedObjects = _objectManager.CollectedObjects,
                Status = status,
            };
        }
    }
}
