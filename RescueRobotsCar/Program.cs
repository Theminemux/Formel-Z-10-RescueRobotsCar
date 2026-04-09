using RescueRobotsCar.Driver.RFID;
using RescueRobotsCar.Driver.IRSensor;
using RescueRobotsCar.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<RFIDRC522Driver>();
builder.Services.AddSingleton<IRSensor>();
//builder.Services.AddHostedService<Mpu6050Driver>();
builder.Services.AddHostedService<RFIDRC522Driver>();
builder.Services.AddHostedService<IRSensor>();
builder.Services.AddHostedService<Startup>();

//builder.Services.AddTransient<Logger>();
//builder.Services.AddSingleton<NavigatorService>();
//builder.Services.AddSingleton<MotorConfig>();
//builder.Services.AddSingleton<MPU6050Config>();
//builder.Services.AddSingleton<MotorDriver>();
//builder.Services.AddSingleton<Mpu6050Driver>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    //logger.Log("Testing motors...", Logger.Severity.Info);
    //motor.InitializeMotors();
    //motor.TestAllMotors();
    //logger.Log("Motors initialized and tested.", Logger.Severity.Info);
}
catch (Exception ex)
{
    Console.WriteLine($"Error whilst testing code: {ex}");
    //logger.Log($"Error initializing motors: {ex.Message}", Logger.Severity.Error);
    //motor.StopAllMotors();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.MapControllers();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Rescue Robots Car started.");

try
{
    await app.RunAsync("http://0.0.0.0");
}
catch (Exception ex)
{
    Console.WriteLine($"Error running the application: {ex.Message}");
}