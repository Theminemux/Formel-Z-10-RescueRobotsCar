using RescueRobotsCar.Driver.Motor;
using RescueRobotsCar.Driving.Maps;

namespace RescueRobotsCar.Services
{
    public class StartupService : IHostedService
    {
        private readonly SystemStateService _systemStateService;
        private readonly MotorDriver _motors;
        private readonly MapProvider _mapProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHostApplicationLifetime _hostLifetime;

        public StartupService(
            MotorDriver motors, 
            MapProvider mapProvider, 
            IHttpClientFactory httpClientFactory,
            IHostApplicationLifetime hostLifetime,
            SystemStateService systemStateService)
        {
            _motors = motors;
            _mapProvider = mapProvider;
            _httpClientFactory = httpClientFactory;
            _hostLifetime = hostLifetime;
            _systemStateService = systemStateService;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            // Login zum Orange Pi
            const string orangePiUrl = "http://192.168.10.10";
            using var httpClient = _httpClientFactory.CreateClient("api");

            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine($"Args: {string.Join(", ", args)}");

            if (!args.Contains("--skip-login"))
            {
                await _systemStateService.SetOrangePiIp(orangePiUrl);

                string link = $"{orangePiUrl}/api/register/?device=rescuecar";

                Console.WriteLine($"Trying to register with link: {link}");

                var loginResponse = await httpClient.GetAsync(link, ct);
                if (!loginResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Login failed");
                    Console.WriteLine($"Status Code: {loginResponse.StatusCode}. Content: {loginResponse.Content.ToString()}");
                    _hostLifetime.StopApplication();
                    return;
                }
                {
                    Console.WriteLine("Login was successful");
                    await _systemStateService.SetLoggedIn(true);
                }
            }

            // Motoren initialisieren
            if (!args.Contains("--skip-motors"))
            {
                try
                {
                    _motors.InitializeMotors();
                    Console.WriteLine("Motors initialized successfully.");
                    await _systemStateService.SetMotorsInitialized(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing motors: {ex.Message}");
                    _hostLifetime.StopApplication();
                    return;
                }
            }

            // Map laden
            if (!args.Contains("--skip-maploading"))
            {
                await _mapProvider.ImportMapFromJsonAsync();
                if (_mapProvider.Track.Count == 0)
                {
                    Console.WriteLine("Die Map enthält keine Elemente. Beende Programm...");
                    _hostLifetime.StopApplication();
                    return;
                }
                Console.WriteLine($"Map loaded successfully with {_mapProvider.Track.Count} elements.");
                await _systemStateService.SetMapLoaded(true);
            }

            // Prüfe Verbindung zum ESP32
            if (!args.Contains("--skip-esp32-check") && _systemStateService.IsLoggedIn)
            {
                if (_systemStateService.OrangePiIp is null)
                {
                    Console.WriteLine("OrangePi IP is unknown. Can't test esp32 connection.");
                }
                var response = await httpClient.GetAsync($"{_systemStateService.OrangePiIp}/api/getip/?device=rescuecar-esp32", ct);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("ESP32 connection failed");
                    Console.WriteLine($"Status Code: {response.StatusCode}. Content: {await response.Content.ReadAsStringAsync(ct)}");
                    _hostLifetime.StopApplication();
                    return;
                }
                {
                    Console.WriteLine("ESP32 connection was successful");
                    await _systemStateService.SetEsp32Connected(true);
                }
            }
        }

        public async Task StopAsync(CancellationToken ct) { }

        private static int GetArgumentValue(string[] args, string argumentName, int defaultValue)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith($"{argumentName}="));
            if (arg != null && int.TryParse(arg.Split('=')[1], out int value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
