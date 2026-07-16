
namespace Telemetry.Simulator;

public class RailwayRoute
{
    public string RouteName { get; set; }
    public List<(double Lat, double Lon)> Waypoints { get; set; }
}