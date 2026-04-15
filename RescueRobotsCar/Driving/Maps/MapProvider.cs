using RescueRobotsCar.Driving.Maps.Models;
using System.Text.Json;

namespace RescueRobotsCar.Driving.Maps
{
    public class MapProvider
    {
        private MapData? _mapData;
        public List<MapPiece> Track => _mapData?.Track ?? [];

        public MapProvider()
        {
            _mapData = new MapData();
        }

        public async Task ImportMapFromJsonAsync()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "map.json");
                string jsonContent = await File.ReadAllTextAsync(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _mapData = JsonSerializer.Deserialize<MapData>(jsonContent, options);
                if (Track.Count == 0)
                    Console.WriteLine("Die Map wurde erfolgreich geladen, enthält jedoch keine Elemente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Map: {ex.Message}");
                _mapData = null;
            }
        }
    }
}
