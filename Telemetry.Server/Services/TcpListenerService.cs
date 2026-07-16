using System.Net;
using System.Net.Sockets;
using Telemetry.Server.Data;
using Telemetry.Server.Models;

namespace Telemetry.Server.Services;

public class TcpListenerService : BackgroundService
{
    private readonly ILogger<TcpListenerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TcpListenerService(ILogger<TcpListenerService> logger,  IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int port = 5000;
        
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        
        _logger.LogInformation($"[SERVER] TCP Dinleyici {port} portunda başlatıldı. Lokomotifler bekleniyor...");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
            TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
            
            _ = HandleClientAsync(client, stoppingToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        EndPoint clientEndPoint = client.Client.RemoteEndPoint;
        _logger.LogInformation($"[BAĞLANTI] Yeni bir lokomotif bağlandı: {clientEndPoint}");

        try
        {
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[22];

            while (client.Connected && !stoppingToken.IsCancellationRequested)
            {
                
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);

                if (bytesRead == 0) break; 
                if (bytesRead == 22)
                {
                    //Ramdeki verileri tekrardan normal hale çeviriyoruz
                    short locomotiveId = BitConverter.ToInt16(buffer, 0); 
                    double latitude = BitConverter.ToDouble(buffer, 2);   
                    double longitude = BitConverter.ToDouble(buffer, 10); 
                    float speed = BitConverter.ToSingle(buffer, 18);      
                    
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var newRecord = new TrainLocationRecord
                        {
                            LocomotiveId = locomotiveId,
                            Latitude = latitude,
                            Longitude = longitude,
                            Speed = speed,
                            RecordedAt = DateTime.UtcNow
                        };
                        dbContext.TrainLocations.Add(newRecord);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    
                    _logger.LogInformation($"[RX] Gelen Veri -> Loko ID: {locomotiveId} | Lat: {latitude:F4} | Lon: {longitude:F4} | Hız: {speed} km/h");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[HATA] Lokomotif iletişimi koptu: {ex.Message}");
        }
        finally
        {
            client.Close();
            _logger.LogInformation($"[KOPMA] Lokomotif ayrıldı: {clientEndPoint}");
        }
    }
}