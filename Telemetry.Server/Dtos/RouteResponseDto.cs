using Telemetry.Server.Models;

namespace Telemetry.Server.Dtos;

public class RouteResponseDto
{
    public List<RouteSegmentDto> Segments { get; set; } = new List<RouteSegmentDto>();
    public List<TrainLocationRecord> TelemetryData { get; set; } = new List<TrainLocationRecord>();
}