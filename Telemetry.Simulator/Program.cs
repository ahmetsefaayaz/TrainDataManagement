using System.Net.Sockets;

namespace Telemetry.Simulator;

class Program
{
    static async Task Main(string[] args)
    {
        var routes = new List<RailwayRoute>
        {
            new RailwayRoute {
                RouteName = "Ankara-Eskişehir YHT",
                // Ankara Gar -> Sincan -> Temelli -> Polatlı YHT -> Beylikova -> Alpu -> Eskişehir Gar
                Waypoints = new List<(double, double)> { 
                    (39.9360, 32.8465), 
                    (39.9566, 32.5765), 
                    (39.7369, 32.3789), 
                    (39.5786, 31.9847), 
                    (39.6833, 31.2000), 
                    (39.7667, 30.9667), 
                    (39.7719, 30.5186) 
                }
            },
            new RailwayRoute {
                RouteName = "Zonguldak-Karabük Hattı",
                // Zonguldak Gar -> Çatalağzı -> Filyos -> Saltukova -> Çaycuma -> Gökçebey -> Yenice -> Karabük Gar
                Waypoints = new List<(double, double)> { 
                    (41.4564, 31.7987), 
                    (41.5244, 31.8966), 
                    (41.5583, 32.0169), 
                    (41.5089, 32.0911),
                    (41.4286, 32.0792), 
                    (41.3031, 32.1469), 
                    (41.1956, 32.3300),
                    (41.1956, 32.6226) 
                }
            },
            new RailwayRoute {
                RouteName = "İzmir-Manisa Hattı",
                // İzmir Basmane -> Karşıyaka -> Çiğli -> Menemen -> Emiralem -> Muradiye -> Manisa Gar
                Waypoints = new List<(double, double)> { 
                    (38.4237, 27.1428),
                    (38.4442, 27.1799),
                    (38.4747, 27.1591),
                    (38.4689, 27.1279),
                    (38.4590, 27.1170), 
                    (38.4883, 27.0581), 
                    (38.5276, 27.0325),
                    (38.6288, 27.1691),
                    (38.6175, 27.1836),
                    (38.6469, 27.2484),
                    (38.6019, 27.0725), 
                    (38.6367, 27.1583),
                    (38.6256, 27.3486),
                    (38.6140, 27.4296) 
                }
            }
        };

        int locomotiveCount = 10;
        Task[] tasks = new Task[locomotiveCount];

        for (short i = 1; i <= locomotiveCount; i++)
        {
            short locoId = i;
            
            RailwayRoute assignedRoute = routes[i % routes.Count];
            
            tasks[i - 1] = Task.Run(() => StartLocomotiveAsync(locoId, assignedRoute));
        }

        await Task.WhenAll(tasks);
    }

    static async Task StartLocomotiveAsync(short locomotiveId, RailwayRoute route)
    {
        string serverIp = "127.0.0.1";
        int port = 5000;
        
        var engine = new LocomotiveEngine(locomotiveId, route);
        
        while (true)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                using NetworkStream stream = client.GetStream();
                
                Console.WriteLine($"[BİLGİ] Lokomotif {locomotiveId} ({route.RouteName}) merkeze bağlandı.");

                while (client.Connected)
                {
                    if(route.RouteName.Contains("YHT"))
                        engine.Move(0.0006); //Yaklaşık 240 km/s
                    else
                        engine.Move(0.0002); //yaklaşık 80km/s
                    byte[] payload = engine.GetPayload();

                    await stream.WriteAsync(payload, 0, payload.Length);
                    
                    Console.WriteLine(engine.GetStatusLog());

                    await Task.Delay(100);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"[UYARI] Lokomotif {locomotiveId} bağlantısı koptu. Yeniden deneniyor...");
                await Task.Delay(3000);
            }
        }
    }
}