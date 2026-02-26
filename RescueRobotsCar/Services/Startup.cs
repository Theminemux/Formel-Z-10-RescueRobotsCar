namespace RescueRobotsCar.Services
{
    public class Startup : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            // Login zum Orange Pi
            const string url = "http://5.175.245.160:8300/text";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url, ct);
            var ip = await response.Content.ReadAsStringAsync();

            var loginResponse = await httpClient.GetAsync($"http://{ip}/api/register/?device=rescuecar", ct);
            if (!loginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Login failed");
                Environment.Exit(0);
            }
        }

        public async Task StopAsync(CancellationToken ct) { }
    }
}
