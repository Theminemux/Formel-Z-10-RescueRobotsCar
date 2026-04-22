using RescueRobotsCar.Driver.RFID;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.MPU6050;
using RescueRobotsCar.Driver.Motor;
using RescueRobotsCar.Services;
using RescueRobotsCar.Driving.Sensors;
using RescueRobotsCar.Driving.Maps;
using RescueRobotsCar.Driving.Maps.MapObjects;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<Mpu6050Driver>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<Mpu6050Driver>());

builder.Services.AddSingleton<RFIDRC522Driver>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RFIDRC522Driver>());

builder.Services.AddSingleton<LineSensor>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<LineSensor>());

builder.Services.AddSingleton<Compass>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<Compass>());

builder.Services.AddHostedService<StartupService>();

builder.Services.AddSingleton<MotorConfig>();
builder.Services.AddSingleton<MPU6050Config>();
builder.Services.AddSingleton<MotorDriver>();
builder.Services.AddSingleton<MapProvider>();
builder.Services.AddSingleton<SystemStateService>();
builder.Services.AddSingleton<MapObjectsProvider>();
builder.Services.AddSingleton<StatusProvider>();
builder.Services.AddSingleton<CollectedObjectsManager>();
builder.Services.AddSingleton<StatusSetter>();
builder.Services.AddSingleton<LineFollower>();
builder.Services.AddSingleton<RFIDTagConverter>();
builder.Services.AddSingleton<PositionService>();

builder.Services.AddHttpClient("api")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
        });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.MapControllers();

app.UseAuthorization();

Console.WriteLine("Rescue Robots Car started.");

try
{
    await app.RunAsync("http://0.0.0.0");
}
catch (Exception ex)
{
    Console.WriteLine($"Error running the application: {ex.Message}");
}