using System.Net.Http.Json;
using System.Net.Sockets;
using Telemetry.Simulator.Dtos;

namespace Telemetry.Simulator;

class Program
{
    static async Task Main(string[] args)
    {
        var routes = new List<RouteDto>();
        
        using (HttpClient client = new HttpClient())
        {
            try
            {
                routes = await client.GetFromJsonAsync<List<RouteDto>>("http://localhost:5215/api/routes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ex.Message}");
            }
        }
        if (routes == null || routes.Count == 0) return;

        int locomotiveCount = 10;
        Task[] tasks = new Task[locomotiveCount];

        for (short i = 1; i <= locomotiveCount; i++)
        {
            short locoId = i;
            RouteDto assignedRoute = routes[i % routes.Count];
            
            tasks[i - 1] = Task.Run(() => StartLocomotiveAsync(locoId, assignedRoute));
        }

        await Task.WhenAll(tasks);
    }

    static async Task StartLocomotiveAsync(short locomotiveId, RouteDto route)
    {
        string serverIp = "127.0.0.1";
        int port = 5000;
        
        
        var engine = new LocomotiveEngine(locomotiveId, route, route.TrainStops);
        
        while (true)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                using NetworkStream stream = client.GetStream();
                
                while (client.Connected)
                {
                    if(route.RouteName.ToUpper().Contains("YHT"))
                        engine.Move(0.0006); 
                    else
                        engine.Move(0.0002); 
                        
                    bool inBlindZone = IsInBlindZone(engine.CurrentLat, engine.CurrentLon);
                    
                    if (!inBlindZone)
                    {
                        byte[] payload = engine.GetPayload();
                        await stream.WriteAsync(payload, 0, payload.Length);
                        Console.WriteLine(engine.GetStatusLog());
                    }
                    else 
                    {
                        Console.WriteLine($"[SİNYAL YOK] Lokomotif {locomotiveId} kör noktada ilerliyor...");
                    }

                    if (engine.IsFinished)
                        break;
                        
                    await Task.Delay(1000);
                }
                
                if(engine.IsFinished)
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UYARI] Lokomotif {locomotiveId} bağlantısı koptu. Yeniden deneniyor...");
                await Task.Delay(3000);
            }
        }
    }
    
    public static bool IsInBlindZone(double currentLat, double currentLon)
    {
        var blindSpots = new List<(double MinLat, double MaxLat, double MinLon, double MaxLon)>
        {
            (39.5439, 39.6912, 31.8739, 32.1070), 
            (38.5951, 38.6639, 27.1514, 27.2517),
        };
        
        foreach (var zone in blindSpots)
        {
            if (currentLat >= zone.MinLat && currentLat <= zone.MaxLat && 
                currentLon >= zone.MinLon && currentLon <= zone.MaxLon)
            {
                return true;
            }
        }

        return false;
    }
}