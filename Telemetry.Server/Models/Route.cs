namespace Telemetry.Server.Models;

public class Route
{
    public int Id { get; set; }
    public string RouteName { get; set; } 
    
    public ICollection<Waypoint> Waypoints { get; set; }
    public ICollection<TrainStop> TrainStops { get; set; }
}