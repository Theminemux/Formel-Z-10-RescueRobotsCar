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

            ip += ":8001"; // Nur weil Mac Book gerade genutzt wird und nicht der orange pi. sonst ohne :8000

            string link = $"http://{ip}/api/register/?device=rescuecar";

            Console.WriteLine($"Trying to register with link: {link}");

            var loginResponse = await httpClient.GetAsync(link, ct);
            if (!loginResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Login failed");
                Console.WriteLine($"Status Code: {loginResponse.StatusCode}. Content: {loginResponse.Content.ToString()}");
                Environment.Exit(0);
            }
            {
                Console.WriteLine("Login was successful");
            }
        }

        public async Task StopAsync(CancellationToken ct) { }
    }
}
