using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Interfaces;
using Telemetry.Server.SeedData;
using Telemetry.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IWaypointService, WaypointService>();
builder.Services.AddScoped<ITrainStopService, TrainStopService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true); 
    });
});


builder.Services.AddHostedService<TcpListenerService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); 
        
        Seeder.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed işlemi sırasında hata oluştu: {ex.Message}");
    }
}

app.Run();