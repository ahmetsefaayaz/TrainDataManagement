namespace Telemetry.Server.Dtos;

public class RouteDto
{
    public int Id { get; set; }
    public string RouteName { get; set; }
    public List<WaypointDto> Waypoints { get; set; } = new();
    public List<TrainStopDto> TrainStops { get; set; } = new();
}