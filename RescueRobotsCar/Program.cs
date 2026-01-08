using RescueRobotsCar.Services;
using RescueRobotsCar.Driver.Motor;
using RescueRobotsCar.Driver.MPU6050;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<Mpu6050Driver>();

builder.Services.AddTransient<Logger>();
builder.Services.AddSingleton<NavigatorService>();
builder.Services.AddSingleton<MotorConfig>();
builder.Services.AddSingleton<MPU6050Config>();
builder.Services.AddSingleton<MotorDriver>();
builder.Services.AddSingleton<Mpu6050Driver>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logger = app.Services.GetRequiredService<Logger>();
var motor = app.Services.GetRequiredService<MotorDriver>();
var mpu = app.Services.GetRequiredService<Mpu6050Driver>();

try
{
    logger.Log("Testing motors...", Logger.Severity.Info);
    motor.InitializeMotors();
    motor.TestAllMotors();
    logger.Log("Motors initialized and tested.", Logger.Severity.Info);
}
catch (Exception ex)
{
    logger.Log($"Error initializing motors: {ex.Message}", Logger.Severity.Error);
    motor.StopAllMotors();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.MapControllers();

app.UseAuthorization();

app.MapControllers();

app.Run();