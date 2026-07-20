using Telemetry.Server.Data;
using Telemetry.Server.Models;
using Route = Telemetry.Server.Models.Route;

namespace Telemetry.Server.SeedData;

public static class Seeder
{
    public static void Initialize(AppDbContext dbContext)
    {
        if (dbContext.Routes.Any()) return;
        
        var ankaraEskisehirCoords = new List<(double lat, double lon)> 
        { 
            (39.9351, 32.8435), (39.9648, 32.5833), (39.9432, 32.5026),
            (39.7321, 32.3528), (39.6832, 32.2338), (39.5857, 32.1417),
            (39.5786, 31.9847), (39.5857, 31.9866), (39.6656, 31.9102),
            (39.6970, 31.6535), (39.7465, 31.5735), (39.7352, 31.5222),
            (39.6931, 31.4625), (39.6901, 31.3770), (39.7040, 31.1892),
            (39.7680, 30.9540), (39.8062, 30.7482), (39.7793, 30.5034) 
        };
        
        var route1 = new Route
        {
            RouteName = "Ankara-Eskişehir YHT",
            Waypoints = ankaraEskisehirCoords.Select((c, index) => new Waypoint { Latitude = c.lat, Longitude = c.lon, OrderIndex = index }).ToList(),
            TrainStops = new List<TrainStop>
            {
                new TrainStop { StationName = "Polatlı YHT", Latitude = 39.5857, Longitude = 32.1417 },
                new TrainStop { StationName = "Eskişehir Gar", Latitude = 39.7793, Longitude = 30.5034 }
            }
        };

        var zonguldakKarabukCoords = new List<(double lat, double lon)> 
        { 
            (41.4564, 31.7987), (41.4725, 31.8124), (41.4986, 31.8744), (41.5170, 31.9041),
            (41.5225, 31.9343), (41.5399, 31.9657), (41.5462, 32.0031), (41.5583, 32.0169),
            (41.5658, 32.0447), (41.5505, 32.0753), (41.5324, 32.0929), (41.5089, 32.0911),
            (41.4897, 32.0911), (41.4704, 32.0939), (41.4473, 32.0960), (41.4239, 32.0954),
            (41.3329, 32.0938), (41.3115, 32.1026), (41.3031, 32.1469), (41.2895, 32.1605),
            (41.2644, 32.1624), (41.2340, 32.1816), (41.2384, 32.1996), (41.2167, 32.2323),
            (41.2195, 32.2480), (41.2180, 32.2975), (41.2052, 32.3191), (41.1996, 32.3392),
            (41.2131, 32.3613), (41.2010, 32.4189), (41.1578, 32.4684), (41.1710, 32.5016),
            (41.1541, 32.5374), (41.1695, 32.5830), (41.1952, 32.6110) 
        };

        var route2 = new Route
        {
            RouteName = "Zonguldak-Karabük Hattı",
            Waypoints = zonguldakKarabukCoords.Select((c, index) => new Waypoint { Latitude = c.lat, Longitude = c.lon, OrderIndex = index }).ToList(),
            TrainStops = new List<TrainStop>
            {
                new TrainStop { StationName = "Karabük Gar", Latitude = 41.1952, Longitude = 32.6110 }
            }
        };

        var izmirManisaCoords = new List<(double lat, double lon)> 
        { 
            (38.4237, 27.1428), (38.4442, 27.1799), (38.4747, 27.1591),
            (38.4689, 27.1279), (38.4590, 27.1170), (38.4921, 27.0627), 
            (38.5276, 27.0325), (38.6029, 27.0766), (38.6245, 27.1477),
            (38.6175, 27.1836), (38.6469, 27.2484), (38.6511, 27.3285), (38.6213, 27.4344) 
        };

        var route3 = new Route
        {
            RouteName = "İzmir-Manisa Hattı",
            Waypoints = izmirManisaCoords.Select((c, index) => new Waypoint { Latitude = c.lat, Longitude = c.lon, OrderIndex = index }).ToList(),
            TrainStops = new List<TrainStop>
            {
                new TrainStop { StationName = "Manisa Gar", Latitude = 38.6213, Longitude = 27.4344 }
            }
        };

        dbContext.Routes.AddRange(route1, route2, route3);
        
        dbContext.SaveChanges();
        
        
    }
}