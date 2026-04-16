using RescueRobotsCar.Driving.Maps.MapObjects;

namespace RescueRobotsCar.Driving.Maps.Status
{
    public class StatusProvider
    {
        private readonly MapObjectsProvider _mapObjectsProvider;

        public StatusProvider(MapObjectsProvider mapObjectsProvider)
        {
            _mapObjectsProvider = mapObjectsProvider;
        }

        public async Task<StatusContainer> GetStatus()
        {
            MapObjectsContainer? mapObjects = await _mapObjectsProvider.GetMapObjectsAsync();
            return StatusContainer.Default with
            {
                MapObjects = mapObjects,
            };
        }
    }
}
