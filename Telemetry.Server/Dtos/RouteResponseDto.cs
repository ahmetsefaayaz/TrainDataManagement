using Telemetry.Server.Models;

namespace Telemetry.Server.Dtos;

public class RouteResponseDto
{
    public List<Waypoint> SmoothedPath { get; set; } = new List<Waypoint>();
        
    public List<TrainLocationRecord> TelemetryData { get; set; } = new List<TrainLocationRecord>();
}