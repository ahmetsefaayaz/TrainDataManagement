namespace Telemetry.Server.Models;

public class TrainStop
{
    public int Id { get; set; }
    
    public string StationName { get; set; }  
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
}