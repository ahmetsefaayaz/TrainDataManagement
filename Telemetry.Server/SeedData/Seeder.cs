using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Models;
using Route = Telemetry.Server.Models.Route;

namespace Telemetry.Server.SeedData;

public static class Seeder
{
    public static async Task Initialize(AppDbContext dbContext)
    {
        if (!await dbContext.Routes.AnyAsync(r => r.RouteName == "Izmir-Manisa"))
        {
            string izmirFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "Izmir-Manisa.json");
            var izmirManisaRoute = await AddJsonRoute("Izmir-Manisa", izmirFilePath);
            await dbContext.Routes.AddAsync(izmirManisaRoute);
            Console.WriteLine("[Seed] Izmir-Manisa eklendi.");
        }
    
        if (!await dbContext.Routes.AnyAsync(r => r.RouteName == "Ankara-Eskisehir YHT"))
        {
            string ankaraFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "Ankara-Eskisehir.json");
            var ankaraEskisehirRoute = await AddJsonRoute("Ankara-Eskisehir YHT", ankaraFilePath);
            await dbContext.Routes.AddAsync(ankaraEskisehirRoute);
            Console.WriteLine("[Seed] Ankara-Eskisehir YHT eklendi.");
        }
        if (!await dbContext.Routes.AnyAsync(r => r.RouteName == "Zonguldak-Karabuk"))
        {
            string zonguldakaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "Zonguldak-Karabuk.json");
            var zonguldakKarabukRoute = await AddJsonRoute("Zonguldak-Karabuk", zonguldakaFilePath);
            await dbContext.Routes.AddAsync(zonguldakKarabukRoute);
            Console.WriteLine("[Seed] Ankara-Eskisehir YHT eklendi.");
        }
        await dbContext.SaveChangesAsync();
        
    }
    
    public static async Task<Route> AddJsonRoute(string routeName, string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {filePath} does not exist.");
        }
        string jsonContent = await File.ReadAllTextAsync(filePath);
        using JsonDocument doc = JsonDocument.Parse(jsonContent);
        var features = doc.RootElement.GetProperty("features");
        if (features.GetArrayLength() == 0) throw new Exception("No features found.");

        var coordinates = features[0]
            .GetProperty("geometry")
            .GetProperty("coordinates");
        
        var newRoute = new Route 
        { 
            RouteName = routeName,
            Waypoints = new List<Waypoint>() 
        };
        int currentIndex = 0;
        foreach (var coord in coordinates.EnumerateArray())
        {
            double lon = coord[0].GetDouble();
            double lat = coord[1].GetDouble();
            
            newRoute.Waypoints.Add(new Waypoint 
            { 
                Latitude = lat, 
                Longitude = lon,
                OrderIndex = currentIndex
            });
            currentIndex++;
        }
        return newRoute;
    }
}