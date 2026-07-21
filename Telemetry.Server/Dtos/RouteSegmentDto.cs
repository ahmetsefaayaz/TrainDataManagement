using Telemetry.Server.Models;

namespace Telemetry.Server.Dtos;

public class RouteSegmentDto
{
    public bool IsGap { get; set; }
    public List<Waypoint> Coordinates { get; set; } = new List<Waypoint>();
}