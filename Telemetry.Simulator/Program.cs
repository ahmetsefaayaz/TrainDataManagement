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
                    (39.9432, 32.5026),
                    (39.7369, 32.3789), 
                    (39.6832, 32.2338),
                    (39.5786, 31.9847), 
                    (39.5857, 31.9866),
                    (39.6656, 31.9102),
                    (39.6970, 31.6535),
                    (39.7465, 31.5735),
                    (39.7352, 31.5222),
                    (39.6931, 31.4625),
                    (39.6901, 31.3770),
                    (39.6833, 31.2000),
                    (39.7667, 30.9667), 
                    (39.8062, 30.7482),
                    (39.7719, 30.5186) 
                }
            },
            new RailwayRoute {
                RouteName = "Zonguldak-Karabük Hattı",
                // Zonguldak Gar -> Çatalağzı -> Filyos -> Saltukova -> Çaycuma -> Gökçebey -> Yenice -> Karabük Gar
                Waypoints = new List<(double, double)> { 
                    (41.4564, 31.7987), 
                    (41.4725, 31.8124),
                    (41.4986, 31.8744),
                    (41.5170, 31.9041),
                    (41.5225, 31.9343),
                    (41.5399, 31.9657),
                    (41.5462, 32.0031),
                    (41.5583, 32.0169),
                    (41.5658, 32.0447),
                    (41.5505, 32.0753),
                    (41.5324, 32.0929),
                    (41.5089, 32.0911),
                    (41.4897, 32.0911),
                    (41.4704, 32.0939),
                    (41.4473, 32.0960),
                    (41.4286, 32.0792),
                    (41.3329, 32.0938),
                    (41.3115, 32.1026),
                    (41.3031, 32.1469), 
                    (41.2895, 32.1605),
                    (41.2644, 32.1624),
                    (41.2340, 32.1816),
                    (41.2384, 32.1996),
                    (41.2167, 32.2323),
                    (41.2195, 32.2480),
                    (41.1956, 32.3300),
                    (41.2180, 32.2975),
                    (41.2131, 32.3118),
                    (41.1996, 32.3392),
                    (41.2131, 32.3613),
                    (41.2010, 32.4189),
                    (41.1578, 32.4684),
                    (41.1710, 32.5016),
                    (41.1541, 32.5374),
                    (41.1695, 32.5830),
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
                    (38.4921, 27.0627), 
                    (38.5276, 27.0325),
                    (38.6029, 27.0766),
                    (38.6245, 27.1477),
                    (38.6175, 27.1836),
                    (38.6469, 27.2484),
                    (38.6511, 27.3285),
                    (38.6213, 27.4344) 
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
                    if(route.RouteName.ToUpper().Contains("YHT"))
                        engine.Move(0.0006); //Yaklaşık 240 km/s
                    else
                        engine.Move(0.0002); //yaklaşık 80km/s
                    byte[] payload = engine.GetPayload();

                    await stream.WriteAsync(payload, 0, payload.Length);
                    
                    Console.WriteLine(engine.GetStatusLog());
                    if (engine.IsFinished)
                        break;

                    await Task.Delay(100);
                }
                if(engine.IsFinished)
                    break;
            }
            catch (Exception)
            {
                Console.WriteLine($"[UYARI] Lokomotif {locomotiveId} bağlantısı koptu. Yeniden deneniyor...");
                await Task.Delay(3000);
            }
        }
    }
}