using Telemetry.Server.Dtos;
using Telemetry.Server.Models;

namespace Telemetry.Server.Interfaces;

public interface ITelemetryService
{
    Task<RouteResponseDto> GetData(DateTime startDate, DateTime endDate, short locomotiveId);
}