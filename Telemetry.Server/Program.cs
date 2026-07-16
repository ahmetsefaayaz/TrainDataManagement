using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Interfaces;
using Telemetry.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true); // Şimdilik tüm kaynaklara izin verelim
    });
});


builder.Services.AddHostedService<TcpListenerService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => "Telemetri Sunucusu Çalışıyor. Arka planda 5000 portu dinleniyor...");

// app.UseHttpsRedirection();

app.Run();