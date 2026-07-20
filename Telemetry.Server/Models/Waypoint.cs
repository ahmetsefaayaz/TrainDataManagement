namespace Telemetry.Server.Models;

public class Waypoint
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int OrderIndex { get; set; }
    
    public int RouteId { get; set; }
    public Route Route { get; set; }
}