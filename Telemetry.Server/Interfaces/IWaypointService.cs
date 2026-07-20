using Telemetry.Server.Dtos;

namespace Telemetry.Server.Interfaces;

public interface IWaypointService
{
    Task<IEnumerable<WaypointDto>> GetWaypoints();
}